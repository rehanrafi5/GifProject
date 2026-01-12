using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjectItem : MonoBehaviour,
    IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Image Image;
    [SerializeField] private float DefaultSize;

    [Header("Set")]
    [SerializeField] private bool _isSet = false;
    [SerializeField] private GameObject _setPattern;

    private DisplayOnPaintingTool paintingTool;
    private RectTransform scrollAllowedArea;
    private RectTransform workArea;
    private ScrollRect scrollRect;
    private Canvas canvas;

    private bool isPrefabSpawned;
    private bool wasDragged;

    public bool IsSet => _isSet;

    public Action<PointerEventData, Sprite, float> OnObjectSpawned;
    public Action<PointerEventData, Sprite, GameObject, float> OnSetSpawned;

    private void Awake()
    {
        paintingTool = GetComponentInParent<DisplayOnPaintingTool>();
        scrollAllowedArea = paintingTool.ScrollAllowedArea;
        workArea = paintingTool.WorkArea;
        canvas = GetComponentInParent<Canvas>();
        scrollRect = GetComponentInParent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isPrefabSpawned = false;
        wasDragged = true;

        if (scrollRect != null)
            scrollRect.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isPrefabSpawned)
        {
            if (scrollRect != null)
                scrollRect.OnDrag(eventData);

            if (IsCursorInWorkArea(eventData.position))
            {
                if (!HasAvailableObjects())
                    return;

                Spawn(eventData);
                isPrefabSpawned = true;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isPrefabSpawned && scrollRect != null)
            scrollRect.OnEndDrag(eventData);
    }

    // 👉 TAP / CLICK SUPPORT
    public void OnPointerClick(PointerEventData eventData)
    {
        if (wasDragged)   // prevent double-trigger from drag
        {
            wasDragged = false;
            return;
        }

        if (!HasAvailableObjects())
            return;

        // Calculate center of work area in screen space
        Vector3 worldCenter = workArea.position; // world position of RectTransform center
        Vector2 centerScreenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldCenter);

        float offsetY = 150f;
        centerScreenPos.y += offsetY;

        // Spawn object at adjusted center using FrameManager
        FrameManager.Instance.CurrentFrame.AddObject(
            gameObject,           // prefab (or the object you want to spawn)
            Image.sprite,
            DefaultSize,
            centerScreenPos
        );
    }



    
    private void Spawn(PointerEventData eventData)
    {
        if (_isSet)
        {
            OnSetSpawned?.Invoke(eventData, Image.sprite, _setPattern, DefaultSize);
        }
        else
        {
            OnObjectSpawned?.Invoke(eventData, Image.sprite, DefaultSize);
        }
    }

    private bool HasAvailableObjects()
    {
        return (!_isSet && PoolManager.Instance.AvailablePaintObjects > 0)
            || (_isSet && PoolManager.Instance.AvailablePaintObjects >= _setPattern.transform.childCount);
    }

    private bool IsCursorInWorkArea(Vector2 screenPoint)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            scrollAllowedArea,
            screenPoint,
            canvas.worldCamera,
            out Vector2 parentPoint);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            workArea,
            screenPoint,
            canvas.worldCamera,
            out Vector2 workPoint);

        return !scrollAllowedArea.rect.Contains(parentPoint)
            && workArea.rect.Contains(workPoint);
    }
}
