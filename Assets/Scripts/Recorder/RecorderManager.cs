using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;

public class RecorderManager : Singleton<RecorderManager>
{
    [SerializeField] private CodelessProGifRecorder _recorder;
    [SerializeField] private GameObject _successBanner;

    private bool _recordStarted;

    private ConfirmationPopup _confirmationPopup;
    private LoadingPopup _loadingPopup;

    private string _filename;

    protected override void Awake()
    {
        base.Awake();

        _loadingPopup = PopupManager.Instance.GetPopup<LoadingPopup>();
        _confirmationPopup = PopupManager.Instance.GetPopup<ConfirmationPopup>();

        _filename = string.IsNullOrEmpty(_recorder.Rec_OptionalFileName) ? "LitKit" : _recorder.Rec_OptionalFileName;

        if (!_filename.ToLower().EndsWith(".gif"))
            _filename += ".gif";
    }

    public void Record(int width, int height, float duration, int fps)
    {
        GIFManager.Instance.OnStart();

        _recorder.Rec_Width = width;
        _recorder.Rec_Height = Mathf.RoundToInt((float)height / 1.6f);
        _recorder.Rec_Duration = duration;
        _recorder.Rec_Fps = fps;

        _recorder.StartRecord();
        _recordStarted = true;

        _loadingPopup.SetHeader("GIF Creation");
        _loadingPopup.SetDescription("Recording GIF...");
        _loadingPopup.Show();

        StartCoroutine(RecordDelay(duration));
    }

    private IEnumerator RecordDelay(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // SaveRecord stops recording and triggers the recorder callback
        _recorder.SaveRecord();
    }

    private void Update()
    {
        if (_recordStarted)
        {
            _loadingPopup.SetDescription($"Creating GIF... {_recorder.m_RecordingProgress}");

            if (_recorder.m_State == "Idle")
            {
                Debug.Log("0");
                _recordStarted = false;
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                HandleWebGLGif(_recorder.m_SavePath);
                #else
                HandleNonWebGLGif(_recorder.m_SavePath);
                #endif
                
            }
        }
    }
    
    private void HandleNonWebGLGif(string path)
    {
        GIFManager.Instance.OnStop();

        NativeGallery.Permission readPermission = CheckPermissions(NativeGallery.PermissionType.Read, path);
        NativeGallery.Permission writePermission = CheckPermissions(NativeGallery.PermissionType.Write, path);

        if (readPermission == NativeGallery.Permission.Granted && writePermission == NativeGallery.Permission.Granted)
        {
            NativeGallery.SaveImageToGallery(path, "LitKit", _filename, OnGallerySavingComplete);
            _loadingPopup.SetDescription("Saving GIF file...");
        }
        else
        {
            _confirmationPopup.SetHeader("Requires Permissions");
            _confirmationPopup.SetDescription("Please give LitKit access to Photos (Read & Write).");
            _confirmationPopup.SetConfirmAction(null, "OK");
            _confirmationPopup.Show();
        }
        Debug.Log("Saved locally at: " + path);
        _loadingPopup.Hide();
        _successBanner.SetActive(true);
    }
    private void HandleWebGLGif(string path)
    {
        GIFManager.Instance.OnStop();

        Debug.Log("1");
#if UNITY_WEBGL && !UNITY_EDITOR
    var recorder = PGif.iGetRecorder("CodelessProGifRecorder");
        Debug.Log("2");
    if (recorder == null)
    {
        Debug.LogError("PGif recorder not found");
        return;
    }
        Debug.Log("3");

    byte[] gifBytes = recorder.GetGif();

    if (gifBytes == null || gifBytes.Length == 0)
    {
        Debug.LogError("GIF bytes are empty");
        return;
    }
        Debug.Log("4");

    DownloadGif(gifBytes, _filename);
#endif
        
        _loadingPopup.SetDescription("Saving GIF file...");
        
        _loadingPopup.Hide();
        _successBanner.SetActive(true);
    }

    private NativeGallery.Permission CheckPermissions(NativeGallery.PermissionType permissionType, string path)
    {
        NativeGallery.Permission permission = NativeGallery.CheckPermission(permissionType);
        if (permission == NativeGallery.Permission.ShouldAsk)
            permission = NativeGallery.RequestPermission(permissionType);
        return permission;
    }
    private void OnGallerySavingComplete(bool success, string path)
    {
        _loadingPopup.Hide();
        _successBanner.SetActive(true);
    }
    
#if UNITY_WEBGL && !UNITY_EDITOR
[DllImport("__Internal")]
private static extern void DownloadGifFile(byte[] data, int length, string fileName);
#endif
    public void DownloadGif(byte[] gifBytes, string filename)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    if (gifBytes == null || gifBytes.Length == 0)
    {
        Debug.LogError("GIF bytes are empty — aborting download");
        return;
    }

    if (!filename.ToLower().EndsWith(".gif"))
        filename += ".gif";

    Debug.Log("Downloading GIF, size: " + gifBytes.Length);

    DownloadGifFile(gifBytes, gifBytes.Length, filename);
        _recorder.m_State = "Idle";
#endif
    }
}
