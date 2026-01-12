using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayerPanel : MonoBehaviour
{
    [SerializeField] private RectTransform swipeControlsPanel;

    [SerializeField] private GameObject _layeredObjectPrefab;
    [SerializeField] private Transform _layeredObjectParent;
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private Button _deleteButton;
    [SerializeField] private Button _duplicateButton;
    [SerializeField] private Button _lockAllButton;
    [SerializeField] private Button _unlockAllButton;

    [Space]
    [SerializeField] private string activateOptionsAnimName = "ActivateOptions";
    [SerializeField] private string deactivateOptionsAnimName = "DeactivateOptions";
    [SerializeField] private string idleAnimName = "Idle";

    private int _selectedObject;
    private List<PaintObject> _paintObjects = new List<PaintObject>();
    private List<LayeredObject> _layeredObjects = new List<LayeredObject>();

    private LayeredObject _cachedLayeredObject = null;

    public Action<int, int> OnLayerChange;
    public Action<PaintObject> OnObjectDeletion;

    // Reference to the currently active layer with swipe controls showing
    private LayeredObject activeSwipeLayer;

    private void Awake()
    {
        if (swipeControlsPanel != null)
        {
            swipeControlsPanel.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        _deleteButton.onClick.AddListener(DeleteSelected);
        _duplicateButton.onClick.AddListener(DuplicateSelected);
        _lockAllButton.onClick.AddListener(LockAllLayers);
        _unlockAllButton.onClick.AddListener(UnlockAllLayers);
        FrameManager.Instance.OnDeleteFrame += DeleteLayers;
    }

    private void OnDestroy()
    {
        _deleteButton.onClick.RemoveListener(DeleteSelected);
        _duplicateButton.onClick.RemoveListener(DuplicateSelected);
        _lockAllButton.onClick.RemoveListener(LockAllLayers);
        _unlockAllButton.onClick.RemoveListener(UnlockAllLayers);
        FrameManager.Instance.OnDeleteFrame -= DeleteLayers;
    }

    public void PopulateObjects(List<PaintObject> paintObjects)
    {
        _paintObjects = new List<PaintObject>();

        Clear();

        for(int i = 0; i < paintObjects.Count; i++)
        {
            AddObject(paintObjects[i], i);
        }
    }

    public void AddObject(PaintObject paintObject, int index,
        FrameObject baseFrame = null, FrameObject dupFrame = null)
    {
        LayeredObject layeredObject = null;
        _paintObjects.Add(paintObject);

        if(index >= _layeredObjects.Count)
        {
            //GameObject layeredObjectInstance = Instantiate(_layeredObjectPrefab, _layeredObjectParent);
            //layeredObject = layeredObjectInstance.GetComponent<LayeredObject>();
            layeredObject = PoolManager.Instance.TakePaintLayer();
            layeredObject.transform.SetParent(_layeredObjectParent);

            layeredObject.OnLayerChange += HandleLayerChanged;
            layeredObject.OnObjectSelected += HandleObjectSelected;
            layeredObject.OnObjectDeselected += HandleObjectDeselected;
            layeredObject.OnObjectLocked += HandleObjectLocked;
            layeredObject.OnSwipeDetected += HandleLayerSwiped;
            layeredObject.OnLongPress += HideSwipeControls;
            layeredObject.SetToggleGroup(_toggleGroup);
            _layeredObjects.Add(layeredObject);
        }

        else
        {
            layeredObject = _layeredObjects[index];
            //layeredObject.Unlock();
            layeredObject.gameObject.SetActive(true);
        }

        layeredObject.SetSprite(paintObject.GetImageSprite());

        if (baseFrame != null && dupFrame != null)
        {
            paintObject.IsLocked = layeredObject.CopyLockStatus(baseFrame, dupFrame);
        }
        else
        {
            layeredObject.RefreshLockIndicator();
        }
        _cachedLayeredObject = layeredObject;
    }

    public LayeredObject GetCachedLayeredObject()
    {
        return _cachedLayeredObject;
    }

    public void SetSelected(PaintObject paintObject)
    {
        int index = _paintObjects.IndexOf(paintObject);

        _selectedObject = index;
        UpdateSelected();
        if (activeSwipeLayer != null)
        {
            HideSwipeControls(true);
        }
    }

    public void DeleteObject(PaintObject paintObject)
    {
        int objectIndex = _paintObjects.FindIndex((p) => p == paintObject);

        if(objectIndex >= 0)
        {
            //Destroy(_layeredObjects[objectIndex].gameObject);
            _layeredObjects[objectIndex].Unlock();
            UnsubscribeLayerListener(_layeredObjects[objectIndex]);
            PoolManager.Instance.ReturnPaintLayer(_layeredObjects[objectIndex].gameObject);

            _paintObjects[objectIndex].Delete();
            _layeredObjects.RemoveAt(objectIndex);
            _paintObjects.RemoveAt(objectIndex);

            if(_selectedObject == objectIndex)
            {
                ObjectManager.Instance.DeselectObject();
            }
        }
    }

    private void UnsubscribeLayerListener(LayeredObject layeredObject)
    {
        layeredObject.OnLayerChange -= HandleLayerChanged;
        layeredObject.OnObjectSelected -= HandleObjectSelected;
        layeredObject.OnObjectDeselected -= HandleObjectDeselected;
        layeredObject.OnObjectLocked -= HandleObjectLocked;
        layeredObject.OnSwipeDetected -= HandleLayerSwiped;
        layeredObject.OnLongPress -= HideSwipeControls;
    }

    public void SetDeselected()
    {
        if (activeSwipeLayer != null)
        {
            HideSwipeControls(false);
        }

        _selectedObject = -1;
        UpdateSelected();
    }

    public void ToggleVisibility()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UpdateSelected()
    {
        for(int i = 0; i < _layeredObjects.Count; i++)
        {
            _layeredObjects[i].SetSelected(i == _selectedObject);
        }
    }

    private void HandleLayerChanged(int previousIndex, int newIndex)
    {
        PaintObject paintObject = _paintObjects[previousIndex];

        paintObject.transform.SetSiblingIndex(newIndex);

        _paintObjects.Sort((o1, o2) => o1.transform.GetSiblingIndex().CompareTo(o2.transform.GetSiblingIndex()));
        _layeredObjects.Sort((o1, o2) => o1.transform.GetSiblingIndex().CompareTo(o2.transform.GetSiblingIndex()));

        OnLayerChange?.Invoke(previousIndex, newIndex);
    }

    private void HandleObjectSelected(int index)
    {
        PaintObject paintObject = _paintObjects[index];

        if(paintObject.IsLocked == false)
        {
            ObjectManager.Instance.SelectObject(paintObject);
        }

        else if(_selectedObject != index && _selectedObject >= 0)
        {
            ObjectManager.Instance.SelectObject(_paintObjects[_selectedObject]);
        }

        else
        {
            ObjectManager.Instance.DeselectObject();
        }
    }

    private void HandleObjectDeselected(int index)
    {
        PaintObject paintObject = _paintObjects[index];

        if(paintObject == ObjectManager.Instance.SelectedObject)
        {
            ObjectManager.Instance.DeselectObject();
        }
    }

    private void DeleteSelected()
    {
        if (activeSwipeLayer != null)
        {
            HideSwipeControls(true);
        }
        if (_selectedObject >= 0 && _selectedObject < _paintObjects.Count)
        {
            OnObjectDeletion?.Invoke(_paintObjects[_selectedObject]);
        }
    }

    private void DuplicateSelected()
    {
        if (activeSwipeLayer != null)
        {
            int layerIndex = activeSwipeLayer.transform.GetSiblingIndex();
            _paintObjects[layerIndex].Duplicate();
            HideSwipeControls(true);
        }
        else if (_selectedObject >= 0 && _selectedObject < _paintObjects.Count)
        {
            _paintObjects[_selectedObject].Duplicate();
        }
    }

    private void HandleObjectLocked(int index, bool isLocked)
    {
        _paintObjects[index].IsLocked = isLocked;

        if (activeSwipeLayer != null)
        {
            HideSwipeControls(false);
        }

        if (index == _selectedObject)
        {
            ObjectManager.Instance.DeselectObject();
        }
    }

    private void Clear()
    {
        for(int i = 0; i < _layeredObjects.Count; i++)
        {
            _layeredObjects[i].gameObject.SetActive(false);
        }
    }

    private void DeleteLayers()
    {
        while(_paintObjects.Count > 0)
        {
            DeleteObject(_paintObjects[0]);
        }
    }

    private void LockAllLayers()
    {
        if (activeSwipeLayer != null)
        {
            HideSwipeControls(false);
        }
        foreach (LayeredObject layer in _layeredObjects)
        {
            if (layer.gameObject.activeInHierarchy)
            {
                layer.Lock();
            }
        }
    }

    private void UnlockAllLayers()
    {
        if (activeSwipeLayer != null)
        {
            HideSwipeControls(false);
        }
        foreach (LayeredObject layer in _layeredObjects)
        {
            if (layer.gameObject.activeInHierarchy)
            {
                layer.Unlock();
            }
        }
    }

    // Called when a layer object detects a right-to-left swipe
    public void HandleLayerSwiped(LayeredObject layer, float swipeAmount)
    {
        // If we have an active layer with controls showing, and it's different from the current one
        if (activeSwipeLayer != null && activeSwipeLayer != layer)
        {
            HandleObjectDeselected(activeSwipeLayer.transform.GetSiblingIndex());
            HideSwipeControls(true);
        }

        // auto-select it before anything else.
        HandleObjectSelected(layer.transform.GetSiblingIndex());

        // Set this as the active layer
        activeSwipeLayer = layer;

        // Position the controls panel next to this layer
        PositionControlsPanel(layer);

        // If not active, activate it
        if (!swipeControlsPanel.gameObject.activeSelf)
        {
            swipeControlsPanel.gameObject.SetActive(true);
            swipeControlsPanel.anchoredPosition = new Vector2(0, 0);
        }

        ShowSwipeControls();
    }

    // Called when a different gesture starts or user taps elsewhere
    public void HideSwipeControls(bool immediate)
    {
        if (activeSwipeLayer != null)
        {
            if (immediate)
            {
                activeSwipeLayer.GetAnimator().Play(idleAnimName);
            }
            else
            {
                activeSwipeLayer.GetAnimator().Play(deactivateOptionsAnimName);
            }

            activeSwipeLayer = null;
            swipeControlsPanel.gameObject.SetActive(false);
        }
    }

    private void HideSwipeControls()
    {
        HideSwipeControls(true);
    }

    // Fully show the controls panel
    private void ShowSwipeControls()
    {
        if (activeSwipeLayer != null)
        {
            activeSwipeLayer.GetAnimator().Play(activateOptionsAnimName);

            bool isActiveSwipeLayerLocked = activeSwipeLayer.GetLockStatus();
            //_deleteButton.gameObject.SetActive(!isActiveSwipeLayerLocked);
            _deleteButton.interactable = !isActiveSwipeLayerLocked;
        }
    }

    private void PositionControlsPanel(LayeredObject layer)
    {
        // Set the swipe panel as a child of the layer to properly position it
        swipeControlsPanel.SetParent(layer.GetControlsParent().transform);

        // Reset local position and scale
        swipeControlsPanel.localScale = Vector3.one;

        // Position at the right edge of the layer
        swipeControlsPanel.localPosition = new Vector3(0, 0, 0);

        // Make sure it's at the top of the hierarchy to draw above other elements
        swipeControlsPanel.SetAsLastSibling();
    }
}
