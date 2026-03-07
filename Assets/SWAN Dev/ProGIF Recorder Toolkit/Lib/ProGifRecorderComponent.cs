/*
 * Copyright (c) 2015 Thomas Hourdel
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *    1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 
 *    2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 
 *    3. This notice may not be removed or altered from any source
 *    distribution.
 */

/// <summary>
/// Modified by SWAN DEV 2017 ~
/// </summary>

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ThreadPriority = System.Threading.ThreadPriority;

[RequireComponent(typeof(Camera)), DisallowMultipleComponent]
public class ProGifRecorderComponent : MonoBehaviour
{
    //onDurationEnd: you can set something to do at the recordTime finish, 
    //the recorder is still recording, use ProGifRecorder->Stop() to stop
    public Action onDurationEnd = null;

    #region Exposed fields

    // These fields aren't public, the user shouldn't modify them directly as they can't break
    // everything if not used correctly. Use Setup() instead.
    [SerializeField]
    Vector2 m_GifRatio = new Vector2(0, 0);

    [SerializeField]
    bool m_AutoAspect = true;

    [SerializeField, Min(8)]
    int m_Width = 320;

    [SerializeField, Min(8)]
    int m_Height = 200;

    [SerializeField, Range(1, 30)]
    int m_FramePerSecond = 15;

    [SerializeField, Min(-1)]
    int m_Repeat = 0;       //Repeat times, 0: infinte

    [SerializeField, Range(1, 100)]
    int m_Quality = 15;     //1: best quality, 100: fastest

    [SerializeField, Min(0.1f)]
    float m_RecordTime = 3f;

    [SerializeField]
    float m_FrameDelay_Override = 0f;

    /// <summary> The transparent color to hide in the GIF. </summary>
    [SerializeField]
    Color32 m_TransparentColor = new Color32(0, 0, 0, 0);
    /// <summary> The range of RGB value for picking nearby colors of the input color to set as transparent pixels. </summary>
    [SerializeField]
    byte m_TransparentColorRange = 0;

    /// <summary> If 'TRUE', check if any pixels' alpha value equal zero and auto encode GIF with transpareny. </summary>
    [SerializeField]
    bool m_AutoTransparent = false;
    #endregion

    #region Public fields
    /// <summary>
    /// Sets the worker threads priority. This will only affect newly created threads.
    /// </summary>
    public ThreadPriority m_WorkerPriority = ThreadPriority.BelowNormal;

    public ImageRotator.Rotation m_Rotation = ImageRotator.Rotation.None;

    /// <summary>
    /// The playback mode for encoding frames to GIF file.
    /// Available modes including Forward, Backward and Ping-pong mode, Ping-pong mode will double the frames in the created GIF file.
    /// </summary>
    public EncodePlayMode m_EncodePlayMode = EncodePlayMode.Normal;
    public enum EncodePlayMode
    {
        Normal = 0,
        Reverse,
        PingPong,
    }

    /// <summary>
    /// The anti-aliasing level for SRP project(URP, LWRP, HDRP) or RENDER_PLUS mode. Valid value: 1 = OFF, 2, 4, 8 = Best quality but Heaviest.
    /// (To enable this setting, please insert the define symbol 'SDEV_RENDER_PLUS' or 'PRO_GIF_SRP' in the Scripting Define Symbols field of Unity PlayerSettings)
    /// </summary>
    [Range(1, 8)] public int m_AntiAliasingLevel = 1;

    /// <summary>
    /// Limits the number of threads for encoding the GIF. Less or equals 0: use all the CPU threads.
    /// (To check the max threads support on the current device, use UnityEngine.SystemInfo.processorCount)
    /// </summary>
    public int m_MaxNumberOfThreads = 0;

    /// <summary>
    /// Affects the frames capture interval in the camera rendering methods.
    /// </summary>
    public bool m_IgnoreTimeScale = true;      // Default: true;

    /// <summary>
    /// A message to embed in the GIF Comment-Extension. Suitable for including an image description, image credit, or other human-readable metadata such as the GPS location of the image capture.
    /// </summary>
    public string m_Comments = string.Empty;

    /// <summary>
    /// Current state of the recorder.
    /// </summary>
    public ProGifRecorder.RecorderState State { get; private set; }

    /// <summary>
    /// Returns the estimated VRam used (in MB) for recording. 
    /// This is not the storage file size. The storage file size can be retrieved with FileInfo after saved.
    /// </summary>
    public float EstimatedMemoryUse
    {
        get
        {
            float mem = m_FramePerSecond * m_RecordTime;
            mem *= m_Width * m_Height * 4;
            mem /= 1024 * 1024;
            return mem;
        }
    }

    public int Width
    {
        get
        {
            return m_Width;
        }
    }

