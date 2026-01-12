using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FrameManager : Singleton<FrameManager>
{
    [SerializeField] private FrameObject _framePrefab;
    [SerializeField] private GameObject _frameButtonPrefab;
    [SerializeField] private Transform _framesParent;
    [SerializeField] private Transform _frameButtonParent;
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private Button _removeFrameButton;
    [SerializeField] private Button _duplicateFrameButton;
    [SerializeField] private Button _duplicateNextFrameButton;
    [SerializeField] private ScrollRect _framesScroll;

    [SerializeField]
    [Range(0f, 500f)]
    [Tooltip("Additional value for the range of content doled out by this script to its subscribers.")]
    private float _visibleFrameBuffer = 160f;
    private float _frameScrollWidth;

    private List<FrameObject> _frames = new List<FrameObject>();
    private FrameObject _currentFrame = null;
    public FrameObject CurrentFrame => _currentFrame;
    private Rect _screenRect;

    public Vector2 FrameScale { private set; get; }

    public Action OnDeleteFrame;
    public Action<float, float> OnFrameScroll;

    protected override void Awake()
    {
        base.Awake();
        _frameScrollWidth = _framesScroll.GetComponent<RectTransform>().rect.width;
    }
    private void Start()
    {
        _removeFrameButton.onClick.AddListener(RemoveFrame);
        _duplicateFrameButton.onClick.AddListener(DuplicateFrame);
        _duplicateNextFrameButton.onClick.AddListener(DuplicateFrameNext);

        RectTransform rectTransform = _framesParent as RectTransform;
        Vector3[] worldCorners = new Vector3[4];
        Vector3[] screenCorners = new Vector3[4];

        rectTransform.GetWorldCorners(worldCorners);

        for (int i = 0; i < worldCorners.Length; i++)
        {
            screenCorners[i] = RectTransformUtility.WorldToScreenPoint(Camera.main, worldCorners[i]);
        }

        Bounds bounds = new Bounds(screenCorners[0], Vector3.zero);

        for (int i = 1; i < 4; ++i)
        {
            bounds.Encapsulate(screenCorners[i]);
        }

        _screenRect = new Rect(bounds.min, bounds.size);
        FrameScale = new Vector2((float) Screen.width / _screenRect.width, (float)Screen.height / _screenRect.height);
    }

    private void OnDestroy()
    {
        _removeFrameButton.onClick.RemoveListener(RemoveFrame);
        _duplicateFrameButton.onClick.RemoveListener(DuplicateFrame);
        _duplicateNextFrameButton.onClick.RemoveListener(DuplicateFrameNext);

        for(int i = 0; i < _frames.Count; i++)
        {
            FrameButton button = _frames[i].FrameButton;
            button.OnFrameClicked -= HandleFrameSwitched;
            button.OnFrameSwapped -= HandleFrameSwapped;

            _frames[i].OnObjectDragged -= HandleObjectDragged;
            _frames[i].OnObjectSelected -= HandleObjectSelected;
        }
    }

    public PaintObject AddObject(GameObject objectPrefab, Sprite sprite, float defaultSize, Vector2 screenPosition)
    {
        return _currentFrame.AddObject(objectPrefab, sprite, defaultSize, screenPosition);
    }

    public PaintObject AddObjectForSet(GameObject objectPrefab, Sprite sprite, float defaultSize, Vector2 screenPosition, Vector2 offsetPos, Quaternion rotation, int setGroup)
    {
        return _currentFrame.AddObjectForSet(objectPrefab, sprite, defaultSize, screenPosition, offsetPos, rotation, setGroup);
    }

    public void AddFrame()
    {
        CreateFrame(_framePrefab, _frames.Count);
    }

    private void DuplicateFrame()
    {
        CreateFrame(_currentFrame, _frames.Count);
        //TakeScreenshots();
        StopAllCoroutines(); // stop it first in case it is still running
        StartCoroutine(C_WaitThenScreenshot());
    }

    private void DuplicateFrameNext()
    {
        int frameIndex = _currentFrame.transform.GetSiblingIndex();
        CreateFrame(_currentFrame, frameIndex + 1);
        //TakeScreenshots();
        StopAllCoroutines(); // stop it first in case it is still running
        StartCoroutine(C_WaitThenScreenshot());
    }

    private void CreateFrame(FrameObject baseFrame, int index)
    {
        bool hasAvailableObjects = (PoolManager.Instance.AvailablePaintObjects >= baseFrame.GetLayerParent().childCount);
        bool hasAvailableFrames = (PoolManager.Instance.AvailablePaintFrames > 0);

        if (!hasAvailableFrames)
        {
            Debug.LogWarning("DGS Cannot add frame due to insufficient frame pool.");
            return;
        }
        if (!hasAvailableObjects)
        {
            Debug.LogWarning("DGS Cannot add frame due to insufficient object pool.");
            return;
        }

        //GameObject frameInstance = Instantiate(baseObject, _framesParent);
        //FrameObject frame = frameInstance.GetComponent<FrameObject>();

        FrameObject frame = PoolManager.Instance.TakePaintFrame();
        frame.transform.SetParent(_framesParent);
        CloneObjects(baseFrame, frame);

        frame.OnObjectDragged += HandleObjectDragged;
        frame.OnObjectSelected += HandleObjectSelected;
        frame.transform.SetSiblingIndex(index);
        _frames.Insert(index, frame);

        //frameInstance.name = "Frame " + _frames.Count.ToString();
        frame.gameObject.name = "Frame " + _frames.Count.ToString();

        //GameObject frameButtonInstance = Instantiate(_frameButtonPrefab, _frameButtonParent);
        //FrameButton frameButton = frameButtonInstance.GetComponent<FrameButton>();

        FrameButton frameButton = PoolManager.Instance.TakePaintFrameButton();
        frameButton.transform.SetParent(_frameButtonParent);

        frameButton.OnFrameClicked += HandleFrameSwitched;
        frameButton.OnFrameSwapped += HandleFrameSwapped;
        frameButton.transform.SetSiblingIndex(index);
        frameButton.SetToggleGroup(_toggleGroup);

        frame.FrameButton = frameButton;

        //TakeScreenshot(frame, () => HandleFrameSwitched(index));
        HandleFrameSwitched(index);
    }

    private void CloneObjects(FrameObject baseFrame, FrameObject dupFrame)
    {
        foreach (Transform child in baseFrame.GetLayerParent())
        {
            if (child.TryGetComponent<PaintObject>(out PaintObject baseObject))
            {
                CloneObject(baseObject, dupFrame, Vector3.zero, baseFrame);
            }
        }
    }

    private PaintObject CloneObject(PaintObject sourceObject, FrameObject dupFrame,
        Vector3 offsetPos, FrameObject baseFrame)
    {
        RectTransform rect = sourceObject.transform as RectTransform;
        PaintObject dupObj = dupFrame.DuplicateObject(
            sourceObject.GetImageSprite(),
            rect,
            dupFrame.GetLayerParent(),
            offsetPos);
        ObjectManager.Instance.AddClonedObjectToLayer(dupObj, baseFrame, dupFrame);

        return dupObj;
    }

    public PaintObject DuplicateObject(PaintObject sourceObject)
    {
        return CloneObject(sourceObject, _currentFrame, new Vector3(20f, -20f, 0), null);
    }

    private void RemoveFrame()
    {
        OnDeleteFrame?.Invoke();
        int frameIndex = _currentFrame.transform.GetSiblingIndex();
        FrameObject removedFrame = _currentFrame;
        UnsubscribeRemovedFrame(removedFrame);
        removedFrame.ReturnToPool();
        removedFrame.FrameButton.RemoveTexture();

        _frames.RemoveAt(frameIndex);
        _currentFrame = null;
        if (frameIndex == 0 && _frames.Count > 0)
            SetFrame(frameIndex);
        else if (frameIndex > 0 && _frames.Count > 0)
            SetFrame(frameIndex - 1);

        if(_frames.Count == 0)
        {
            AddFrame();
        }
        //TakeScreenshots();
        StopAllCoroutines(); // stop it first in case it is still running
        StartCoroutine(C_WaitThenScreenshot());
    }

    private void UnsubscribeRemovedFrame(FrameObject removedFrame)
    {
        removedFrame.OnObjectDragged -= HandleObjectDragged;
        removedFrame.OnObjectSelected -= HandleObjectSelected;

        removedFrame.FrameButton.OnFrameClicked -= HandleFrameSwitched;
        removedFrame.FrameButton.OnFrameSwapped -= HandleFrameSwapped;
    }

    private void HandleFrameSwitched(int index)
    {
        ObjectManager.Instance.DeselectObject();
        UndoManager.Instance.Clear();

        if(_frames.Contains(_currentFrame))
        {
            TakeScreenshot(_currentFrame, () => SwitchFrame(index));
        }

        else
        {
            SetFrame(index);
        }
    }

    private void TakeScreenshot(FrameObject frame, Action callback)
    {
        GIFManager.Instance.TakeScreenshot(frame, (texture) => OnScreenshotTaken(frame, texture, callback));
    }

    private void TakeScreenshot(FrameObject frame)
    {
        GIFManager.Instance.TakeScreenshot(frame, (texture) => OnScreenshotTaken(frame, texture));
    }

    private void OnScreenshotTaken(FrameObject frame, Texture2D screenshot, Action callback)
    {
        frame.FrameButton.SetScreenshot(screenshot);
        callback?.Invoke();
    }

    private void OnScreenshotTaken(FrameObject frame, Texture2D screenshot)
    {
        frame.FrameButton.SetScreenshot(screenshot);
    }

    private void SwitchFrame(int index)
    {
        _currentFrame.Hide();
        SetFrame(index);
    }

    private void HandleFrameSwapped(int previousIndex, int newIndex)
    {
        FrameObject swappedFrame = _frames[previousIndex];
        swappedFrame.transform.SetSiblingIndex(newIndex);

        _frames.Sort((f1, f2) => f1.transform.GetSiblingIndex().CompareTo(f2.transform.GetSiblingIndex()));

        foreach(FrameObject frame in _frames)
        {
            frame.FrameButton.UpdateFrameNumber();
        }

    }

    private void SetFrame(int index)
    {
        _currentFrame = _frames[index];
        _currentFrame.Show();
        ObjectManager.Instance.PopulateObjects(_currentFrame.GetPaintObjects());
    }

    public List<FrameObject> GetFrames()
    {
        return _frames;
    }

    private void HandleObjectDragged(PointerEventData e)
    {
        ObjectManager.Instance.SetupControls(e);
    }

    private void HandleObjectSelected(PaintObject paintObject)
    {
        ObjectManager.Instance.SelectObject(paintObject);
    }

    #region Scroll Frame Buttons
    // These methods need to be added into both the Scroll Rect and Scrollbar
    // to accommodate both scrolling using the scrollbar and the scroll list
    public void OnScrollBegin()
    {
        StopAllCoroutines(); // stop it first in case it is still running

        // Unload the Textures
        // Show Label on all FrameButtons
        foreach (FrameObject frame in _frames)
        {
            frame.FrameButton.RemoveTexture();
            frame.FrameButton.ShowLabel();
        }

        CollectGarbage();
    }

    public void OnScrollEnd()
    {
        TakeScreenshots();
    }
    #endregion // Scroll Frame Buttons

    private void CollectGarbage()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    // Need to wait to give time when removing a frame
    private IEnumerator C_WaitThenScreenshot()
    {
        yield return new WaitForEndOfFrame();
        TakeScreenshots();
    }

    private void TakeScreenshots()
    {
        // Get the scroll's current position and add the buffer
        var currentScrollPos = Mathf.Abs(_framesScroll.content.localPosition.x);
        var posMinVisible = currentScrollPos - _visibleFrameBuffer;
        var posMaxVisible = currentScrollPos + _visibleFrameBuffer + _frameScrollWidth;

        // The individual frames will determine whether they are visible or not
        OnFrameScroll?.Invoke(posMinVisible, posMaxVisible);

        StopAllCoroutines(); // stop it first in case it is still running
        StartCoroutine(C_TakeScreenshots());
    }

    private IEnumerator C_TakeScreenshots()
    {
        yield return new WaitForEndOfFrame();

        int count = 0;

        foreach (FrameObject frame in _frames)
        {
            frame.FrameButton.UpdateFrameNumber();
            frame.FrameButton.HideLabel();
            // If it is visible in the scroll area AND the frame screenshot is null
            if (frame.FrameButton.IsVisibleOnScroll && !frame.FrameButton.IsScreenshotSaved)
            {
                TakeScreenshot(frame);
                yield return new WaitForEndOfFrame();
                continue;
            }
            else if (!frame.FrameButton.IsVisibleOnScroll && frame.FrameButton.IsScreenshotSaved)
            {
                frame.FrameButton.RemoveTexture();
                count++;
            }
        }

        if (count > 0)
        {
            Debug.Log("DGS Collecting garbage from unloaded texture");
            CollectGarbage();
        }
    }
}
