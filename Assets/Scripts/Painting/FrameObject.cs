using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class FrameObject : MonoBehaviour
{
    [SerializeField] private Transform _parent;

    private Canvas _canvas;
    private RectTransform _rectTransform;
    private List<PaintObject> _spawnedObjects = new List<PaintObject>();
    private FrameButton _button;

    public Action<PaintObject> OnObjectSelected;
    public Action<PointerEventData> OnObjectDragged;

    public FrameButton FrameButton { get { return _button; } set { _button = value; } }

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
    }

    // commented as it has an issue.
    // Scenario => duplicate a frame and select an object to duplicate
    // Result => Creates 2 objects when duplicating an object.
    //private void Start()
    //{
    //    _spawnedObjects = GetComponentsInChildren<PaintObject>(true).ToList();

    //    foreach(PaintObject paintObject in _spawnedObjects)
    //    {
    //        paintObject.OnObjectSelected += HandleObjectSelected;
    //        paintObject.OnObjectDragged += HandleObjectDragged;
    //        paintObject.OnObjectDeleted += HandleObjectDeleted;
    //        paintObject.OnDuplicateTrigger += HandleObjectDuplication;
    //    }
    //}

    private void OnDestroy()
    {
        UnsubscribeObjectsInFrame();
    }

    private void HandleObjectDragged(PointerEventData e)
    {
        OnObjectDragged?.Invoke(e);
    }

    private void HandleObjectSelected(PaintObject paintObject)
    {
        OnObjectSelected?.Invoke(paintObject);
    }

    private void HandleObjectDeleted(PaintObject paintObject)
    {
        paintObject.OnObjectSelected -= HandleObjectSelected;
        paintObject.OnObjectDragged -= HandleObjectDragged;
        paintObject.OnDuplicateTrigger -= HandleObjectDuplication;
        paintObject.OnObjectDeleted -= HandleObjectDeleted;

        _spawnedObjects.Remove(paintObject);
    }

    private void HandleObjectDuplication(PaintObject sourceObject)
    {
        //ObjectManager.Instance.DuplicateObject(sourceObject);
        FrameManager.Instance.DuplicateObject(sourceObject);
    }

    public RectTransform GetRectTransform()
    {
        return _rectTransform;
    }

    public Transform GetLayerParent()
    {
        return _parent;
    }

    public PaintObject AddObject(GameObject objectPrefab, Sprite sprite, float defaultSize, Vector2 screenPosition)
    {
        float objectRatio = sprite.rect.width / sprite.rect.height;

        PaintObject paintObject = PoolManager.Instance.TakePaintObject();
        RectTransform rectTransform = paintObject.GetComponent<RectTransform>();

        // Convert screen position to local position inside the frame's parent
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parent as RectTransform,
            screenPosition,
            _canvas.worldCamera,
            out Vector2 localpoint
        );

        rectTransform.SetParent(_parent); // set parent first
        rectTransform.localPosition = localpoint;

        paintObject.OnObjectSelected += HandleObjectSelected;
        paintObject.OnObjectDragged += HandleObjectDragged;
        paintObject.OnObjectDeleted += HandleObjectDeleted;
        paintObject.OnDuplicateTrigger += HandleObjectDuplication;

        paintObject.SetImageSprite(sprite);
        paintObject.SetSize(new Vector2(defaultSize, defaultSize / objectRatio));
        paintObject.SetIsSetPlaced(false); // SET TO FALSE

        _spawnedObjects.Add(paintObject);

        return paintObject;
    }


    public PaintObject AddObjectForSet(GameObject objectPrefab, Sprite sprite, float defaultSize, Vector2 screenPosition, Vector2 offsetPos, Quaternion rotation, int setGroup)
    {
        float objectRatio = sprite.rect.width / sprite.rect.height;

        PaintObject paintObject = PoolManager.Instance.TakePaintObject();
        RectTransform rectTransform = paintObject.GetComponent<RectTransform>();

        // Convert screen position to local position inside the frame's parent
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parent as RectTransform,
            screenPosition,
            _canvas.worldCamera,
            out Vector2 localpoint
        );

        rectTransform.SetParent(_parent); // set parent first
        rectTransform.localPosition = localpoint + offsetPos;
        rectTransform.rotation = rotation;

        paintObject.OnObjectSelected += HandleObjectSelected;
        paintObject.OnObjectDragged += HandleObjectDragged;
        paintObject.OnObjectDeleted += HandleObjectDeleted;
        paintObject.OnDuplicateTrigger += HandleObjectDuplication;

        paintObject.SetImageSprite(sprite);
        paintObject.SetSize(new Vector2(defaultSize, defaultSize / objectRatio));
        paintObject.SetIsSetPlaced(true); // SET TO TRUE
        paintObject.SetSetGroup(setGroup);

        _spawnedObjects.Add(paintObject);

        return paintObject;
    }


    /// <summary>
    /// DGS This is a helper to Frame Manager when duplicating a frame.
    /// </summary>
    public PaintObject DuplicateObject(Sprite sprite, RectTransform rect, Transform parent, Vector3 offsetPos)
    {
        PaintObject dupObj = PoolManager.Instance.TakePaintObject();
        RectTransform dupRect = dupObj.GetComponent<RectTransform>();

        dupRect.localPosition = rect.localPosition + offsetPos;
        dupRect.rotation = rect.rotation;
        dupRect.sizeDelta = rect.sizeDelta;
        dupRect.localScale = rect.localScale;

        dupObj.OnObjectSelected += HandleObjectSelected;
        dupObj.OnObjectDragged += HandleObjectDragged;
        dupObj.OnObjectDeleted += HandleObjectDeleted;
        dupObj.OnDuplicateTrigger += HandleObjectDuplication;

        dupObj.transform.SetParent(parent);
        dupObj.SetImageSprite(sprite);
        dupObj.SetIsSetPlaced(false); // do not consider as a set
        _spawnedObjects.Add(dupObj);

        return dupObj;
    }

    public List<PaintObject> GetPaintObjects()
    {
        _spawnedObjects.Sort((o1, o2) => o1.transform.GetSiblingIndex().CompareTo(o2.transform.GetSiblingIndex()));

        return _spawnedObjects;
    }

    public Rect GetRectTransformRect()
    {
        RectTransform rectTransform = transform as RectTransform;
        Vector3[] worldCorners = new Vector3[4];
        Vector3[] screenCorners = new Vector3[4];

        rectTransform.GetWorldCorners(worldCorners);

        for(int i = 0; i < worldCorners.Length; i++)
        {
            screenCorners[i] = RectTransformUtility.WorldToScreenPoint(Camera.main, worldCorners[i]);
        }

        Bounds bounds = new Bounds(screenCorners[0], Vector3.zero);

        for(int i = 1; i < 4; ++i)
        {
            bounds.Encapsulate(screenCorners[i]);
        }

        Rect screenRect = new Rect(bounds.min, bounds.size);

        return screenRect;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ReturnToPool()
    {
        //FrameButton.transform.SetParent(null);
        //Destroy(FrameButton.gameObject);
        //Destroy(this.gameObject);

        UnsubscribeObjectsInFrame();
        PoolManager.Instance.ReturnPaintFrameButton(FrameButton.gameObject);
        PoolManager.Instance.ReturnPaintFrame(this.gameObject);
    }

    
    private void UnsubscribeObjectsInFrame()
    {
        foreach (PaintObject spawnedObject in _spawnedObjects)
        {
            spawnedObject.OnObjectSelected -= HandleObjectSelected;
            spawnedObject.OnObjectDragged -= HandleObjectDragged;
            spawnedObject.OnObjectDeleted -= HandleObjectDeleted;
            spawnedObject.OnDuplicateTrigger -= HandleObjectDuplication;
        }
    }
}