    public int Height
    {
        get
        {
            return m_Height;
        }
    }

    public bool IsCustomRatio
    {
        get
        {
            return (m_GifRatio.x > 0 && m_GifRatio.y > 0);
        }
    }

    public int FPS
    {
        get
        {
            return m_FramePerSecond;
        }
    }

    public Queue<RenderTexture> Frames
    {
        get
        {
            return _qFrames;
        }
    }
    #endregion


    #region Delegates

    /// <summary>
    /// Called when the pre-processing step has finished.
    /// </summary>
    public Action OnPreProcessingDone;

    /// <summary>
    /// Called by each worker thread every time a frame is processed during the save process.
    /// The first parameter holds the worker ID and the second one a value in range [0;1] for
    /// the actual progress. This callback is probably not thread-safe, use at your own risks.
    /// </summary>
    public Action<int, float> OnFileSaveProgress;

    /// <summary>
    /// Called once a gif file has been saved. The first parameter will hold the worker ID and
    /// the second one the absolute file path.
    /// </summary>
    public Action<int, string> OnFileSaved;

    /// <summary>
    /// Called once the gif file byte array has been created. 
    /// </summary>
    public Action<byte[]> OnGifBytesReceive;

    #endregion

    #region Internal fields

    public float RecordProgress
    {
        get
        {
            return (float)_framesRecorded / _maxFrameCount;
        }
    }
    private Action<float> _OnRecordAction = null;
    public void SetOnRecordAction(Action<float> onRecordAction)
    {
        _OnRecordAction = onRecordAction;
    }

    private Camera _rCamera = null;
    private System.Diagnostics.Stopwatch _stopwatch;
    private Queue<RenderTexture> _qFrames;
    private RenderTexture _recycledRenderTexture;

    private float _time;
    private float _timePerFrame;
    private int _maxFrameCount;
    private int _workerId = 0;
    private int _framesRecorded = 0;
    private float _progress = 0.0f;
    private string _fullFilePath = string.Empty, _subFolder_WebGL;
    private bool _invokeFileProgress = false;
    private bool _invokeFileSaved = false;

    private List<ProGifWorker> _workers;
    private int _numberOfThreads = 1;
    private int _framesFinished;
    #endregion

    #region Public API

    /// <summary>
    /// Force assign frames(RenderTexture) to this recorder. This will clear all previous stored frames first.
    /// </summary>
    /// <param name="renderTextures">RenderTexture list(FIFO).</param>
    public void ForceSetFrames(Queue<RenderTexture> renderTextures)
    {
        FlushMemory();
        _qFrames = renderTextures;
    }

    /// <summary>
    /// Initializes the component. Use this if you need to change the recorder settings in a script.
    /// This will flush the previously saved frames as settings can't be changed while recording.
    /// </summary>
    /// <param name="autoAspect">Automatically compute height from the current aspect ratio</param>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="fps">Frames per second</param>
    /// <param name="recorderTime">Maximum amount of seconds to record to memory</param>
    /// <param name="repeat">-1: no repeat, 0: infinite, >0: repeat count</param>
    /// <param name="quality">Quality of color quantization (conversion of images to the maximum
    /// 256 colors allowed by the GIF specification). Lower values (minimum = 1) produce better
    /// colors, but slow processing significantly. Higher values will speed up the quantization
    /// pass at the cost of lower image quality (maximum = 100).</param>
    public void Setup(bool autoAspect, int width, int height, int fps, float recorderTime, int repeat, int quality)
    {
        _Setup(autoAspect, width, height, fps, recorderTime, repeat, quality, new Vector2(0, 0));
    }

    /// <summary>
    /// Initializes the component. Use this if you need to change the recorder settings in a script.
    /// This will flush the previously saved frames as settings can't be changed while recording.
    /// (Use this Setup if you need to crop the image to a specify aspect ratio. 
    /// The pixels out of the provided aspect ratio will be cut.)
    /// </summary>
    /// <param name="gifAspectRatio">Image ratio, 1:1, 16:9, 4:3, 3:2, etc. Use autoAspect if x or y of gifAspectRatio not greater than 0.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="fps">Frames per second</param>
    /// <param name="recorderTime">Maximum amount of seconds to record to memory</param>
    /// <param name="repeat">-1: no repeat, 0: infinite, >0: repeat count</param>
    /// <param name="quality">Quality of color quantization (conversion of images to the maximum
    /// 256 colors allowed by the GIF specification). Lower values (minimum = 1) produce better
    /// colors, but slow processing significantly. Higher values will speed up the quantization
    /// pass at the cost of lower image quality (maximum = 100).</param>
    public void Setup(Vector2 gifAspectRatio, int width, int height, int fps, float recorderTime, int repeat, int quality)
    {
        _Setup(false, width, height, fps, recorderTime, repeat, quality, gifAspectRatio);
    }

