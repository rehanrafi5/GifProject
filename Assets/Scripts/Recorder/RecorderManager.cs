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
    StartCoroutine(SyncAndDownload(path));
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
    IEnumerator SyncAndDownload(string path)
    {
        yield return new WaitForEndOfFrame(); // allow FS write to complete

        byte[] bytes = _recorder.GetBytes();
        SaveGifWebGL(bytes, "litkit");
    }
#if UNITY_WEBGL && !UNITY_EDITOR
[System.Runtime.InteropServices.DllImport("__Internal")]
private static extern void DownloadFile(string fileName, byte[] data, int length);
#endif

    public void SaveGifWebGL(byte[] gifBytes, string fileName)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    DownloadFile(fileName, gifBytes, gifBytes.Length);
#else
        Debug.Log("WebGL download only works in WebGL build");
#endif
    }
}
