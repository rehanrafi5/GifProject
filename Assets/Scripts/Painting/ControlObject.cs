using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameConstants;

public class ControlObject : DragDrop
{
    [SerializeField] private HandleObject _nwCorner;
    [SerializeField] private HandleObject _neCorner;
    [SerializeField] private HandleObject _swCorner;
    [SerializeField] private HandleObject _seCorner;
    [SerializeField] private HandleObject _rotateHandle;

    #region Set
    private bool IsSet = false;
    private float DefaultSize = 0f;
    public Action<float> OnSetPlaced;
    #endregion // Set

    private bool dragStarted = false;
    private Quaternion oldRotation;
    private Vector2 oldPosition;
    private Vector2 oldSize;
    private Vector2 oldScale;
    private float aspectRatio;
    private RectTransform _imageTransform;

    public Action OnObjectModified;

    private void Start()
    {
        _nwCorner.OnHandleStartDrag += SaveOldSizeAndPosition;
        _neCorner.OnHandleStartDrag += SaveOldSizeAndPosition;
        _swCorner.OnHandleStartDrag += SaveOldSizeAndPosition;
        _seCorner.OnHandleStartDrag += SaveAspectRatio;
        _rotateHandle.OnHandleStartDrag += SaveOldRotation;

        _nwCorner.OnHandleDragged += ((corner) => HandleCornerDragged(corner, Directions.NW));
        _neCorner.OnHandleDragged += ((corner) => HandleCornerDragged(corner, Directions.NE));
        _swCorner.OnHandleDragged += ((corner) => HandleCornerDragged(corner, Directions.SW));
        _seCorner.OnHandleDragged += ((corner) => HandleRescaleCornerDragged(corner, Directions.SE));
        _rotateHandle.OnHandleDragged += HandleRotateDragged;

        _nwCorner.OnHandleReleased += ((corner) => OnObjectModified?.Invoke());
        _neCorner.OnHandleReleased += ((corner) => OnObjectModified?.Invoke());
        _swCorner.OnHandleReleased += ((corner) => OnObjectModified?.Invoke());
        _seCorner.OnHandleReleased += ((corner) => OnObjectModified?.Invoke());
        _rotateHandle.OnHandleReleased += ((corner) => OnObjectModified?.Invoke());
    }
    
    private void OnDestroy()
    {
        _nwCorner.OnHandleStartDrag -= SaveOldSizeAndPosition;
        _neCorner.OnHandleStartDrag -= SaveOldSizeAndPosition;
        _swCorner.OnHandleStartDrag -= SaveOldSizeAndPosition;
        _seCorner.OnHandleStartDrag -= SaveAspectRatio;
        _rotateHandle.OnHandleStartDrag -= SaveOldRotation;

        _nwCorner.OnHandleDragged -= ((corner) => HandleCornerDragged(corner, Directions.NW));
        _neCorner.OnHandleDragged -= ((corner) => HandleCornerDragged(corner, Directions.NE));
        _swCorner.OnHandleDragged -= ((corner) => HandleCornerDragged(corner, Directions.SW));
        _seCorner.OnHandleDragged -= ((corner) => HandleRescaleCornerDragged(corner, Directions.SE));
        _rotateHandle.OnHandleDragged -= HandleRotateDragged;

        _nwCorner.OnHandleReleased -= ((corner) => OnObjectModified?.Invoke());
        _neCorner.OnHandleReleased -= ((corner) => OnObjectModified?.Invoke());
        _swCorner.OnHandleReleased -= ((corner) => OnObjectModified?.Invoke());
        _seCorner.OnHandleReleased -= ((corner) => OnObjectModified?.Invoke());
        _rotateHandle.OnHandleReleased -= ((corner) => OnObjectModified?.Invoke());
    }

    private void SaveOldRotation(HandleObject handle)
    {
        oldRotation = _imageTransform.rotation;
    }

    private void SaveOldSizeAndPosition(HandleObject handle)
    {
        oldSize = _imageTransform.sizeDelta;
        oldPosition = _imageTransform.anchoredPosition;
        oldScale = _imageTransform.localScale;
    }

    private void SaveAspectRatio(HandleObject handle)
    {
        SaveOldSizeAndPosition(handle);
        aspectRatio = oldSize.x / oldSize.y;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
    }

    public void SetImageTransform(RectTransform rectTransform)
    {
        _imageTransform = rectTransform;
        Reset();
    }