    private void _Setup(bool autoAspect, int width, int height, int fps, float recorderTime, int repeat, int quality, Vector2 gifAspectRatio)
    {
        if (State == ProGifRecorder.RecorderState.PreProcessing)
        {
            Debug.LogWarning("Attempting to setup the component during the pre-processing step.");
            return;
        }

        // Start fresh
        FlushMemory();

        // Set values and validate them
        SetGifAspectRatio(gifAspectRatio);
        m_AutoAspect = IsCustomRatio ? true : autoAspect;
        SetSize(ref width, ref height);

        m_FramePerSecond = Mathf.Clamp(fps, 1, 30);
        m_RecordTime = Mathf.Clamp(recorderTime, 0.1f, Mathf.Infinity);
        m_Repeat = (int)Mathf.Clamp(repeat, -1, Mathf.Infinity);
        m_Quality = Mathf.Clamp(quality, 1, 100);

        Init();
    }

    private void SetSize(ref int targetWidth, ref int targetHeight)
    {
        targetWidth = (int)Mathf.Clamp(targetWidth, 8, Mathf.Infinity);
        if (m_AutoAspect)
        {
            float deviceAspect = (Camera.main != null) ? Camera.main.aspect : (float)Screen.width / Screen.height;
            if (deviceAspect < 1f)
            {
                targetHeight = (int)Mathf.Clamp(Mathf.RoundToInt(targetWidth / deviceAspect), 8, Mathf.Infinity);
            }
            else
            {
                targetWidth = (int)Mathf.Clamp(Mathf.RoundToInt(targetWidth * deviceAspect), 8, Mathf.Infinity);
                targetHeight = (int)Mathf.Clamp(Mathf.RoundToInt(targetWidth / deviceAspect), 8, Mathf.Infinity);
            }
        }
        else
        {
            targetHeight = (int)Mathf.Clamp(targetHeight, 8, Mathf.Infinity);
        }
        m_Width = targetWidth;
        m_Height = targetHeight;
    }

    /// <summary>
    /// Sets the GIF images encoding quality, you should set this value before start to Save the GIF.
    /// (Quality 1 = best, 100 = fastest)
    /// </summary>
	public void SetOverrideQuality(int quality)
    {
        m_Quality = Mathf.Clamp(quality, 1, 100);
    }

    /// <summary>
    /// Set the new frame delay to override the FPS(m_FramePerSecond).
    /// * Change during/after PreProcessing state will not be applied.
    /// </summary>
    public void SetOverrideFrameDelay(float frameDelayInSeconds)
    {
        if (State == ProGifRecorder.RecorderState.PreProcessing)
        {
            Debug.LogWarning("Attempting to set gif (override) frame delay during the pre-processing step.");
            return;
        }
        m_FrameDelay_Override = frameDelayInSeconds;
    }

    /// <summary>
    /// Sets the transparent color, hide this color in the GIF. 
    /// The GIF specification allows setting a color to be transparent. 
    /// *** Use case: if you want to record gameObject, character or anything else with transparent background, 
    /// please make sure the background is of solid color(no gradient), and the target object do not contain this color.
    /// (Also be reminded, the transparent feature takes more time for encoding the GIF)
    /// </summary>
    /// <param name="color32">The Color to hide in the gif. Make sure the alpha value greater than Zero, else disable the transparent feature.</param>
    /// <param name="transparentColorRange">The range of RGB value for picking nearby colors of the input color to set as transparent pixels.</param>
    public void SetTransparent(Color32 color32, byte transparentColorRange)
    {
        if (State == ProGifRecorder.RecorderState.PreProcessing)
        {
            Debug.LogWarning("Attempting to set gif transparent during the pre-processing step.");
            return;
        }
        m_TransparentColor = color32;
        m_TransparentColorRange = transparentColorRange;
    }

    /// <summary>
    /// Auto detects the input image(s) pixels for enable/disable transparent feature. 
    /// *** Use case: for pre-made images that have transparent pixels manually set.
    /// (Also be reminded, the transparent feature takes more time to encoding the GIF)
    /// </summary>
    /// <param name="autoDetectTransparent">If set to <c>true</c> auto detect transparent pixels to enable the transparent feature, else disable the auto detection.</param>
    public void SetTransparent(bool autoDetectTransparent)
    {
        if (State == ProGifRecorder.RecorderState.PreProcessing)
        {
            Debug.LogWarning("Attempting to set gif transparent during the pre-processing step.");
            return;
        }
        m_AutoTransparent = autoDetectTransparent;
    }

