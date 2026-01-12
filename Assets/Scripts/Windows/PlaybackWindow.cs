using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaybackWindow : Window
{
    [SerializeField] Scrollbar _speedScrollbar;
    [SerializeField] Button _saveGIFButton;
    [SerializeField] Transform _framesParent;

    [SerializeField] private int MIN_FPS = 6;
    private const int MAX_FPS = 15;
    [SerializeField] private int FPS_INCREASE = 3;

    private int _framesPerSecond;
    private float _currentSpeed;
    private float _currentTime = 0f;

    private List<GameObject> _frames = new List<GameObject>();
    private List<GameObject> _objects = new List<GameObject>();
    private int _frameIndex;
    private bool _isRunning;

    protected override void Start()
    {
        base.Start();
        EnableMenu();

        _framesPerSecond = MIN_FPS;
        _currentSpeed = 1f / (float)MIN_FPS;
    }

    private void EnableMenu()
    {
        _saveGIFButton.onClick.AddListener(OnSaveGIF);
        _speedScrollbar.onValueChanged.AddListener(OnSpeedChange);
    }

    private void DisableMenu()
    {
        _saveGIFButton.onClick.RemoveListener(OnSaveGIF);
        _speedScrollbar.onValueChanged.RemoveListener(OnSpeedChange);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DisableMenu();
    }

    private void Update()
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

    public override void Show()
    {
        base.Show();

        _frameIndex = 0;
        _currentTime = 0f;
        _currentSpeed = 1f / (float) _framesPerSecond;
        _frames[0].SetActive(true);
        _isRunning = true;

        GIFManager.Instance.SetSpeed(_currentSpeed);
    }

    public override void Hide()
    {
        base.Hide();
        _isRunning = false;
    }

    public void SetFrames(List<FrameObject> frameObjects)
    {
        Clear();

        for(int i = 0; i < frameObjects.Count; i++)
        {
            //GameObject frameInstance = Instantiate(frameObjects[i].gameObject, _framesParent);
            FrameObject frameInstance = PoolManager.Instance.TakePlayFrame();
            frameInstance.transform.SetParent(_framesParent);
            BuildChildObjects(frameObjects[i], frameInstance);

            _frames.Add(frameInstance.gameObject);
            frameInstance.gameObject.SetActive(false);
        }

        GIFManager.Instance.SetFrames(frameObjects);
    }

    private void BuildChildObjects(FrameObject fromFrame, FrameObject toFrame)
    {
        foreach (Transform obj in fromFrame.GetLayerParent())
        {
            PaintObject original = obj.GetComponent<PaintObject>();
            RectTransform originalRect = obj as RectTransform;

            PaintObject playObject = PoolManager.Instance.TakePlayObject();
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
            PoolManager.Instance.ReturnPlayFrame(frame);
        }

        foreach(GameObject gameObject in _objects)
        {
            PoolManager.Instance.ReturnPlayObject(gameObject);
        }

        _frames.Clear();
        _objects.Clear();
    }

    private void OnSpeedChange(float value)
    {
        _framesPerSecond = MIN_FPS + FPS_INCREASE * Mathf.RoundToInt(value * _speedScrollbar.numberOfSteps);
        _currentSpeed = 1f / (float) _framesPerSecond;

        GIFManager.Instance.SetSpeed(_currentSpeed);
    }

    private void OnSaveGIF()
    {
        RecorderManager.Instance.Record(GIFManager.Instance.Width, GIFManager.Instance.Height, (float) _frames.Count * _currentSpeed, _framesPerSecond);
    }
}
