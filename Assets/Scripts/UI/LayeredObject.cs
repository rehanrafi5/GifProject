using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LayeredObject : DragDropScrollItem
{
    [SerializeField] private Toggle _toggle;
    [SerializeField] private Image _image;
    [SerializeField] private Image _selectedImage;
    //[SerializeField] private GameObject _placeholderPrefab;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Toggle _lockToggle;
    [SerializeField] private LockIndicator _lockIndicator;
    [SerializeField] private RectTransform _controlsParent;
    [SerializeField] private Animator _animator;

    // Swipe detection variables
    private Vector2 _swipeStartPos;
    private bool _isSwipingHorizontal = false;
    private float _swipeThreshold = 50f;
    private float _maxSwipeDistance = 200f;

    public Action<int, int> OnLayerChange;
    public Action<int> OnObjectDeselected;
    public Action<int> OnObjectSelected;
    public Action<int, bool> OnObjectLocked;
    public Action<LayeredObject, float> OnSwipeDetected;

    private Dictionary<FrameObject, bool> frameLockStatus;

    protected override void Awake()
    {
        base.Awake();
        frameLockStatus = new Dictionary<FrameObject, bool>();
    }

    private void Start()
    {
        _toggle.onValueChanged.AddListener(HandleObjectClicked);
        _lockToggle.onValueChanged.AddListener(HandleObjectLocked);
    }

    private void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(HandleObjectClicked);
        _lockToggle.onValueChanged.RemoveListener(HandleObjectLocked);
    }

    public void SetToggleGroup(ToggleGroup group)
    {
        _toggle.group = group;
    }

    public void SetSprite(Sprite sprite)
    {
        _image.sprite = sprite;
    }

    public void SetSelected(bool isSelected)
    {
        _toggle.onValueChanged.RemoveListener(HandleObjectClicked);
        _toggle.isOn = isSelected;
        _toggle.onValueChanged.AddListener(HandleObjectClicked);
    }

    public void Unlock()
    {
        _lockToggle.isOn = false;
        _lockIndicator.UpdateLockIcon(false);
        SaveLockStatusOnFrame(false);
    }

    public void Lock()
    {
        _lockToggle.isOn = true;
        _lockIndicator.UpdateLockIcon(true);
        SaveLockStatusOnFrame(true);
    }

    public bool GetLockStatus()
    {
        var frame = FrameManager.Instance.CurrentFrame;
        bool lockStatus = false;
        if (frame != null && frameLockStatus.ContainsKey(frame))
        {
            lockStatus = frameLockStatus[frame];
        }

        return lockStatus;
    }

    public void RefreshLockIndicator()
    {
        bool lockStatus = GetLockStatus();

        _lockToggle.isOn = lockStatus;
        _lockIndicator.UpdateLockIcon(lockStatus);
    }

    public bool CopyLockStatus(FrameObject baseFrame, FrameObject dupFrame)
    {
        bool lockStatus = false;

        if (baseFrame != null && frameLockStatus.ContainsKey(baseFrame))
        {
            lockStatus = frameLockStatus[baseFrame];
        }
        SaveLockStatusOnFrame(lockStatus, dupFrame);

        _lockToggle.isOn = lockStatus;
        _lockIndicator.UpdateLockIcon(lockStatus);

        return lockStatus;
    }

    public RectTransform GetRectTransform()
    {
        return _rectTransform;
    }

    public RectTransform GetControlsParent()
    {
        return _controlsParent;
    }

    public Animator GetAnimator()
    {
        return _animator;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        _swipeStartPos = eventData.position;
        _isSwipingHorizontal = false;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        // Determine if this is a horizontal swipe
        Vector2 delta = eventData.position - _swipeStartPos;

        // If we've moved more horizontally than vertically, and it's not a long press
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && !longPressTriggered)
        {
            // For right-to-left swipe, delta.x should be negative
            if (delta.x < 0)
            {
                _isSwipingHorizontal = true;
                _canvasGroup.blocksRaycasts = false;

                // Notify manager that a swipe has started
                float swipeAmount = Mathf.Clamp01(Mathf.Abs(delta.x) / _maxSwipeDistance);
                OnSwipeDetected?.Invoke(this, swipeAmount);

                // Don't pass to base handlers
                return;
            }
        }

        isDragging = true;
        // If it's not a horizontal swipe, handle as normal
        if (longPressTriggered)
        {
            OnBeginDragLong(eventData);
        }
        else
        {
            base.OnBeginDragShort(eventData);
        }
    }

    protected override void OnBeginDragLong(PointerEventData eventData)
    {
        base.OnBeginDragLong(eventData);
        _canvasGroup.blocksRaycasts = false;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (_isSwipingHorizontal)
        {
            //// Calculate swipe amount (0-1 representing progress)
            //float swipeDelta = _swipeStartPos.x - eventData.position.x;
            //float swipeAmount = Mathf.Clamp01(swipeDelta / _maxSwipeDistance);

            //// Notify manager about swipe progress
            //OnSwipeDetected?.Invoke(this, swipeAmount);

            // Don't pass to base handlers
            return;
        }

        isDragging = true;
        // Default behavior for non-swipe
        if (longPressTriggered)
        {
            OnDragLong(eventData);
        }
        else
        {
            base.OnDragShort(eventData);
        }
    }

    protected override void OnDragLong(PointerEventData eventData)
    {
        base.OnDragLong(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (_isSwipingHorizontal)
        {
            //// Check if swipe was far enough to show controls
            //float swipeDelta = _swipeStartPos.x - eventData.position.x;

            //if (swipeDelta >= _swipeThreshold)
            //{
            //    // Notify manager to fully show controls
            //    OnSwipeDetected?.Invoke(this, 1.0f);
            //}
            //else
            //{
            //    // Swipe wasn't far enough, notify manager to hide controls
            //    OnSwipeDetected?.Invoke(this, 0f);
            //}


            _isSwipingHorizontal = false;
            _canvasGroup.blocksRaycasts = true;
            return;
        }

        isDragging = false;
        // Default behavior for non-swipe
        if (longPressTriggered)
        {
            OnEndDragLong(eventData);
            longPressTriggered = false;
        }
        else
        {
            base.OnEndDragShort(eventData);
        }
    }

    protected override void OnEndDragLong(PointerEventData eventData)
    {
        base.OnEndDragLong(eventData);
        _canvasGroup.blocksRaycasts = true;
    }

    protected override void InvokeSwapItem(int index)
    {
        OnLayerChange?.Invoke(_previousIndex, index);
    }

    public void ReorderLayer(int index)
    {
        int prevIndex = transform.GetSiblingIndex();
        transform.SetSiblingIndex(index);

        if (prevIndex != index)
        {
            OnLayerChange?.Invoke(prevIndex, index);
        }
    }

    private void HandleObjectClicked(bool isSelected)
    {
        int index = transform.GetSiblingIndex();

        if(isSelected)
        {
            OnObjectSelected?.Invoke(index);
        }

        else if(_toggle.group.AnyTogglesOn() == false)
        {
            OnObjectDeselected?.Invoke(index);
        }
    }

    private void HandleObjectLocked(bool isSelected)
    {
        int index = transform.GetSiblingIndex();
        _lockIndicator.UpdateLockIcon(isSelected);
        SaveLockStatusOnFrame(isSelected);

        if(isSelected)
        {
            OnObjectLocked?.Invoke(index, true);
        }

        else
        {
            OnObjectLocked?.Invoke(index, false);
        }
    }

    private void SaveLockStatusOnFrame(bool isLocked, FrameObject frame = null)
    {
        if (frame == null)
        {
            frame = FrameManager.Instance.CurrentFrame;
        }

        if (frame == null)
        {
            return;
        }

        if (frameLockStatus.ContainsKey(frame))
        {
            frameLockStatus[frame] = isLocked;
        }
        else
        {
            frameLockStatus.Add(frame, isLocked);
        }
    }
}
