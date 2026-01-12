using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GIFManager : Singleton<GIFManager>
{
    [SerializeField] RenderTexture _cameraRenderTexture;
    [SerializeField] RectTransform _frameTransform;
    [SerializeField] Transform _framesParent;
    [SerializeField] Transform _screenshotBackground;
    //[SerializeField] Material _transparentCutoutMaterial; // DGS Removed Transparent Cutout Material

    private float _currentSpeed = 0.1f;
    private float _currentTime = 0f;
    private float _cameraScale = 1f;

    private List<GameObject> _frames = new List<GameObject>();
    private List<GameObject> _objects = new List<GameObject>();
    private int _frameIndex;
    private bool _isRunning;
    private bool _isHigherRatio;
    private Action<Texture2D> _screenshotCallback;

    public int Width { get; private set; }
    public int Height { get; private set; }

    Rect _rect;

    protected override void Awake()
    {
        base.Awake();

        Height = Mathf.RoundToInt(((float)Screen.width * 10f) / 16f);
        Width = Screen.width;
        _cameraScale = (float) Screen.height / (float) Height;
        _isHigherRatio = false;

        if(Height > Screen.height)
        {
            Height = Screen.height;
            Width = Mathf.RoundToInt(((float)Screen.height * 16f) / 10f);
            _cameraScale = 1f / ((float)Screen.width / (float) Width);
            _isHigherRatio = true;
        }

        if(Height > Width)
        {
            int retainedValue = Height;
            Height = Width;
            Width = retainedValue;
        }

        _cameraRenderTexture.width = Width;
        _cameraRenderTexture.height = Height;
        _cameraRenderTexture.format = RenderTextureFormat.ARGB32;

        _rect = new Rect(0, 0, Width, Height);
    }

    private void Start()
    {
        float anchorDiffX = (1f - _frameTransform.anchorMax.x) / 2f;
        float anchorDiffY = (1f - _frameTransform.anchorMax.x) / 2f;

        _frameTransform.anchorMin = new Vector2(anchorDiffX, anchorDiffY);
        _frameTransform.anchorMax = new Vector2(1f - anchorDiffX, 1f - anchorDiffY);
    }

    private void LateUpdate()
    {
        if(_isRunning)
        {
            _currentTime += Time.deltaTime;

            if(_currentTime > _currentSpeed)
            {
                _currentTime -= _currentSpeed;
                _frames[_frameIndex].SetActive(false);

                _frameIndex++;

                if(_frameIndex >= _frames.Count)
                {
                    _frameIndex = 0;
                }

                _frames[_frameIndex].SetActive(true);
            }
        }
    }

    public void OnStart()
    {
        _frameIndex = _frames.Count - 1; // DGS Last Index
        _currentTime = 0f;
        _frames[_frameIndex].SetActive(true); // DGS Use last index
        _isRunning = true;
    }

    public void OnStop()
    {
        _isRunning = false;
    }

    public void SetSpeed(float speed)
    {
        _currentSpeed = speed;
    }

    public void TakeScreenshot(FrameObject frame, Action<Texture2D> callback)
    {
        Clear();

        _screenshotCallback = callback;
        float scale = FrameManager.Instance.FrameScale.x * _cameraScale;
        _screenshotBackground.gameObject.SetActive(true);

        //GameObject frameInstance = Instantiate(frame, _framesParent);
        FrameObject frameInstance = PoolManager.Instance.TakeGIFFrame();
        frameInstance.transform.SetParent(_framesParent);
        BuildChildObjects(frame, frameInstance);

        frameInstance.transform.localScale = new Vector3(scale, scale, 1f);
        _frames.Add(frameInstance.gameObject);
        frameInstance.gameObject.SetActive(true);

        StopAllCoroutines(); // stop it first in case it is still running
        if (frame.FrameButton.IsTextureSet)
        {
            StartCoroutine(ScreenshotFrame(frame.FrameButton.CaptureTexture));
        }
        else
        {
            frame.FrameButton.SetupTexture(Width, Height, TextureFormat.ARGB32, false);
            StartCoroutine(ScreenshotFrame(frame.FrameButton.CaptureTexture));
        }
    }

    private IEnumerator ScreenshotFrame(Texture2D texture2D)
    {
        yield return new WaitForEndOfFrame();

        RenderTexture.active = _cameraRenderTexture;

        texture2D.ReadPixels(_rect, 0, 0);
        texture2D.Apply();

        var bytes = ImageConversion.EncodeToJPG(texture2D, 25);

        texture2D.LoadImage(bytes);

        RenderTexture.active = null;

        _screenshotCallback?.Invoke(texture2D);
    }

    public void SetFrames(List<FrameObject> frames)
    {
        Clear();

        float scale = FrameManager.Instance.FrameScale.x * _cameraScale;

        _screenshotBackground.gameObject.SetActive(false);

        for(int i = 0; i < frames.Count; i++)
        {
            //GameObject frameInstance = Instantiate(frames[i], _framesParent);
            FrameObject frameInstance = PoolManager.Instance.TakeGIFFrame();
            frameInstance.transform.SetParent(_framesParent);
            BuildChildObjects(frames[i], frameInstance);

            frameInstance.transform.localScale = new Vector3(_isHigherRatio ? FrameManager.Instance.FrameScale.y * _cameraScale : scale, _isHigherRatio ? scale : FrameManager.Instance.FrameScale.x, 1f);
            _frames.Add(frameInstance.gameObject);

            // DGS - Commented this; It was causing the flipped images to disappear on saving the GIF
            //List<Image> images = frameInstance.GetComponentsInChildren<Image>(true).ToList();
            //for(int j = 0; j < images.Count; j++)
            //{
            //    images[j].material = _transparentCutoutMaterial;
            //}

            frameInstance.gameObject.SetActive(false);
        }
    }

    private void BuildChildObjects(FrameObject fromFrame, FrameObject toFrame)
    {
        foreach (Transform obj in fromFrame.GetLayerParent())
        {
            PaintObject original = obj.GetComponent<PaintObject>();
            RectTransform originalRect = obj as RectTransform;

            PaintObject playObject = PoolManager.Instance.TakeGIFObject();
            RectTransform rect = playObject.transform as RectTransform;

            playObject.transform.SetParent(toFrame.GetLayerParent());

            playObject.SetImageSprite(original.GetImageSprite());
            playObject.SetSize(originalRect.sizeDelta);
            rect.rotation = originalRect.rotation;
            rect.localPosition = originalRect.localPosition;
            rect.localScale = originalRect.localScale;

            _objects.Add(playObject.gameObject);
        }
    }

    private void Clear()
    {
        foreach(GameObject frame in _frames)
        {
            //Destroy(frame);
            PoolManager.Instance.ReturnGIFFrame(frame);
        }

        foreach(GameObject gameObject in _objects)
        {
            PoolManager.Instance.ReturnGIFObject(gameObject);
        }

        _frames.Clear();
        _objects.Clear();
    }
}
