using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class RecorderManagerWebGL : MonoBehaviour
{
    public static RecorderManagerWebGL Instance;

    [SerializeField] private Texture2D sdsd;
    
    [SerializeField] private CodelessProGifRecorder _recorder;
    [SerializeField] private GameObject _successBanner;
    
    private ConfirmationPopup _confirmationPopup;
    private LoadingPopup _loadingPopup;
    
    private string _filename;

    private bool _recordStarted;

    /// <summary>
    /// Start recording GIF for WebGL.
    /// </summary>
    /// <param name="width">Target GIF width in pixels.</param>
    /// <param name="height">Target GIF height in pixels.</param>
    /// <param name="duration">Duration in seconds.</param>
    /// <param name="fps">Frames per second.</param>
    private void Awake()
    {
        Instance = this;
    }

    
    
    private void Start()
    {
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
                _recordStarted = false;
                GIFManager.Instance.OnStop();
                _loadingPopup.Hide();
                _successBanner.SetActive(true);
            }
        }
    }
    public void OnFileSaved(byte[] gifBytes)
    {
        Debug.Log(gifBytes.Length);
    }
// #if UNITY_WEBGL && !UNITY_EDITOR
// [DllImport("__Internal")]
// private static extern void DownloadGifFile(byte[] data, int length, string fileName);
// #endif
//     public void DownloadGif(byte[] gifBytes, string filename)
//     {
// #if UNITY_WEBGL && !UNITY_EDITOR
//     // if (gifBytes == null || gifBytes.Length == 0)
//     // {
//     //     Debug.LogError("GIF bytes are empty — aborting download");
//     //     return;
//     // }
//     //
//     // if (!filename.ToLower().EndsWith(".gif"))
//     //     filename += ".gif";
//     //
//     // Debug.Log("Downloading GIF, size: " + gifBytes.Length);
//     //
//     // DownloadGifFile(gifBytes, gifBytes.Length, filename);
//     //     _recorder.m_State = "Idle";
// #else
//         Debug.Log("Download only works in WebGL build");
// #endif
//     }
}
