using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandleObject : DragDrop
{
    public Action<HandleObject> OnHandleDragged;
    public Action<HandleObject> OnHandleReleased;
    public Action<HandleObject> OnHandleStartDrag;

    public override void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, Input.mousePosition, _canvas.worldCamera, out Vector2 pos);
        _rectTransform.position = _canvas.transform.TransformPoint(pos);

        OnHandleDragged?.Invoke(this);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        OnHandleReleased?.Invoke(this);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        OnHandleStartDrag?.Invoke(this);
    }

    public void SetSize(float size)
    {
        _rectTransform.localScale = new Vector3(size, size, size);
    }

    public float GetSize()
    {
        return Mathf.Abs(_rectTransform.localScale.x);
    }

    public void SetPosition(Vector2 position)
    {
        _rectTransform.position = new Vector3(position.x, position.y, _rectTransform.position.z);
    }

    public Vector2 GetPosition()
    {
        return _rectTransform.position;
    }

    public void SetAnchoredPosition(Vector2 position)
    {
        _rectTransform.anchoredPosition = position;
    }

    public Vector2 GetAnchoredPosition()
    {
        return _rectTransform.anchoredPosition;
    }

    public Vector2 GetScreenPoint()
    {
        return RectTransformUtility.WorldToScreenPoint(Camera.main, _rectTransform.position);
    }

    public void Reset()
    {
        _rectTransform.anchoredPosition = Vector2.zero;
    }
}