    public void IsDraggingSet(bool isSet, float defaultSize)
    {
        IsSet = isSet;
        DefaultSize = defaultSize;
    }

    public void Reset()
    {
        if(_imageTransform != null)
        {
            _rectTransform.rotation = _imageTransform.rotation;
            _rectTransform.position = _imageTransform.position;
            _rectTransform.sizeDelta = _imageTransform.sizeDelta;

            _nwCorner.Reset();
            _neCorner.Reset();
            _swCorner.Reset();
            _seCorner.Reset();

            if(_imageTransform.localScale.x < 0f)
            {
                SwapCornerPositions(_neCorner, _nwCorner);
                SwapCornerPositions(_seCorner, _swCorner);
            }

            if(_imageTransform.localScale.y < 0f)
            {
                SwapCornerPositions(_neCorner, _seCorner);
                SwapCornerPositions(_nwCorner, _swCorner);
            }
        }  
    }

    private void SwapCornerPositions(HandleObject originCorner, HandleObject targetCorner)
    {
        Vector3 previousPosition = originCorner.GetPosition();
        originCorner.SetPosition(targetCorner.GetPosition());
        targetCorner.SetPosition(previousPosition);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        ResizeImage();
        ResizeBox();
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        ObjectManager.Instance.SelectedObject.SetIsSetPlaced(false); // SET SELECTED OBJECT Is Set Placed to FALSE
        OnObjectModified?.Invoke();

        if (IsSet)
        {
            OnSetPlaced?.Invoke(DefaultSize);
        }
    }

    private void HandleCornerDragged(HandleObject handle, Directions direction)
    {
        ObjectManager.Instance.SelectedObject.SetIsSetPlaced(false); // SET SELECTED OBJECT Is Set Placed to FALSE

        Vector2 cornerPos = handle.GetPosition();
        DetermineOtherHandles(direction,
            out HandleObject xCorner, out HandleObject yCorner,
            out HandleObject oppositeCorner);

        Vector2 v1 = xCorner.GetPosition() - oppositeCorner.GetPosition();
        Vector2 v2 = yCorner.GetPosition() - oppositeCorner.GetPosition();

        Vector3 directionVector = cornerPos - oppositeCorner.GetPosition();
        Vector3 northNormal = Vector3.Project(directionVector, v1) + (Vector3) oppositeCorner.GetPosition();
        Vector3 eastNormal = Vector3.Project(directionVector, v2) + (Vector3) oppositeCorner.GetPosition();

        xCorner.SetPosition(northNormal);
        yCorner.SetPosition(eastNormal);

        ResizeImage();
        ResizeBox();
    }

    private void HandleRescaleCornerDragged(HandleObject handle, Directions direction)
    {
        ObjectManager.Instance.SelectedObject.SetIsSetPlaced(false); // SET SELECTED OBJECT Is Set Placed to FALSE

        Vector2 draggedPos = handle.GetPosition();
        DetermineOtherHandles(direction,
            out HandleObject xCorner, out HandleObject yCorner,
            out HandleObject oppositeCorner);

        // Get the center in world space
        Vector2 center = (xCorner.GetPosition() + yCorner.GetPosition()) * 0.5f;

        // Get the inverse transform to bring world to local
        Matrix4x4 worldToLocal = _imageTransform.worldToLocalMatrix;
        Matrix4x4 localToWorld = _imageTransform.localToWorldMatrix;

        // Transform the drag point into local space relative to the center
        Vector2 localCenter = worldToLocal.MultiplyPoint3x4(center);
        Vector2 localDrag = worldToLocal.MultiplyPoint3x4(draggedPos);
        Vector2 centerToDragLocal = localDrag - localCenter;

        // Maintain aspect ratio
        float width = Mathf.Abs(centerToDragLocal.x) * 2f;
        float height = Mathf.Abs(centerToDragLocal.y) * 2f;

        if (width / aspectRatio > height)
        {
            width = Mathf.Abs(width);
            height = width / aspectRatio;
        }
        else
        {
            height = Mathf.Abs(height);
            width = height * aspectRatio;
        }

        float xSign = Mathf.Sign(centerToDragLocal.x);
        float ySign = Mathf.Sign(centerToDragLocal.y);

        // Final offset in local space
        Vector2 offsetLocal = new Vector2(width * 0.5f * xSign, height * 0.5f * ySign);

        // Transform offsets back into world space
        Vector2 offsetWorld = localToWorld.MultiplyVector(offsetLocal);
        Vector2 xOffsetWorld = localToWorld.MultiplyVector(new Vector2(-offsetLocal.x, offsetLocal.y));
        Vector2 yOffsetWorld = localToWorld.MultiplyVector(new Vector2(offsetLocal.x, -offsetLocal.y));

        // Set the handle and corners based on world offsets
        handle.SetPosition(center + offsetWorld);
        xCorner.SetPosition(center + xOffsetWorld);
        yCorner.SetPosition(center + yOffsetWorld);
        oppositeCorner.SetPosition(center - offsetWorld);

        ResizeImage();
        ResizeBox();
    }

