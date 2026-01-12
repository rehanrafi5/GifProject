using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : Singleton<ObjectPooling>
{
    #region Serialized Fields
    [SerializeField] private int _objectCount;
    [SerializeField] private int _frameCount;

    [SerializeField] GameObject _objectPrefab;
    [SerializeField] GameObject _layerPrefab;
    [SerializeField] GameObject _framePrefab;
    [SerializeField] GameObject _frameButtonPrefab;

    [SerializeField] private Transform _objectsParent;
    [SerializeField] private Transform _layersParent;
    [SerializeField] private Transform _framesParent;
    [SerializeField] private Transform _frameButtonsParent;

    [SerializeField] private Transform _objectsParentPlay;
    [SerializeField] private Transform _framesParentPlay;

    [SerializeField] private Transform _objectsParentGIF;
    [SerializeField] private Transform _framesParentGIF;

    [SerializeField] private int _numObjects;
    [SerializeField] private int _numLayers;
    [SerializeField] private int _numFrames;
    [SerializeField] private int _numFrameButtons;

    [SerializeField] private int _numObjectsPlay;
    [SerializeField] private int _numFramesPlay;

    [SerializeField] private int _numObjectsGIF;
    [SerializeField] private int _numFramesGIF;

    #endregion // Serialized Fields

    #region Serialized List
    [Header("Painting Window")]
    [SerializeField] private List<PaintObject> _listObjects = new List<PaintObject>();
    [SerializeField] private List<LayeredObject> _listLayer = new List<LayeredObject>();
    [SerializeField] private List<FrameObject> _listFrames = new List<FrameObject>();
    [SerializeField] private List<FrameButton> _listFrameButtons = new List<FrameButton>();

    [Header("Playback Window")]
    [SerializeField] private List<PaintObject> _listObjectsPlay = new List<PaintObject>();
    [SerializeField] private List<FrameObject> _listFramesPlay = new List<FrameObject>();

    [Header("GIF")]
    [SerializeField] private List<PaintObject> _listObjectsGIF = new List<PaintObject>();
    [SerializeField] private List<FrameObject> _listFramesGIF = new List<FrameObject>();
    #endregion // Serialized List

    #region Context Menus
    [ContextMenu("Generate Pool Items")]
    private void GeneratePoolItems()
    {
        Generate(PoolItemType.Object, WindowType.Painting);
        Generate(PoolItemType.Frame, WindowType.Painting);
        Generate(PoolItemType.FrameButton, WindowType.Painting);
        Generate(PoolItemType.Layer, WindowType.Painting);

        Generate(PoolItemType.Object, WindowType.Playback);
        Generate(PoolItemType.Frame, WindowType.Playback);

        Generate(PoolItemType.Object, WindowType.GIF);
        Generate(PoolItemType.Frame, WindowType.GIF);
    }

    [ContextMenu("Clear Pool Items")]
    private void ClearPoolItems()
    {
        Clear(PoolItemType.Object, WindowType.Painting);
        Clear(PoolItemType.Frame, WindowType.Painting);
        Clear(PoolItemType.FrameButton, WindowType.Painting);
        Clear(PoolItemType.Layer, WindowType.Painting);

        Clear(PoolItemType.Object, WindowType.Playback);
        Clear(PoolItemType.Frame, WindowType.Playback);

        Clear(PoolItemType.Object, WindowType.GIF);
        Clear(PoolItemType.Frame, WindowType.GIF);
    }

    #endregion // Context Menus

    #region Unity Callbacks
    private void Start()
    {
        GeneratePoolItems();
    }
    #endregion // Unity Callbacks

    #region Public Methods
    public PaintObject TakePooledObject(WindowType window)
    {
        if (window == WindowType.Painting)
        {
            for (int i = 0; i < _listObjects.Count; i++)
            {
                if (!_listObjects[i].gameObject.activeInHierarchy)
                {
                    DecrementCounterValue(PoolItemType.Object, window);
                    return _listObjects[i];
                }
            }
            return null;
        }
        else if (window == WindowType.Playback)
        {
            for (int i = 0; i < _listObjectsPlay.Count; i++)
            {
                if (!_listObjectsPlay[i].gameObject.activeInHierarchy)
                {
                    DecrementCounterValue(PoolItemType.Object, window);
                    return _listObjectsPlay[i];
                }
            }
            return null;
        }
        else if (window == WindowType.GIF)
        {
            for (int i = 0; i < _listObjectsGIF.Count; i++)
            {
                if (!_listObjectsGIF[i].gameObject.activeInHierarchy)
                {
                    DecrementCounterValue(PoolItemType.Object, window);
                    return _listObjectsGIF[i];
                }
            }
            return null;
        }
        else
        {
            return null;
        }
    }

    public void ReturnObjectToPool(PaintObject paintObject, WindowType window)
    {
        Transform parent = GetParent(PoolItemType.Object, window);
        if (parent == null)
            return;

        bool logic = ((window == WindowType.Painting) && (_listObjects.Contains(paintObject))
            || (window == WindowType.Playback) && (_listObjectsPlay.Contains(paintObject))
            || (window == WindowType.GIF) && (_listObjectsGIF.Contains(paintObject)));

        if (logic)
        {
            paintObject.gameObject.SetActive(false);
            paintObject.transform.SetParent(parent);

            paintObject.GetRectTransform().localPosition = Vector3.zero;

            IncrementCounterValue(PoolItemType.Object, window);
        }
    }

    public FrameObject TakePooledFrame(WindowType window)
    {
        if (window == WindowType.Painting)
        {
            for (int i = 0; i < _listFrames.Count; i++)
            {
                if (!_listFrames[i].gameObject.activeInHierarchy)
                {
                    DecrementCounterValue(PoolItemType.Frame, window);
                    return _listFrames[i];
                }
            }
            return null;
        }
        else if (window == WindowType.Playback)
        {
            for (int i = 0; i < _listFramesPlay.Count; i++)
            {
                if (!_listFramesPlay[i].gameObject.activeInHierarchy)
                {
                    DecrementCounterValue(PoolItemType.Frame, window);
                    return _listFramesPlay[i];
                }
            }
            return null;
        }
        else if (window == WindowType.GIF)
        {
            for (int i = 0; i < _listFramesGIF.Count; i++)
            {
                if (!_listFramesGIF[i].gameObject.activeInHierarchy)
                {
                    DecrementCounterValue(PoolItemType.Frame, window);
                    return _listFramesGIF[i];
                }
            }
            return null;
        }
        else
        {
            return null;
        }
    }

    public void ReturnFrameToPool(FrameObject frameObject, WindowType window)
    {
        Transform parent = GetParent(PoolItemType.Frame, window);
        if (parent == null)
            return;

        bool logic = ((window == WindowType.Painting) && (_listFrames.Contains(frameObject))
            || (window == WindowType.Playback) && (_listFramesPlay.Contains(frameObject))
            || (window == WindowType.GIF) && (_listFramesGIF.Contains(frameObject)));

        if (logic)
        {
            frameObject.gameObject.SetActive(false);
            frameObject.transform.SetParent(parent);

            frameObject.GetRectTransform().localPosition = Vector3.zero;

            IncrementCounterValue(PoolItemType.Frame, window);
        }
    }

    public FrameButton TakePooledFrameButton(WindowType window)
    {
        if (window == WindowType.Painting)
        {
            for (int i = 0; i < _listFrameButtons.Count; i++)
            {
                if (!_listFrameButtons[i].gameObject.activeInHierarchy)
                {
                    DecrementCounterValue(PoolItemType.FrameButton, window);
                    return _listFrameButtons[i];
                }
            }
            return null;
        }
        else
        {
            return null;
        }
    }

    public void ReturnFrameButtonToPool(FrameButton frameButton, WindowType window)
    {
        Transform parent = GetParent(PoolItemType.FrameButton, window);
        if (parent == null)
            return;

        if ((window == WindowType.Painting) && (_listFrameButtons.Contains(frameButton)))
        {
            frameButton.gameObject.SetActive(false);
            frameButton.transform.SetParent(parent);

            frameButton.GetRectTransform().localPosition = Vector3.zero;

            IncrementCounterValue(PoolItemType.FrameButton, window);
        }
    }

    public LayeredObject TakePooledLayer(WindowType window)
    {
        if (window == WindowType.Painting)
        {
            for (int i = 0; i < _listLayer.Count; i++)
            {
                if (!_listLayer[i].gameObject.activeInHierarchy)
                {
                    DecrementCounterValue(PoolItemType.Layer, window);
                    return _listLayer[i];
                }
            }
            return null;
        }
        else
        {
            return null;
        }
    }

    public void ReturnLayerToPool(LayeredObject layer, WindowType window)
    {
        Transform parent = GetParent(PoolItemType.Layer, window);
        if (parent == null)
            return;

        if ((window == WindowType.Painting) && (_listLayer.Contains(layer)))
        {
            layer.gameObject.SetActive(false);
            layer.transform.SetParent(parent);

            layer.GetRectTransform().localPosition = Vector3.zero;

            IncrementCounterValue(PoolItemType.Layer, window);
        }
    }
    #endregion // Public Methods

    #region Private Methods
    private void Generate(PoolItemType type, WindowType window)
    {
        if (type == PoolItemType.Unset)
        {
            Debug.LogWarning("Cannot Generate Unset Pool Item Type.");
            return;
        }

        int count = CalculateNeededItems(type, window);

        if (count <= 0)
        {
            Debug.LogWarning($"Cannot Generate Pool with {count} items.");
            return;
        }

        GameObject temp = null;
        for (int i = 0; i < count; i++)
        {
            temp = Instantiate(GetPrefab(type), GetParent(type, window));
            temp.SetActive(false);
            //temp.transform.SetParent(GetParent(type));

            AddToList(temp, type, window);
        }

        SetCounterValue(GetPoolCount(type), type, window);
    }

    private void SetCounterValue(int value, PoolItemType type, WindowType window)
    {
        if (type == PoolItemType.Object)
        {
            if (window == WindowType.Painting)
                _numObjects = value;
            else if (window == WindowType.Playback)
                _numObjectsPlay = value;
            else if (window == WindowType.GIF)
                _numObjectsGIF = value;
        }
        else if (type == PoolItemType.Frame)
        {
            if (window == WindowType.Painting)
                _numFrames = value;
            else if (window == WindowType.Playback)
                _numFramesPlay = value;
            else if (window == WindowType.GIF)
                _numFramesGIF = value;
        }
        else if (type == PoolItemType.FrameButton)
        {
            _numFrameButtons = value;
        }
        else if (type == PoolItemType.Layer)
        {
            _numLayers = value;
        }
    }

    private int GetCounterValue(PoolItemType type, WindowType window)
    {
        if (type == PoolItemType.Object)
        {
            if (window == WindowType.Painting)
                return _numObjects;
            else if (window == WindowType.Playback)
                return _numObjectsPlay;
            else if (window == WindowType.GIF)
                return _numObjectsGIF;
            else
                return 0;
        }
        else if (type == PoolItemType.Frame)
        {
            if (window == WindowType.Painting)
                return _numFrames;
            else if (window == WindowType.Playback)
                return _numFramesPlay;
            else if (window == WindowType.GIF)
                return _numFramesGIF;
            else
                return 0;
        }
        else if (type == PoolItemType.FrameButton)
        {
            return _numFrameButtons;
        }
        else if (type == PoolItemType.Layer)
        {
            return _numLayers;
        }
        else
            return 0;
    }

    private void IncrementCounterValue(PoolItemType type, WindowType window)
    {
        SetCounterValue(
            (GetCounterValue(type, window) + 1),
            type,
            window);
    }

    private void DecrementCounterValue(PoolItemType type, WindowType window)
    {
        SetCounterValue(
            (GetCounterValue(type, window) - 1),
            type,
            window);
    }

    private void AddToList(GameObject gameObject, PoolItemType type, WindowType window)
    {
        switch (type)
        {
            case PoolItemType.Object:
                gameObject.TryGetComponent(out PaintObject paintObject);
                if (paintObject == null)
                {
                    return;
                }

                if (window == WindowType.Painting)
                    _listObjects.Add(paintObject);
                else if (window == WindowType.Playback)
                    _listObjectsPlay.Add(paintObject);
                else if (window == WindowType.GIF)
                    _listObjectsGIF.Add(paintObject);
                else
                    Debug.LogWarning("No window set when adding object.");

                break;
            case PoolItemType.Frame:
                gameObject.TryGetComponent(out FrameObject frameObject);
                if (frameObject == null)
                {
                    return;
                }

                if (window == WindowType.Painting)
                    _listFrames.Add(frameObject);
                else if (window == WindowType.Playback)
                    _listFramesPlay.Add(frameObject);
                else if (window == WindowType.GIF)
                    _listFramesGIF.Add(frameObject);
                else
                    Debug.LogWarning("No window set when adding frame.");

                break;
            case PoolItemType.FrameButton:
                gameObject.TryGetComponent(out FrameButton frameButton);
                if (frameButton == null)
                {
                    return;
                }

                _listFrameButtons.Add(frameButton);
                break;
            case PoolItemType.Layer:
                gameObject.TryGetComponent(out LayeredObject layer);
                if (layer == null)
                {
                    return;
                }

                _listLayer.Add(layer);
                break;
            default:
                Debug.LogWarning($"Cannot add type {type} to any list.");
                break;
        }
    }

    private int CalculateNeededItems(PoolItemType type, WindowType window)
    {
        Transform parent = GetParent(type, window);
        if (parent == null)
        {
            Debug.LogWarning($"Parent for type {type} and window {window} is null.");
            return 0;
        }

        return (GetPoolCount(type) - parent.childCount);
    }

    private int GetPoolCount(PoolItemType type)
    {
        switch (type)
        {
            case PoolItemType.Object:
            case PoolItemType.Layer:
                return _objectCount;
            case PoolItemType.Frame:
            case PoolItemType.FrameButton:
                return _frameCount;
            default:
                return 0;
        }
    }

    private GameObject GetPrefab(PoolItemType type)
    {
        switch(type)
        {
            case PoolItemType.Object:
                return _objectPrefab;
            case PoolItemType.Frame:
                return _framePrefab;
            case PoolItemType.FrameButton:
                return _frameButtonPrefab;
            case PoolItemType.Layer:
                return _layerPrefab;
            default:
                return null;
        }
    }

    private Transform GetParent(PoolItemType type, WindowType window)
    {
        switch(type)
        {
            case PoolItemType.Object:
                if (window == WindowType.Painting)
                    return _objectsParent;
                else if (window == WindowType.Playback)
                    return _objectsParentPlay;
                else if (window == WindowType.GIF)
                    return _objectsParentGIF;
                else
                    return null;
            case PoolItemType.Frame:
                if (window == WindowType.Painting)
                    return _framesParent;
                else if (window == WindowType.Playback)
                    return _framesParentPlay;
                else if (window == WindowType.GIF)
                    return _framesParentGIF;
                else
                    return null;
            case PoolItemType.FrameButton:
                return _frameButtonsParent;
            case PoolItemType.Layer:
                return _layersParent;
            default:
                return null;
        }
    }

    private void Clear(PoolItemType type, WindowType window)
    {
        Transform parent = GetParent(type, window);
        if (parent == null)
        {
            Debug.Log($"Cannot Clear items for null type {type}");
            return;
        }

        ClearList(type, window);
        SetCounterValue(0, type, window);

        while (parent.childCount > 0)
        {
            DestroyImmediate(parent.GetChild(0).gameObject);
        }
    }

    private void ClearList(PoolItemType type, WindowType window)
    {
        if (type == PoolItemType.Object)
        {
            if (window == WindowType.Painting)
                _listObjects.Clear();
            else if (window == WindowType.Playback)
                _listObjectsPlay.Clear();
            else if (window == WindowType.GIF)
                _listObjectsGIF.Clear();
        }
        else if (type == PoolItemType.Frame)
        {
            if (window == WindowType.Painting)
                _listFrames.Clear();
            else if (window == WindowType.Playback)
                _listFramesPlay.Clear();
            else if (window == WindowType.GIF)
                _listFramesGIF.Clear();
        }
        else if (type == PoolItemType.FrameButton)
        {
            _listFrameButtons.Clear();
        }
        else if (type == PoolItemType.Layer)
        {
            _listLayer.Clear();
        }
        else
        {
            Debug.LogWarning($"No list to clear PoolItemType {type} and WindowType {window}");
        }
    }
    #endregion // Private Methods
}