    private bool transparentExtraMode = false;
    public void SetTransparentExtras(bool enableExtraSettings)
    {
        if (_rCamera == null) _rCamera = GetComponent<Camera>();
        transparentExtraMode = enableExtraSettings;
    }

    public void SetTransparentExtras(bool enableExtraSettings, int targetWidth, int targetHeight)
    {
        SetTransparentExtras(enableExtraSettings);

#if SDEV_RENDER_PLUS || PRO_GIF_SRP
        // Do nothing
#else
        int screenW = Screen.width;
        int screenH = Screen.height;

        if (targetWidth > screenW) targetWidth = screenW;
        if (targetHeight > screenH) targetHeight = screenH;

        SetSize(ref targetWidth, ref targetHeight);

        _rCamera.rect = new Rect(0, 0, (float)targetWidth / screenW, (float)targetHeight / screenH);
#endif
    }

    /// <summary>
    /// Set the GIF rotation (Support rotate 0, -90, 90, 180 degrees).
    /// * Change during/after PreProcessing state will not be applied.
    /// </summary>
    /// <param name="rotation">Rotation. 0, -90, 90, 180</param>
    public void SetGifRotation(ImageRotator.Rotation rotation)
    {
        if (State == ProGifRecorder.RecorderState.PreProcessing)
        {
            Debug.LogWarning("Attempting to set gif rotation during the pre-processing step.");
            return;
        }
        m_Rotation = rotation;
    }

    /// <summary>
    /// Set the gif aspect ratio.
    /// * Change during/after PreProcessing state will not be applied.
    /// </summary>
    /// <param name="gifAspectRatio">Aspect ratio for gif.</param>
    public void SetGifAspectRatio(Vector2 gifAspectRatio)
    {
        if (State == ProGifRecorder.RecorderState.PreProcessing)
        {
            Debug.LogWarning("Attempting to set gif aspact ratio during the pre-processing step.");
            return;
        }
        m_GifRatio = gifAspectRatio;
    }

    /// <summary>
    /// Pauses recording.
    /// </summary>
    public void Pause()
    {
        if (State == ProGifRecorder.RecorderState.PreProcessing)
        {
            Debug.LogWarning("Attempting to pause recording during the pre-processing step. The recorder is automatically paused when pre-processing.");
            return;
        }
        else if (State == ProGifRecorder.RecorderState.Stopped)
        {
            Debug.LogWarning("Attempting to pause recording after it has been stopped.");
            return;
        }

        State = ProGifRecorder.RecorderState.Paused;
    }

    /// <summary>
    /// Resumes recording. You can't resume while it's pre-processing data to be saved.
    /// </summary>
    public void Resume()
    {
        if (State == ProGifRecorder.RecorderState.PreProcessing)
        {
            Debug.LogWarning("Attempting to resume recording during the pre-processing step.");
            return;
        }
        else if (State == ProGifRecorder.RecorderState.Stopped)
        {
            Debug.LogWarning("Attempting to resume recording after it has been stopped.");
            return;
        }

        State = ProGifRecorder.RecorderState.Recording;
    }

    /// <summary>
    /// Starts or resumes recording. You can't start or resume while it's pre-processing data to be saved.
    /// </summary>
    public void Record(Action onDurationEnd = null)
    {
        this.onDurationEnd = onDurationEnd;

        if (State == ProGifRecorder.RecorderState.PreProcessing)
        {
            Debug.LogWarning("Attempting to start recording during the pre-processing step.");
            return;
        }
        else if (State == ProGifRecorder.RecorderState.Stopped)
        {
            Debug.LogWarning("Attempting to start recording after it has been stopped.");
            return;
        }

        m_AntiAliasingLevel = Mathf.Clamp(m_AntiAliasingLevel, 1, 8);

        State = ProGifRecorder.RecorderState.Recording;
    }

    /// <summary>
    /// Stops the recording. You can't resume the record after it has been stopped. You can save gif or start a new recording.
    /// </summary>
    public void Stop()
    {
        State = ProGifRecorder.RecorderState.Stopped;
    }

    /// <summary>
    /// Clears all saved frames from memory and starts fresh.
    /// </summary>
    public void FlushMemory()
    {
        if (State == ProGifRecorder.RecorderState.PreProcessing)
        {
            Debug.LogWarning("Attempting to flush memory during the pre-processing step.");
            return;
        }

        Init();

        if (_recycledRenderTexture != null) Flush(_recycledRenderTexture);

        if (_qFrames == null) return;

        foreach (RenderTexture rt in _qFrames)
        {
            Flush(rt);
        }

        _qFrames.Clear();
    }

