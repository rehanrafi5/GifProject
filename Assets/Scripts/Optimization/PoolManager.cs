using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PoolManager : Singleton<PoolManager>
{
    #region Serialized Fields
    [Header("Pools")]
    [SerializeField] private Pool _paintObject;
    [SerializeField] private Pool _paintLayer;
    [SerializeField] private Pool _paintFrame;
    [SerializeField] private Pool _paintFrameButton;
    [Space]
    [SerializeField] private Pool _playObject;
    [SerializeField] private Pool _playFrame;
    [Space]
    [SerializeField] private Pool _gifObject;
    [SerializeField] private Pool _gifFrame;
    [Space]
    [SerializeField] private Pool _dragPlaceholder;

    [Header("Pool Items Count")]
    [SerializeField] private int _objectCount;
    [SerializeField] private int _frameCount;
    [SerializeField] private int _dragPlaceholderCount;

    [Header("Visual Available Pool Items")]
    [SerializeField] private TextMeshProUGUI _txtAvailableObjects;
    [SerializeField] private TextMeshProUGUI _txtAvailableFrames;
    #endregion // Serialized Fields

    #region Public API
    public int AvailablePaintObjects => _paintObject.AvailableItems;
    public int AvailablePaintFrames => _paintFrame.AvailableItems;

    public int MaxObjects => _objectCount;
    public int MaxFrames => _frameCount;

    // TAKE FROM POOL
    public PaintObject TakePaintObject()
        => TakeFromPool(WindowType.Painting, PoolItemType.Object)
        .GetComponent<PaintObject>();

    public LayeredObject TakePaintLayer()
        => TakeFromPool(WindowType.Painting, PoolItemType.Layer)
        .GetComponent<LayeredObject>();

    public FrameObject TakePaintFrame()
        => TakeFromPool(WindowType.Painting, PoolItemType.Frame)
        .GetComponent<FrameObject>();

    public FrameButton TakePaintFrameButton()
        => TakeFromPool(WindowType.Painting, PoolItemType.FrameButton)
        .GetComponent<FrameButton>();

    public PaintObject TakePlayObject()
        => TakeFromPool(WindowType.Playback, PoolItemType.Object)
        .GetComponent<PaintObject>();

    public FrameObject TakePlayFrame()
        => TakeFromPool(WindowType.Playback, PoolItemType.Frame)
        .GetComponent<FrameObject>();

    public PaintObject TakeGIFObject()
        => TakeFromPool(WindowType.GIF, PoolItemType.Object)
        .GetComponent<PaintObject>();

    public FrameObject TakeGIFFrame()
        => TakeFromPool(WindowType.GIF, PoolItemType.Frame)
        .GetComponent<FrameObject>();

    public GameObject TakeDragPlaceholder()
        => TakeFromPool(WindowType.Unset, PoolItemType.DragPlaceholder);

    // RETURN TO POOL
    public void ReturnPaintObject(GameObject gameObject)
        => ReturnToPool(gameObject, WindowType.Painting, PoolItemType.Object);

    public void ReturnPaintLayer(GameObject gameObject)
        => ReturnToPool(gameObject, WindowType.Painting, PoolItemType.Layer);

    public void ReturnPaintFrame(GameObject gameObject)
        => ReturnToPool(gameObject, WindowType.Painting, PoolItemType.Frame);

    public void ReturnPaintFrameButton(GameObject gameObject)
        => ReturnToPool(gameObject, WindowType.Painting, PoolItemType.FrameButton);

    public void ReturnPlayObject(GameObject gameObject)
        => ReturnToPool(gameObject, WindowType.Playback, PoolItemType.Object);

    public void ReturnPlayFrame(GameObject gameObject)
        => ReturnToPool(gameObject, WindowType.Playback, PoolItemType.Frame);

    public void ReturnGIFObject(GameObject gameObject)
        => ReturnToPool(gameObject, WindowType.GIF, PoolItemType.Object);

    public void ReturnGIFFrame(GameObject gameObject)
        => ReturnToPool(gameObject, WindowType.GIF, PoolItemType.Frame);

    public void ReturnDragPlaceholder(GameObject gameObject)
        => ReturnToPool(gameObject, WindowType.Unset, PoolItemType.DragPlaceholder);

    public void UpdateAvailableCount(WindowType windowType, PoolItemType poolItemType)
    {
        if (windowType == WindowType.Painting && poolItemType == PoolItemType.Object)
        {
            _txtAvailableObjects.text = AvailablePaintObjects.ToString();
        }

        if (windowType == WindowType.Painting && poolItemType == PoolItemType.Frame)
        {
            _txtAvailableFrames.text = AvailablePaintFrames.ToString();
        }
    }

    #endregion // Public API

    #region Private Methods
    private GameObject TakeFromPool(WindowType windowType, PoolItemType poolItemType)
    {
        if (_paintObject.IsPoolCompatible(poolItemType, windowType))
        {
            return _paintObject.TakeFromPool();
        }
        else if (_paintLayer.IsPoolCompatible(poolItemType, windowType))
        {
            return _paintLayer.TakeFromPool();
        }
        else if (_paintFrame.IsPoolCompatible(poolItemType, windowType))
        {
            return _paintFrame.TakeFromPool();
        }
        else if (_paintFrameButton.IsPoolCompatible(poolItemType, windowType))
        {
            return _paintFrameButton.TakeFromPool();
        }
        else if (_playObject.IsPoolCompatible(poolItemType, windowType))
        {
            return _playObject.TakeFromPool();
        }
        else if (_playFrame.IsPoolCompatible(poolItemType, windowType))
        {
            return _playFrame.TakeFromPool();
        }
        else if (_gifObject.IsPoolCompatible(poolItemType, windowType))
        {
            return _gifObject.TakeFromPool();
        }
        else if (_gifFrame.IsPoolCompatible(poolItemType, windowType))
        {
            return _gifFrame.TakeFromPool();
        }
        else if (_dragPlaceholder.IsPoolCompatible(poolItemType, windowType))
        {
            return _dragPlaceholder.TakeFromPool();
        }

        return null;
    }

    private void ReturnToPool(GameObject gameObject, WindowType windowType, PoolItemType poolItemType)
    {
        if (_paintObject.IsPoolCompatible(poolItemType, windowType))
        {
            _paintObject.ReturnToPool(gameObject);
        }
        else if (_paintLayer.IsPoolCompatible(poolItemType, windowType))
        {
            _paintLayer.ReturnToPool(gameObject);
        }
        else if (_paintFrame.IsPoolCompatible(poolItemType, windowType))
        {
            _paintFrame.ReturnToPool(gameObject);
        }
        else if (_paintFrameButton.IsPoolCompatible(poolItemType, windowType))
        {
            _paintFrameButton.ReturnToPool(gameObject);
        }
        else if (_playObject.IsPoolCompatible(poolItemType, windowType))
        {
            _playObject.ReturnToPool(gameObject);
        }
        else if (_playFrame.IsPoolCompatible(poolItemType, windowType))
        {
            _playFrame.ReturnToPool(gameObject);
        }
        else if (_gifObject.IsPoolCompatible(poolItemType, windowType))
        {
            _gifObject.ReturnToPool(gameObject);
        }
        else if (_gifFrame.IsPoolCompatible(poolItemType, windowType))
        {
            _gifFrame.ReturnToPool(gameObject);
        }
        else if (_dragPlaceholder.IsPoolCompatible(poolItemType, windowType))
        {
            _dragPlaceholder.ReturnToPool(gameObject);
        }
    }

    private void SetPoolCount(bool usePoolCount = true)
    {
        if (usePoolCount)
        {
            // OBJECT-RELATED
            _paintObject.SetPoolCount(_objectCount);
            _paintLayer.SetPoolCount(_objectCount);
            _playObject.SetPoolCount(_objectCount);
            _gifObject.SetPoolCount(_objectCount);

            // FRAME-RELATED
            _paintFrame.SetPoolCount(_frameCount);
            _paintFrameButton.SetPoolCount(_frameCount);
            _playFrame.SetPoolCount(_frameCount);
            _gifFrame.SetPoolCount(_frameCount);

            // DRAG PLACEHOLDER
            _dragPlaceholder.SetPoolCount(_dragPlaceholderCount);
        }
        else
        {
            // OBJECT-RELATED
            _paintObject.SetPoolCount(0);
            _paintLayer.SetPoolCount(0);
            _playObject.SetPoolCount(0);
            _gifObject.SetPoolCount(0);

            // FRAME-RELATED
            _paintFrame.SetPoolCount(0);
            _paintFrameButton.SetPoolCount(0);
            _playFrame.SetPoolCount(0);
            _gifFrame.SetPoolCount(0);

            // DRAG PLACEHOLDER
            _dragPlaceholder.SetPoolCount(0);
        }
    }
    #endregion // Private Methods

    #region Context Menus
    [ContextMenu("Generate Pool Items")]
    private void GeneratePoolItems()
    {
        SetPoolCount(true);

        _paintObject.Generate();
        _paintLayer.Generate();
        _paintFrame.Generate();
        _paintFrameButton.Generate();

        _playObject.Generate();
        _playFrame.Generate();
        _gifObject.Generate();
        _gifFrame.Generate();

        _dragPlaceholder.Generate();
    }
    [ContextMenu("Clear Pool Items")]
    private void ClearPoolItems()
    {
        SetPoolCount(false);

        _paintObject.Clear();
        _paintLayer.Clear();
        _paintFrame.Clear();
        _paintFrameButton.Clear();

        _playObject.Clear();
        _playFrame.Clear();
        _gifObject.Clear();
        _gifFrame.Clear();

        _dragPlaceholder.Clear();
    }
    #endregion // Context Menus

    #region Unity Callbacks
    //private void Start()
    protected override void Awake()
    {
        base.Awake();
        // In case the designer fails to use the Context Menu to Generate Pools
        GeneratePoolItems();
    }
    #endregion // Unity Callbacks
}
