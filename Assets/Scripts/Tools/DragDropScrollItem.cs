using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropScrollItem : ADragLongPress
{
    private ScrollRect scrollRect;

    protected int _previousIndex;
    protected GameObject _visualPlaceholder;
    protected Transform _parentToReturnTo = null;
    protected Transform _placeholderParent = null;

    private ScrollRect ScrollRect
    {
        get
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponentInParent<ScrollRect>();
            }
            return scrollRect;
        }
    }

    private float edgeThresholdSlow = 100f; // Distance from edge for slow scrolling
    private float edgeThresholdFast = 50f;  // Distance from edge for fast scrolling
    private float scrollSpeedSlow = 3f;     // Slow scroll speed
    private float scrollSpeedFast = 10f;    // Fast scroll speed

    private bool hasAutoScrolled = false;

    protected override void Awake()
    {
        base.Awake();
        scrollRect = GetComponentInParent<ScrollRect>();
    }

    protected override void OnBeginDragShort(PointerEventData eventData)
    {
        ScrollRect.OnBeginDrag(eventData);
    }

    protected override void OnDragShort(PointerEventData eventData)
    {
        ScrollRect.OnDrag(eventData);
    }

    protected override void OnEndDragShort(PointerEventData eventData)
    {
        ScrollRect.OnEndDrag(eventData);
    }

    protected override void OnBeginDragLong(PointerEventData eventData)
    {
        
        _previousIndex = transform.GetSiblingIndex();

        //_visualPlaceholder = Instantiate(_placeholderPrefab);
        _visualPlaceholder = PoolManager.Instance.TakeDragPlaceholder();
        _visualPlaceholder.transform.SetParent(transform.parent);
        _visualPlaceholder.transform.SetSiblingIndex(_previousIndex);

        _parentToReturnTo = transform.parent;
        _placeholderParent = _parentToReturnTo;

        transform.SetParent(transform.parent.parent);
        _rectTransform.sizeDelta = enlargedSize;
        //_canvasGroup.blocksRaycasts = false;
    }

    protected override void OnDragLong(PointerEventData eventData)
    {
        // Drag movement
        Vector3 vec = _mainCamera.WorldToScreenPoint(_rectTransform.position);
        vec.x += eventData.delta.x;
        vec.y += eventData.delta.y;
        _rectTransform.position = _mainCamera.ScreenToWorldPoint(vec);

        // Auto-scroll functionality
        HandleAutoScroll(eventData);

        // Sibling Index Manipulation
        int newSiblingIndex = _placeholderParent.childCount;

        for (int i = 0; i < _placeholderParent.childCount; i++)
        {
            if ((ScrollRect.horizontal && !ScrollRect.vertical
                && transform.position.x < _placeholderParent.GetChild(i).position.x)
            || (ScrollRect.vertical && !ScrollRect.horizontal
                && transform.position.y < _placeholderParent.GetChild(i).position.y))
            {
                newSiblingIndex = i;

                if (_visualPlaceholder.transform.GetSiblingIndex() < newSiblingIndex)
                {
                    newSiblingIndex--;
                }
                break;
            }
        }

        _visualPlaceholder.transform.SetSiblingIndex(newSiblingIndex);
    }

    protected override void OnEndDragLong(PointerEventData eventData)
    {
        int currentIndex = _visualPlaceholder.transform.GetSiblingIndex();

        transform.SetParent(_parentToReturnTo);
        transform.SetSiblingIndex(currentIndex);
        //_canvasGroup.blocksRaycasts = true;

        //Destroy(_visualPlaceholder);
        PoolManager.Instance.ReturnDragPlaceholder(_visualPlaceholder);

        if (_previousIndex != currentIndex)
        {
            InvokeSwapItem(currentIndex);
        }
    }

    protected virtual void InvokeSwapItem(int index) { }

    private void HandleAutoScroll(PointerEventData eventData)
    {
        // Get the ScrollRect's viewport rect in screen space
        RectTransform viewportRectTransform = ScrollRect.viewport;
        if (viewportRectTransform == null) viewportRectTransform = ScrollRect.GetComponent<RectTransform>();

        // Convert viewport corners to screen space
        Vector3[] corners = new Vector3[4];
        viewportRectTransform.GetWorldCorners(corners);

        // Convert to screen space
        for (int i = 0; i < 4; i++)
        {
            corners[i] = RectTransformUtility.WorldToScreenPoint(_mainCamera, corners[i]);
        }

        // Calculate viewport bounds in screen space
        float minX = corners[0].x;
        float maxX = corners[2].x;
        float minY = corners[0].y;
        float maxY = corners[2].y;

        // Current mouse position from the event data
        Vector2 mousePosition = eventData.position;

        // Calculate scroll direction and speed for vertical scrolling
        float verticalScrollAmount = 0;

        // Bottom edge detection
        if (mousePosition.y - minY < edgeThresholdSlow)
        {
            // Determine speed based on how close to the edge
            if (mousePosition.y - minY < edgeThresholdFast)
                verticalScrollAmount = -scrollSpeedFast * Time.deltaTime;
            else
                verticalScrollAmount = -scrollSpeedSlow * Time.deltaTime;
        }
        // Top edge detection
        else if (maxY - mousePosition.y < edgeThresholdSlow)
        {
            // Determine speed based on how close to the edge
            if (maxY - mousePosition.y < edgeThresholdFast)
                verticalScrollAmount = scrollSpeedFast * Time.deltaTime;
            else
                verticalScrollAmount = scrollSpeedSlow * Time.deltaTime;
        }

        // Calculate scroll direction and speed for horizontal scrolling
        float horizontalScrollAmount = 0;

        // Left edge detection
        if (mousePosition.x - minX < edgeThresholdSlow)
        {
            // Determine speed based on how close to the edge
            if (mousePosition.x - minX < edgeThresholdFast)
                horizontalScrollAmount = -scrollSpeedFast * Time.deltaTime;
            else
                horizontalScrollAmount = -scrollSpeedSlow * Time.deltaTime;
        }
        // Right edge detection
        else if (maxX - mousePosition.x < edgeThresholdSlow)
        {
            // Determine speed based on how close to the edge
            if (maxX - mousePosition.x < edgeThresholdFast)
                horizontalScrollAmount = scrollSpeedFast * Time.deltaTime;
            else
                horizontalScrollAmount = scrollSpeedSlow * Time.deltaTime;
        }

        // Apply the scrolling
        if (ScrollRect.vertical && verticalScrollAmount != 0)
        {
            Vector2 newNormalizedPosition = ScrollRect.normalizedPosition;
            newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y + verticalScrollAmount);
            ScrollRect.normalizedPosition = newNormalizedPosition;
        }

        if (ScrollRect.horizontal && horizontalScrollAmount != 0)
        {
            Vector2 newNormalizedPosition = ScrollRect.normalizedPosition;
            newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x + horizontalScrollAmount);
            ScrollRect.normalizedPosition = newNormalizedPosition;
        }
    }
}
