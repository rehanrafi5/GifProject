using System.Collections;
using UnityEngine;

public class PinchDetection : MonoBehaviour
{
    [SerializeField] private RectTransform _targetTransform;
    [SerializeField] private float _zoomSpeed = 0.1f;
    [SerializeField] private float _maxZoom = 10f;

    private Vector3 _initialPosition;
    private Vector3 _initialScale;
    private Vector2 _startPinchCenterPosition;
    private bool _gesturesEnabled;

    private void Awake()
    {
        _initialPosition = _targetTransform.localPosition;
        _initialScale = _targetTransform.localScale;
    }

    private void OnEnable()
    {
        _targetTransform.localPosition = _initialPosition;
        _targetTransform.localScale = _initialScale;
    }

    private void Start()
    {
        InputManager.Instance.OnPinch += HandlePinch;
        InputManager.Instance.OnScroll += HandleScroll;
        InputManager.Instance.OnDrag += HandleDrag;
        InputManager.Instance.OnStartTouchSecondary += HandleSecondTouch;
    }

    private void OnDestroy()
    {
        InputManager.Instance.OnPinch -= HandlePinch;
        InputManager.Instance.OnScroll -= HandleScroll;
        InputManager.Instance.OnDrag -= HandleDrag;
        InputManager.Instance.OnStartTouchSecondary -= HandleSecondTouch;
    }

    private void HandleSecondTouch(Vector2 position, float time)
    {
        _gesturesEnabled = InputManager.Instance.GetPrimaryFingerDistance() < GameConstants.GESTURES_DISTANCE;
    }

    private void HandleScroll(float value)
    {
        _startPinchCenterPosition = InputManager.Instance.GetMousePosition();
        Zoom(value);
    }

    private void HandlePinch(Vector2 position, float value)
    {
        if(_gesturesEnabled)
        {
            _startPinchCenterPosition = position;
            Zoom(value * _zoomSpeed);
        }
    }

    private void HandleDrag(Vector2 direction)
    {
        if(_gesturesEnabled)
        {
            Drag(direction);
        }
    }

    private void Drag(Vector2 direction)
    {
        _targetTransform.localPosition += (Vector3) direction;
    }

    private void Zoom(float increment)
    {
        //Set new scale
        var delta = Vector3.one * (increment * _zoomSpeed);
        var desiredScale = _targetTransform.localScale + delta;
        desiredScale = ClampDesiredScale(desiredScale);

        // Set new pivot
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_targetTransform, _startPinchCenterPosition, Camera.main, out var localpoint);

        Vector2 normalizedPoint = Rect.PointToNormalized(_targetTransform.rect, localpoint);

        Vector3 scaleChange = desiredScale - _targetTransform.localScale;
        Vector3 offset = new Vector2(-(localpoint.x * scaleChange.x), -(localpoint.y * scaleChange.y));

        _targetTransform.localPosition += offset;
        _targetTransform.localScale = desiredScale;
    }

    private Vector3 ClampDesiredScale(Vector3 desiredScale)
    {
        desiredScale = Vector3.Max(_initialScale, desiredScale);
        desiredScale = Vector3.Min(_initialScale * _maxZoom, desiredScale);
        return desiredScale;
    }
}