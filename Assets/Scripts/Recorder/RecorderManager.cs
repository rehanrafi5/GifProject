using System.Collections;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

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

#if UNITY_WEBGL && !UNITY_EDITOR
        _recorder.Rec_Width = width;
        _recorder.Rec_Height = height;
        _recorder.Rec_Fps = Mathf.Clamp(fps, 1, 15); // reduce fps for WebGL
        _recorder.Rec_Duration = duration;
#else
        _recorder.Rec_Width = width;
        _recorder.Rec_Height = Mathf.RoundToInt((float)height / 1.6f);
        _recorder.Rec_Duration = duration;
        _recorder.Rec_Fps = fps;
#endif

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
    public void RecordWebGL(int width, int height, float duration, int fps)
    {
        GIFManager.Instance.OnStart();

        _recorder.Rec_Width = width;
        _recorder.Rec_Height = height;
        _recorder.Rec_Fps = Mathf.Clamp(fps, 1, 15);
        _recorder.Rec_Duration = duration;

        _loadingPopup.SetHeader("GIF Creation");
        _loadingPopup.SetDescription("Recording GIF...");
        _loadingPopup.Show();

        // WebGL: Start recording, and use the internal callback when done
        _recorder.StartRecord();
    }
#if UNITY_WEBGL && !UNITY_EDITOR
public void OnGifReadyWebGL(string path, string optionalName)
{
    string fileName = string.IsNullOrEmpty(optionalName) ? "LitKit.gif" : optionalName;
    if (!fileName.ToLower().EndsWith(".gif")) fileName += ".gif";

    DownloadGif(fileName, path); // JS download
    _loadingPopup.Hide();
    _successBanner.SetActive(true);
    GIFManager.Instance.OnStop();
}

[DllImport("__Internal")]
private static extern void DownloadGif(string fileName, string path);
#endif
    private void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    // On WebGL, ignore progress updates, wait for callback
#else
        if (_recordStarted)
        {
            _loadingPopup.SetDescription($"Creating GIF... {_recorder.m_RecordingProgress}");

            if (_recorder.m_State == "Idle")
            {
                _recordStarted = false;
                HandleNonWebGLGif(_recorder.m_SavePath);
            }
        }
#endif
    }


    private void HandleNonWebGLGif(string path)
    {
        GIFManager.Instance.OnStop();

#if UNITY_ANDROID || UNITY_IOS
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
#else
        Debug.Log("Saved locally at: " + path);
        _loadingPopup.Hide();
        _successBanner.SetActive(true);
#endif
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
}
