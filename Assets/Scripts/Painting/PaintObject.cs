using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PaintObject : DragDrop
{
    [SerializeField] private Image _image;

    public Action<PaintObject> OnDuplicateTrigger;
    public Action<PaintObject> OnObjectSelected;
    public Action<PointerEventData> OnObjectDragged;
    public Action<PaintObject> OnObjectDeleted;

    public bool IsLocked;

    private bool _isSetPlaced;
    private int _setGroup;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        OnObjectDragged?.Invoke(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {

    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        //_rectTransform.SetAsLastSibling();
        OnObjectSelected?.Invoke(this);
    }

    public Sprite GetImageSprite()
    {
        return _image.sprite;
    }

    public RectTransform GetRectTransform()
    {
        return _rectTransform;
    }

    public bool GetIsSetPlaced()
    {
        return _isSetPlaced;
    }

    public int GetSetGroup()
    {
        return _setGroup;
    }

    public void SetSetGroup(int setGroup)
    {
        _setGroup = setGroup;
    }

    public void SetIsSetPlaced(bool isSetPlaced)
    {
        _isSetPlaced = isSetPlaced;
    }

    public void SetScale(float scale)
    {
        _rectTransform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetSize(Vector2 size)
    {
        _rectTransform.sizeDelta = size;
    }

    public void Duplicate()
    {
        OnDuplicateTrigger?.Invoke(this);
    }

    public void Delete()
    {
        OnObjectDeleted?.Invoke(this);
        //Destroy(this.gameObject);
        PoolManager.Instance.ReturnPaintObject(gameObject);
    }

    public void SetImageSprite(Sprite sprite)
    {
        _image.sprite = sprite;
    }

    public void SetImageAlpha(float alpha)
    {
        Color tempColor = _image.color;
        tempColor.a = alpha;
        _image.color = tempColor;
    }
}