    /// <summary>
    /// Saves the stored frames to a gif file (filename will automatically be generated). Recording will be paused and won't resume automatically.
    /// By default, the GIF will save to persistentDataPath directory(or project dataPath if in the editor).
    /// The save path will return in the OnFileSaved callback, you can use it to move the GIF to other places with System.IO methods or use a native plugin to save it to the device gallery.
    /// Use the 'OnPreProcessingDone' callback to be notified when the pre-processing step has finished.
    /// </summary>
    public void Save()
    {
        Save(FilePathName.Instance.GetGifFileName());
    }

    /// <summary>
    /// Saves the stored frames to a gif file. Recording will be paused and won't resume automatically.
    /// By default, the GIF will save to persistentDataPath directory(or project dataPath if in the editor).
    /// The save path will return in the OnFileSaved callback, you can use it to move the GIF to other places with System.IO methods or use a native plugin to save it to the device gallery.
    /// Use the 'OnPreProcessingDone' callback to be notified when the pre-processing step has finished.
    /// </summary>
    /// <param name="gifFileName">The GIF filename without extension. If filename is null or empty, an unique one will be generated.</param>
    /// <param name="subFolderName">The sub-folder for saving the GIF.</param>
    public void Save(string gifFileName, string subFolderName = "")
    {
        if (State == ProGifRecorder.RecorderState.PreProcessing)
        {
            Debug.LogWarning("Attempting to save during the pre-processing step.");
            return;
        }

        if (_qFrames.Count == 0)
        {
            Debug.LogWarning("Nothing to save. Maybe you forgot to start the recorder?");
            return;
        }

        string directory = FilePathName.Instance.GetSaveDirectory();
        if (!string.IsNullOrEmpty(subFolderName)) directory = Path.Combine(directory, subFolderName);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        string filename = string.IsNullOrEmpty(gifFileName) ? FilePathName.Instance.GetGifFileName() : gifFileName;

        _fullFilePath = Path.Combine(directory, filename + ".gif");
        _subFolder_WebGL = subFolderName;

        State = ProGifRecorder.RecorderState.PreProcessing;
        StartCoroutine(PreProcess());
    }

    /// <summary>
    /// Saves the stored frames to a gif file and/or receive the gif bytes in the callback. Recording will be paused and won't resume automatically.
    /// By default, the GIF will save to persistentDataPath directory(or project dataPath if in the editor).
    /// The save path will return in the OnFileSaved callback, you can use it to move the GIF to other places with System.IO methods or use a native plugin to save it to the device gallery.
    /// Use the 'OnPreProcessingDone' callback to be notified when the pre-processing step has finished.
    /// </summary>
    /// <param name="onGifBytesReceive">Returns the encoded GIF file byte array.</param>
    /// <param name="gifFileName">The GIF filename without extension. If filename is null or empty, the GIF will not be saved in the storage.</param>
    /// <param name="subFolderName">The sub-folder for saving the GIF.</param>
    public void Save(Action<byte[]> onGifBytesReceive, string gifFileName = "", string subFolderName = "")
    {
        if (State == ProGifRecorder.RecorderState.PreProcessing)
        {
            Debug.LogWarning("Attempting to save during the pre-processing step.");
            return;
        }

        if (_qFrames.Count == 0)
        {
            Debug.LogWarning("Nothing to save. Maybe you forgot to start the recorder?");
            return;
        }

        if (string.IsNullOrEmpty(gifFileName))
        {
            _fullFilePath = null;
        }
        else
        {
            string directory = FilePathName.Instance.GetSaveDirectory();
            if (!string.IsNullOrEmpty(subFolderName)) directory = Path.Combine(directory, subFolderName);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            _fullFilePath = Path.Combine(directory, gifFileName + ".gif");
            _subFolder_WebGL = subFolderName;
        }

        OnGifBytesReceive = onGifBytesReceive;

        State = ProGifRecorder.RecorderState.PreProcessing;
        StartCoroutine(PreProcess());
    }

    #endregion


    #region Unity events

    private void Awake()
    {
        _qFrames = new Queue<RenderTexture>();
        Init();
    }

    private void Update()
    {
        if (_invokeFileProgress)
        {
            _invokeFileProgress = false;
            OnFileSaveProgress(_workerId, _progress);
        }

        if (_invokeFileSaved)
        {
#if UNITY_EDITOR
            Debug.Log("invokeFileSaved - workerId: " + _workerId + " filePath: " + _fullFilePath);
#endif
            _invokeFileSaved = false;

            if (OnGifBytesReceive != null) OnGifBytesReceive(_gifBytes);

            OnFileSaved(_workerId, _fullFilePath);
        }

#if SDEV_RENDER_PLUS || PRO_GIF_SRP
        StartCoroutine(OnUpdateRender());
#endif
    }

