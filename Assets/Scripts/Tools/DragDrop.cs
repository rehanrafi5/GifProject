using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    protected Canvas _canvas;
    protected RectTransform _rectTransform;
    protected Camera _mainCamera;

    protected virtual void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
        _mainCamera = Camera.main;
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {

    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        Vector3 vec = _mainCamera.WorldToScreenPoint(_rectTransform.position);
        vec.x += eventData.delta.x;
        vec.y += eventData.delta.y;
        _rectTransform.position = _mainCamera.ScreenToWorldPoint(vec);
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {

    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {

    }
}
