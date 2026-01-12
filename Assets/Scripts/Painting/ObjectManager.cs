using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ObjectManager : Singleton<ObjectManager>
{
    [SerializeField] private TMP_Dropdown _categoryDropdown;
    [Tooltip("Also known as the Objects")]
    [SerializeField] private List<ItemDetail> _actorItemsDetails = new List<ItemDetail>();
    [SerializeField] private List<ItemDetail> _backgroundItemsDetails = new List<ItemDetail>();
    [SerializeField] private List<ItemDetail> _shapeItemsDetails = new List<ItemDetail>();
    [SerializeField] private List<ItemDetail> _setItemsDetails = new List<ItemDetail>();
    [SerializeField] private GameObject _objectPrefab;
    [SerializeField] private LayerPanel _layerPanel;
    [SerializeField] private ControlObject _controls;
    [SerializeField] private Button _layersButton;
    [SerializeField] private Transform _infoPanel;
    [SerializeField] private Transform _setParent;

    private PaintObject _temp = null;
    private LayeredObject _tempLayer = null;
    private GameObject _tempSet = null;
    private int _setCounter = 0;
    private Camera mainCam;

    public PaintObject SelectedObject { protected set; get; }

    public Action OnObjectAdded;
    public Action OnObjectSelected;
    public Action OnObjectModified;
    public Action OnObjectDeleted;

    private Dictionary<string, int> _categoryDict = new Dictionary<string, int>();

    protected override void Awake()
    {
        base.Awake();
        mainCam = Camera.main;
    }

    private void Start()
    {
        PopulateCategories();
        _categoryDropdown.onValueChanged.AddListener(OnChangeCategory);

        _controls.gameObject.SetActive(false);
        _layersButton.onClick.AddListener(ToggleLayerPanel);
        _layerPanel.gameObject.SetActive(false);
        _layerPanel.OnObjectDeletion += HandleObjectDeletion;

        SubscribeSpawnHandler(_actorItemsDetails);
        SubscribeSpawnHandler(_backgroundItemsDetails);
        SubscribeSpawnHandler(_shapeItemsDetails);
        SubscribeSpawnHandler(_setItemsDetails);

        _controls.OnObjectModified += HandleObjectModified;
        _controls.OnSetPlaced += AfterSetHandle;
    }

    private void OnDestroy()
    {
        UnsubscribeSpawnHandler(_actorItemsDetails);
        UnsubscribeSpawnHandler(_backgroundItemsDetails);
        UnsubscribeSpawnHandler(_shapeItemsDetails);
        UnsubscribeSpawnHandler(_setItemsDetails);

        _layerPanel.OnObjectDeletion -= HandleObjectDeletion;
        _layersButton.onClick.RemoveListener(ToggleLayerPanel);
        _controls.OnObjectModified -= HandleObjectModified;
        _controls.OnSetPlaced -= AfterSetHandle;
    }

    private void PopulateCategories()
    {
        _categoryDict = Enum.GetValues(typeof(Category))
               .Cast<Category>()
               .ToDictionary(t => t.ToString(), t => (int)t);

        _categoryDropdown.ClearOptions();
        _categoryDropdown.AddOptions(_categoryDict.Keys.ToList());
    }

    private void OnChangeCategory(int value)
    {
        int realValue = _categoryDict.Values.ElementAt(value);

        // hide/show items based on the category
        ManageVisibleItems(_actorItemsDetails, realValue);
        ManageVisibleItems(_backgroundItemsDetails, realValue);
        ManageVisibleItems(_shapeItemsDetails, realValue);
        ManageVisibleItems(_setItemsDetails, realValue);
    }

    private void ManageVisibleItems(List<ItemDetail> items, int selectedCategory)
    {
        bool checker = false;
        foreach(ItemDetail item in items)
        {
            checker = false;

            foreach(Category category in item.Categories)
            {
                if ((int)category == selectedCategory)
                {
                    checker = true;
                    break;
                }
            }

            if (checker)
            {
                item.Item.gameObject.SetActive(true);
            }
            else
            {
                item.Item.gameObject.SetActive(false);
            }
        }
    }

    private void SubscribeSpawnHandler(List<ItemDetail> items)
    {
        foreach (ItemDetail item in items)
        {
            if (item.Item.IsSet)
            {
                item.Item.OnSetSpawned += HandleSetSpawned;
            }
            else
            {
                item.Item.OnObjectSpawned += HandleObjectSpawned;
            }
        }
    }

    private void UnsubscribeSpawnHandler(List<ItemDetail> items)
    {
        foreach (ItemDetail item in items)
        {
            if (item.Item.IsSet)
            {
                item.Item.OnSetSpawned -= HandleSetSpawned;
            }
            else
            {
                item.Item.OnObjectSpawned -= HandleObjectSpawned;
            }
        }
    }

    public void PopulateObjects(List<PaintObject> paintObjects)
    {
        _layerPanel.PopulateObjects(paintObjects);
    }

    private void HandleObjectSpawned(PointerEventData e, Sprite sprite, float defaultSize)
    {
        AddObject(sprite, defaultSize, Input.mousePosition);
        SetupControls(e);
        _controls.IsDraggingSet(false, 0f);
    }

    private void HandleSetSpawned(PointerEventData e, Sprite sprite, GameObject setObjects, float defaultSize)
    {
        _temp = AddObject(sprite, defaultSize, Input.mousePosition);
        _tempLayer = GetCachedLayeredObject();
        _temp.SetImageAlpha(0f); // For Set, the image should be invisible
        _tempSet = setObjects;
        _tempSet.transform.SetParent(_temp.transform, false);
        _controls.IsDraggingSet(true, defaultSize);

        SetupControls(e);
    }

    private void AfterSetHandle(float defaultSize)
    {
        if(_tempSet == null)
        {
            return;
        }

        Vector3 tempPos = mainCam.WorldToScreenPoint(_temp.transform.position);
        tempPos.z = 0;

        // UNDO THE TEMP
        _temp.SetImageAlpha(255f);
        UndoManager.Instance.Undo();
        //PoolManager.Instance.ReturnPaintObject(_temp.gameObject);
        //PoolManager.Instance.ReturnPaintLayer(_tempLayer.gameObject);

        // Record Set
        _setCounter++;

        // PAINT EACH OBJECT IN THE SET
        foreach (Transform child in _tempSet.transform)
        {
            if (child.TryGetComponent<PaintObject>(out PaintObject paintObject))
            {
                // ADD AND SELECT OBJECT
                RectTransform rect = child as RectTransform;
                PaintObject obj = AddObjectForSet(paintObject.GetImageSprite(), rect.rect.width, tempPos, rect.localPosition, rect.rotation, _setCounter);

                // SAVE THIS IN THE UNDO MANAGER BY INVOKING OnObjectModified
                HandleObjectModified();
            }
        }

        _tempSet.transform.SetParent(_setParent);
        _tempSet.transform.localPosition = Vector2.zero;
        //DeleteObject(_temp);
        _controls.IsDraggingSet(false, 0f);
    }

    public void SetupControls(PointerEventData e)
    {
        e.pointerDrag = _controls.gameObject;
    }

    /// <summary>
    /// This increments the set counter first before returning it.
    /// DGS This is a helper to Frame Manager when duplicating a frame.
    /// </summary>
    /// <returns></returns>
    public int GetSetCounter()
    {
        _setCounter++;
        return _setCounter;
    }

    // Not used. Replaced by FrameManager Duplicate Object
    //public PaintObject DuplicateObject(PaintObject sourceObject)
    //{
    //    Sprite sprite = sourceObject.GetImageSprite();
    //    float defaultSize = (sourceObject.transform as RectTransform).rect.width;
    //    Vector2 position = Camera.main.WorldToScreenPoint(sourceObject.transform.position);
    //    position += new Vector2(20.0f, -20.0f);

    //    return AddObject(sprite, defaultSize, position);
    //}

    public PaintObject RecreateObject(Sprite sprite)
    {
        PaintObject paintObject = AddObject(sprite, 1f, Vector2.zero);
        DeselectObject();

        return paintObject;
    }

    private PaintObject AddObject(Sprite sprite, float defaultSize, Vector2 screenPosition)
    {
        PaintObject paintObject = FrameManager.Instance.AddObject(_objectPrefab, sprite, defaultSize, screenPosition);
        AddPaintObjectToLayer(paintObject);

        return paintObject;
    }

    
    private PaintObject AddObjectForSet(Sprite sprite, float defaultSize, Vector2 screenPosition, Vector2 offsetPos, Quaternion rotation, int setGroup)
    {
        PaintObject paintObject = FrameManager.Instance.AddObjectForSet(_objectPrefab, sprite, defaultSize, screenPosition, offsetPos, rotation, setGroup);
        AddPaintObjectToLayer(paintObject);

        return paintObject;
    }

    private void AddPaintObjectToLayer(PaintObject paintObject,
        FrameObject baseFrame = null, FrameObject dupFrame = null)
    {
        _layerPanel.AddObject(paintObject, paintObject.transform.GetSiblingIndex(),
            baseFrame, dupFrame);
        SelectObject(paintObject);
        OnObjectAdded?.Invoke();
    }

    /// <summary>
    /// DGS This is a helper to Frame Manager when duplicating a frame.
    /// </summary>
    /// <param name="clonedObject"></param>
    public void AddClonedObjectToLayer(PaintObject clonedObject,
        FrameObject baseFrame, FrameObject dupFrame)
    {
        AddPaintObjectToLayer(clonedObject, baseFrame, dupFrame);

        // if not cloned / entirely new
        if (baseFrame == null)
        {
            OnObjectModified?.Invoke();
        }
    }

    public LayeredObject GetCachedLayeredObject()
    {
        return _layerPanel.GetCachedLayeredObject();
    }

    public void DeleteObject(PaintObject paintObject)
    {
        _layerPanel.DeleteObject(paintObject);
        OnObjectDeleted?.Invoke();
    }

    public void SelectObject(PaintObject paintObject)
    {
        if(paintObject.IsLocked == false)
        {
            _layerPanel.SetSelected(paintObject);
            _infoPanel.gameObject.SetActive(false);
            _controls.gameObject.SetActive(true);
            _controls.SetImageTransform(paintObject.GetRectTransform());

            SelectedObject = paintObject;
            OnObjectSelected?.Invoke();
        }
    }

    public void DeselectObject()
    {
        SelectedObject = null;

        _infoPanel.gameObject.SetActive(false);
        _controls.gameObject.SetActive(false);
        _layerPanel.SetDeselected();
    }

    public void RefreshControls()
    {
        _controls.Reset();
    }

    private void HandleObjectModified()
    {
        OnObjectModified?.Invoke();
    }

    private void HandleObjectDeletion(PaintObject paintObject)
    {
        paintObject.SetIsSetPlaced(false); // SET OBJECT Is Set Placed to FALSE
        UndoManager.Instance.SaveDeletionState();
        DeleteObject(paintObject);
    }

    private void ToggleLayerPanel()
    {
        _layerPanel.ToggleVisibility();
    }

    public void HideLayerPanel()
    {
        _layerPanel.Hide();
    }
}