    private void OnDestroy()
    {
        FlushMemory();
    }

#if SDEV_RENDER_PLUS || PRO_GIF_SRP
    private IEnumerator OnUpdateRender()
    {
        if (State != ProGifRecorder.RecorderState.Recording)
        {
            yield break; //return
        }

        if (_rCamera == null) _rCamera = GetComponent<Camera>();

        yield return new WaitForEndOfFrame();

        _time += m_IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

        if (_time >= _timePerFrame)
        {
            if (_OnRecordAction != null) _OnRecordAction(RecordProgress);

            // Limit the amount of frames stored in memory
            if (_qFrames.Count >= _maxFrameCount)
            {
                _recycledRenderTexture = _qFrames.Dequeue();
                if (onDurationEnd != null)
                {
                    onDurationEnd();
                    onDurationEnd = null;
                }
            }

            _time -= _timePerFrame;

            if (transparentExtraMode)
            {
                if (m_AntiAliasingLevel != 1) m_AntiAliasingLevel = 1; //turn off Anti-Aliasing
            }

            // Frame data
            RenderTexture rt = _recycledRenderTexture;
            _recycledRenderTexture = null;

            if (rt == null)
            {
                rt = new RenderTexture(m_Width, m_Height, 24, RenderTextureFormat.ARGB32);
                rt.antiAliasing = m_AntiAliasingLevel;
                rt.wrapMode = TextureWrapMode.Clamp;
                rt.filterMode = FilterMode.Bilinear;
                rt.anisoLevel = 0;
            }
            rt.DiscardContents();

            _rCamera.targetTexture = rt;
            _rCamera.Render();
            _rCamera.targetTexture = null;

            _framesRecorded++;
            _qFrames.Enqueue(rt);
        }
    }
#else
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (State != ProGifRecorder.RecorderState.Recording)
        {
            Graphics.Blit(source, destination);
            return;
        }

        _time += m_IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

        if (_time >= _timePerFrame)
        {
            if (_OnRecordAction != null) _OnRecordAction(RecordProgress);

            // Limit the amount of frames stored in memory
            if (_qFrames.Count >= _maxFrameCount)
            {
                _recycledRenderTexture = _qFrames.Dequeue();
                if (onDurationEnd != null)
                {
                    onDurationEnd();
                    onDurationEnd = null;
                }
            }

            _time -= _timePerFrame;

            // Special setup for recording Transparent GIFs : no scaling, becareful of too large texture size, you can reduce the recorder Camera viewport size to reduce the source texture size,
            // e.g. for a 1080 * 1920 physical screen, set the W & H to 0.5 in the Camera viewport, the source texture size becomes 540 * 960. 
            // ** For better transparent result(reduce border on the edges), Anti-Aliasing should turn OFF.
            // ** The source texture size depends on device screen resolution, set the recorder camera viewport W & H to set the GIF image size.
            if (transparentExtraMode)
            {
                m_Width = source.width;
                m_Height = source.height;
                //if(QualitySettings.antiAliasing != 1) QualitySettings.antiAliasing = 1;
            }

            // Frame data
            RenderTexture rt = _recycledRenderTexture;
            _recycledRenderTexture = null;

            if (rt == null)
            {
                rt = new RenderTexture(m_Width, m_Height, 24, RenderTextureFormat.ARGB32);
                rt.wrapMode = TextureWrapMode.Clamp;
                rt.filterMode = FilterMode.Bilinear;
                rt.anisoLevel = 0;
            }
            rt.DiscardContents();

            Graphics.Blit(source, rt);

            _framesRecorded++;
            _qFrames.Enqueue(rt);
        }

        Graphics.Blit(source, destination);
    }
