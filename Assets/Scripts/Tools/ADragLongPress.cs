using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ADragLongPress : DragDrop, IPointerUpHandler, IPointerExitHandler
{
    protected bool longPressTriggered = false;
    protected bool isPointerDown = false;
    protected bool isDragging = false;

    private float longPressDuration = 0.3f;
    private Coroutine longPressCoroutine;
    private Vector2 normalSize;
    protected Vector2 enlargedSize;
    private Vector2 addSize = new Vector2(10, 10);

    public Action OnLongPress;

    protected override void Awake()
    {
        base.Awake();
        normalSize = _rectTransform.sizeDelta;
        enlargedSize = normalSize + addSize;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Reset();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Reset();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        longPressCoroutine = StartCoroutine(LongPressRoutine());
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        if (longPressTriggered)
        {
            OnBeginDragLong(eventData);
        }
        else
        {
            OnBeginDragShort(eventData);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        if (longPressTriggered)
        {
            OnDragLong(eventData);
        }
        else
        {
            OnDragShort(eventData);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        if (longPressTriggered)
        {
            OnEndDragLong(eventData);
            longPressTriggered = false;
        }
        else
        {
            OnEndDragShort(eventData);
        }
    }

    protected abstract void OnBeginDragLong(PointerEventData eventData);
    protected abstract void OnBeginDragShort(PointerEventData eventData);
    protected abstract void OnDragLong(PointerEventData eventData);
    protected abstract void OnDragShort(PointerEventData eventData);
    protected abstract void OnEndDragLong(PointerEventData eventData);
    protected abstract void OnEndDragShort(PointerEventData eventData);

    private void Reset()
    {
        isPointerDown = false;
        _rectTransform.sizeDelta = normalSize;
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
            longPressCoroutine = null;
        }
    }

    private IEnumerator LongPressRoutine()
    {
        yield return new WaitForSeconds(longPressDuration);
        if (isPointerDown && !isDragging)
        {
            longPressTriggered = true;
            _rectTransform.sizeDelta = enlargedSize;
            OnLongPress?.Invoke();
        }
    }
}
