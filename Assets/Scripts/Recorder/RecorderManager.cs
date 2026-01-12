using System.Collections;
using System.IO;
using UnityEngine;

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

        _filename = _recorder.Rec_OptionalFileName;
        if (_filename == null || _filename == "")
            _filename = "LitKit";

        if (!_filename.ToLower().EndsWith(".gif"))
            _filename += ".gif";
    }

    public void Record(int width, int height, float duration, int fps)
    {
        GIFManager.Instance.OnStart();

        _recorder.Rec_Width = height;
        _recorder.Rec_Height = Mathf.RoundToInt((float) height / 1.6f);
        _recorder.Rec_Duration = duration;
        _recorder.Rec_Fps = fps;
        _recorder.StartRecord();
        _recordStarted = true;

        _loadingPopup.SetHeader("GIF Creation");
        _loadingPopup.SetDescription("Recording GIF... ");
        _loadingPopup.Show();

        StartCoroutine(RecordDelay(duration));
    }

    private IEnumerator RecordDelay(float duration)
    {
        float delay = 0f;

        while(delay < duration)
        {
            delay += Time.deltaTime;
            yield return null;
        }

        SaveRecording();
    }

    private void SaveRecording()
    {
        _recorder.SaveRecord();

        GIFManager.Instance.OnStop();
    }

    private void Update()
    {
        if(_recordStarted)
        {
            if(_recorder.m_State == "Idle")
            {
                _recordStarted = false;
                OnSavingComplete(_recorder.m_SavePath);
            }

            else
            {
                _loadingPopup.SetDescription("Creating GIF file... " + _recorder.m_RecordingProgress.ToString());
            }
        }
    }

    private void OnSavingComplete(string path)
    {
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
            _confirmationPopup.SetDescription("Please go to your Settings to give LitKit access to your Photos. Change the access to Read and Write.");
            _confirmationPopup.SetConfirmAction(null, "OK");
            _confirmationPopup.Show();
        }
    }

    private NativeGallery.Permission CheckPermissions(NativeGallery.PermissionType permissionType, string path)
    {
        NativeGallery.Permission permission = NativeGallery.CheckPermission(permissionType);

        if(permission == NativeGallery.Permission.ShouldAsk)
        {
            permission = NativeGallery.RequestPermission(permissionType);
        }

        return permission;
    }

    private void OnGallerySavingComplete(bool success, string path)
    {
        _loadingPopup.Hide();
        _successBanner.SetActive(true);
        //_confirmationPopup.SetHeader("File Saved");
        //_confirmationPopup.SetDescription("GIF file has been saved on your device's Gallery.");
        //_confirmationPopup.SetConfirmAction(null, "OK");
        //_confirmationPopup.Show();
    }
}