#endif

    #endregion

    #region Methods

    // Used to reset internal values, called on Start(), Setup() and FlushMemory()
    private void Init()
    {
        State = ProGifRecorder.RecorderState.Paused;
        ComputeHeight();
        _maxFrameCount = Mathf.RoundToInt(m_RecordTime * m_FramePerSecond);
        _timePerFrame = 1f / m_FramePerSecond;
        _time = 0f;
    }

    public bool HasRecorded()
    {
        return _qFrames != null && _qFrames.Count > 0;
    }

    // Automatically computes height from the current aspect ratio if auto aspect is set to true
    public void ComputeHeight()
    {
        if (!m_AutoAspect || IsCustomRatio) return;

        if (Camera.main != null)
        {
            m_Height = Mathf.RoundToInt(m_Width / Camera.main.aspect);
        }
    }

    public void ComputeCropSize()
    {
        if (IsCustomRatio)
        {
            float frameRatio = (float)m_Width / m_Height;
            float gifRatio = m_GifRatio.x / m_GifRatio.y;

            if (frameRatio > gifRatio)
            {
                if (gifRatio.Equals(1f))
                {
                    //for 1:1 gif
                    if (m_Width > m_Height)
                    {
                        m_Width = m_Height;
                    }
                    else if (m_Height > m_Width)
                    {
                        m_Height = m_Width;
                    }
                }
                else
                {
                    //m_Height remain unchange, cal m_Width with gif ratio
                    m_Width = (int)(m_Height * gifRatio);
                }
            }
            else if (frameRatio < gifRatio)
            {
                if (gifRatio.Equals(1f))
                {
                    //for 1:1 gif
                    if (m_Width > m_Height)
                    {
                        m_Width = m_Height;
                    }
                    else if (m_Height > m_Width)
                    {
                        m_Height = m_Width;
                    }
                }
                else
                {
                    //m_Width remain unchange, cal m_Height with gif ratio
                    m_Height = (int)(m_Width / gifRatio);
                }
            }
            else
            {
                //Do nothing
            }
        }
    }

    // Flushes a single Texture object
    private void Flush(Texture texture)
    {
        if (RenderTexture.active == texture) return;
        Texture2D.Destroy(texture);
    }

    /// <summary>
    /// Pre-processing coroutine to extract frame data and send everything to a separate worker thread
    /// </summary>
    IEnumerator PreProcess()
    {
        List<Frame> frames = new List<Frame>(_qFrames.Count);

        // Recaculate image Width & Height if gifAspectRatio is used
        ComputeCropSize();

        // Get a temporary texture to read RenderTexture data
        Texture2D temp = new Texture2D(m_Width, m_Height, TextureFormat.RGB24, false);
        temp.hideFlags = HideFlags.HideAndDontSave;
        temp.wrapMode = TextureWrapMode.Clamp;
        temp.filterMode = FilterMode.Point;
        temp.anisoLevel = 0;

        // Process the frame queue
        RenderTexture[] textures = _qFrames.ToArray();

        foreach (RenderTexture rt in textures)
        {
            yield return 0;
            Frame frame = ToGifFrame(rt, temp);
            frames.Add(frame);
        }

        switch (m_EncodePlayMode)
        {
            case EncodePlayMode.Reverse:
                frames.Reverse();
                break;
            case EncodePlayMode.PingPong:
                int T = frames.Count;
                for (int i = 0; i < T; i++) frames.Add(frames[T - i - 1]);
                break;
        }

        frameCountFinal = frames.Count;

        // Dispose the temporary texture
        Flush(temp);

        // Switch the state to pause, let the user choose to keep recording or not
        State = ProGifRecorder.RecorderState.Paused;

        // Callback
        if (OnPreProcessingDone != null) OnPreProcessingDone();


        // Multithreaded encoding jobs starts here.
        _numberOfThreads = Mathf.Min(frameCountFinal, m_MaxNumberOfThreads < 1 ? SystemInfo.processorCount : m_MaxNumberOfThreads);
#if UNITY_EDITOR
        Debug.Log("Number of threads: " + _numberOfThreads);
#endif

        _stopwatch = new System.Diagnostics.Stopwatch();
        _stopwatch.Start();

        _framesFinished = 0;
        jobsFinished = 0;

        // Split frames
        _workers = new List<ProGifWorker>();
        List<Frame>[] framesArray = new List<Frame>[_numberOfThreads];

        int framesOnEachThread = Mathf.FloorToInt((float)frameCountFinal / (float)_numberOfThreads);
        int leftOverFrames = frameCountFinal % _numberOfThreads;

        int startIndex = 0;
        for (int threadIndex = 0; threadIndex < _numberOfThreads; threadIndex++)
        {
            int leftOverFrameAvg = (leftOverFrames > 0 ? 1 : 0);
#if UNITY_EDITOR
            Debug.Log("ThreadIndex-" + threadIndex + ": " + startIndex + "-" + (startIndex + framesOnEachThread + leftOverFrameAvg - 1) + ", Total: " + frameCountFinal);
#endif
            framesArray[threadIndex] = new List<Frame>();
            for (int i = startIndex; i < startIndex + framesOnEachThread + leftOverFrameAvg; i++)
            {
                framesArray[threadIndex].Add(frames[i]);
            }
            //The leftover frames are added to the first thread.
            startIndex += framesOnEachThread + leftOverFrameAvg;
            if (leftOverFrames > 0) leftOverFrames--;
        }

        for (int i = 0; i < _numberOfThreads; i++)
        {
            // Setup a worker thread for GIF encoding and save file -----------------
            ProGifEncoder encoder = new ProGifEncoder(m_Repeat, m_Quality, i, EncoderFinished);

            encoder.m_Comments = m_Comments;

            if (m_AutoTransparent)
            {
                encoder.m_AutoTransparent = m_AutoTransparent;
            }
            else if (m_TransparentColor.a != 0)
            {
                encoder.SetTransparencyColor(m_TransparentColor, m_TransparentColorRange);
            }

            // Check if apply the Override Frame Delay value
            if (m_FrameDelay_Override > 0f)
            {
                encoder.SetDelay(Mathf.RoundToInt(m_FrameDelay_Override * 1000f));
            }
            else
            {
                encoder.SetDelay(Mathf.RoundToInt(_timePerFrame * 1000f));
            }

#if UNITY_WEBGL
            ProGifWorker worker = new ProGifWorker(i)
#else
            ProGifWorker worker = new ProGifWorker(i, m_WorkerPriority)
#endif
            {
                m_Encoder = encoder,
                m_Frames = framesArray[i],
                m_OnFileSaveProgress = FileSaveProgress
            };

            _workers.Add(worker);

            // Make sure only the first encoder writes the beginning
            worker.m_Encoder.m_IsFirstFrame = i == 0;

            // Make sure only the last encoder appends the trail.
            worker.m_Encoder.m_IsLastEncoder = i == _numberOfThreads - 1;
#if UNITY_WEBGL
            StartCoroutine(worker.Start_Corountine());
#else
            worker.Start();
#endif
        }
    }

    private byte[] _gifBytes;
    private void FileSaved(int id, byte[] bytes)
    {
        this._workerId = id;
        this._gifBytes = bytes;
        this._invokeFileSaved = true;
    }

    private int frameCountFinal;
    private void FileSaveProgress(int id)
    {
        this._workerId = id;
        this._invokeFileProgress = true;
        this._framesFinished++;
        this._progress = (float)_framesFinished / frameCountFinal;
    }

    private int jobsFinished;
    private void EncoderFinished(int encoderIndex)
    {
        jobsFinished++;
        if (jobsFinished == _numberOfThreads)
        {
            JobsFinished();
        }
    }

    private void JobsFinished()
    {
        MemoryStream newStream = new MemoryStream( );
        for (int i = 0; i < _workers.Count; i++)
        {
            MemoryStream stream = _workers[i].m_Encoder.GetMemoryStream();
            stream.Position = 0;
            stream.WriteTo(newStream);
            stream.Close();
        }
        byte[] gifBytes = newStream.GetBuffer();
        newStream.Close();

        if (!string.IsNullOrEmpty(_fullFilePath))
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SDev.EasyIO.DeleteFile(Path.GetFileName(_fullFilePath), _subFolder_WebGL);
            _fullFilePath = SDev.EasyIO.SaveBytes(gifBytes, Path.GetFileName(_fullFilePath), _subFolder_WebGL); // WebGL supports save to persistent data path only
#else
            if (File.Exists(_fullFilePath)) File.Delete(_fullFilePath);
            File.WriteAllBytes(_fullFilePath, gifBytes);
#endif
        }

        FileSaved(_workerId, gifBytes);