    private void DetermineOtherHandles(Directions direction,
        out HandleObject xCorner, out HandleObject yCorner,
        out HandleObject oppositeCorner)
    {
        xCorner = null;
        yCorner = null;
        oppositeCorner = null;

        switch (direction)
        {
            case Directions.NW:
                xCorner = _neCorner;
                yCorner = _swCorner;
                oppositeCorner = _seCorner;
                break;
            case Directions.NE:
                xCorner = _nwCorner;
                yCorner = _seCorner;
                oppositeCorner = _swCorner;
                break;
            case Directions.SW:
                xCorner = _seCorner;
                yCorner = _nwCorner;
                oppositeCorner = _neCorner;
                break;
            case Directions.SE:
                xCorner = _swCorner;
                yCorner = _neCorner;
                oppositeCorner = _nwCorner;
                break;
        }
    }

    private void HandleRotateDragged(HandleObject handle)
    {
        ObjectManager.Instance.SelectedObject.SetIsSetPlaced(false); // SET SELECTED OBJECT Is Set Placed to FALSE

        Vector2 handlePos = _rotateHandle.GetScreenPoint();
        Vector2 centerPos = Vector2.Lerp(_neCorner.GetScreenPoint(), _swCorner.GetScreenPoint(), 0.5f);

        Vector2 v1 = handlePos - centerPos;
        Vector2 v2 = Vector2.up;

        float angle = Mathf.Atan2(v2.y - v1.y, v2.x - v1.x) * 180 / Mathf.PI + 90f;
        _imageTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        _rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        _rotateHandle.SetAnchoredPosition(Vector2.zero);
    }

    private void ResizeImage()
    {
        float width = Vector2.Distance(_neCorner.GetScreenPoint(), _nwCorner.GetScreenPoint());
        float height = Vector2.Distance(_neCorner.GetScreenPoint(), _seCorner.GetScreenPoint());
        float xSize = Mathf.Abs(_imageTransform.localScale.x);
        float ySize = Mathf.Abs(_imageTransform.localScale.y);

        Quaternion angleAxis = Quaternion.Euler(0, 0, -_rectTransform.rotation.eulerAngles.z);
        Vector3 horizontalVector = _neCorner.GetPosition() - _nwCorner.GetPosition();
        Vector3 verticalVector = _neCorner.GetPosition() - _seCorner.GetPosition();
        Vector3 xVector = angleAxis * horizontalVector;
        Vector3 yVector = angleAxis * verticalVector;

        float xScale = xVector.x > 0 ? xSize : -xSize;
        float yScale = yVector.y > 0 ? ySize : -ySize;

        width /= _canvas.scaleFactor;
        height /= _canvas.scaleFactor;

        Vector2 center = Vector2.Lerp(_neCorner.GetPosition(), _swCorner.GetPosition(), 0.5f);

        _imageTransform.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        _imageTransform.localScale = new Vector3(xScale, yScale, 1f);
        _imageTransform.position = new Vector3(center.x, center.y, _imageTransform.position.z);
    }

    private void ResizeBox()
    {
        _nwCorner.transform.SetParent(_imageTransform);
        _neCorner.transform.SetParent(_imageTransform);
        _swCorner.transform.SetParent(_imageTransform);
        _seCorner.transform.SetParent(_imageTransform);

        _rectTransform.rotation = _imageTransform.rotation;
        _rectTransform.position = _imageTransform.position;
        _rectTransform.sizeDelta = _imageTransform.sizeDelta;

        _nwCorner.transform.SetParent(_rectTransform);
        _neCorner.transform.SetParent(_rectTransform);
        _swCorner.transform.SetParent(_rectTransform);
        _seCorner.transform.SetParent(_rectTransform);
    }
}