#if UNITY_EDITOR
        Debug.Log("All jobs finished, total encode time: " + _stopwatch.Elapsed);
#endif
        _stopwatch.Stop();
    }

    /// <summary>
    /// Converts a RenderTexture to a GifFrame
    /// Should be fast enough for low-res textures but will tank the framerate at higher res
    /// </summary>
    private Frame ToGifFrame(RenderTexture source, Texture2D target)
    {
        RenderTexture.active = source;

        if (IsCustomRatio) target.ReadPixels(new Rect((source.width - target.width) / 2, (source.height - target.height) / 2, target.width, target.height), 0, 0);
        else target.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        //target.Apply(); // unnecessary
        RenderTexture.active = null;

        // Rotate Image ---------------------
        int newWidth = target.width;
        int newHeight = target.height;

        if (m_Rotation == ImageRotator.Rotation.None)
        {
            return new Frame() { Width = newWidth, Height = newHeight, Data = target.GetPixels32() };
        }
        else
        {
            Color32[] colors;

            switch (m_Rotation)
            {
                case ImageRotator.Rotation.Right: //90
                    newWidth = target.height;
                    newHeight = target.width;
                    break;
                case ImageRotator.Rotation.HalfCircle: //180
                    break;
                case ImageRotator.Rotation.Left: //-90
                    newWidth = target.height;
                    newHeight = target.width;
                    break;
            }

            // Rotate Image (Supports rotate with interval of 90 degrees)
            colors = ImageRotator.RotateImageToColor32(target, m_Rotation);
            return new Frame() { Width = newWidth, Height = newHeight, Data = colors };
        }
    }

    /// <summary>
    /// Clear and Remove this script from camera
    /// </summary>
    public void ClearAndRemoveScript()
    {
        OnDestroy();
        Destroy(this);
    }

#endregion

}
