using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{ 
    private TouchControls _controls;

    #region Events
    public delegate void StartTouch(Vector2 position, float time);
    public event StartTouch OnStartTouchPrimary;
    public event StartTouch OnStartTouchSecondary;

    public delegate void EndTouch(Vector2 position, float time);
    public event StartTouch OnEndTouchPrimary;
    public event StartTouch OnEndTouchSecondary;

    public delegate void PinchGesture(Vector2 position, float value);
    public event PinchGesture OnPinch;

    public delegate void DragGesture(Vector2 delta);
    public event DragGesture OnDrag;

    public delegate void StartPress(Vector2 position, float time);
    public event StartPress OnStartPressPrimary;

    public delegate void Scroll(float scrollValue);
    public event Scroll OnScroll;
    #endregion

    #region Values
    private Coroutine _gestureDetection;
    private Vector2 _primaryStartPosition;
    private float _primaryTouchDistance;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void OnDestroy()
    {
        Clear();
    }

    private void OnEnable()
    {
        _controls.Enable();

        _controls.Touch.PrimaryTouchContact.started += StartTouchPrimary;
        _controls.Touch.PrimaryTouchContact.canceled += EndTouchPrimary;
        _controls.Touch.PrimaryTouchPress.performed += StartPressPrimary;

        _controls.Touch.SecondaryTouchContact.started += StartTouchSecondary;
        _controls.Touch.SecondaryTouchContact.canceled += EndTouchSecondary;

        _controls.Mouse.ScrollWheel.performed += HandleScrollWheel;
    }

    private void OnDisable()
    {
        if (_controls == null)
        {
            return;
        }

        _controls.Touch.PrimaryTouchContact.started -= StartTouchPrimary;
        _controls.Touch.PrimaryTouchContact.canceled -= EndTouchPrimary;

        _controls.Touch.SecondaryTouchContact.started -= StartTouchSecondary;
        _controls.Touch.SecondaryTouchContact.canceled -= EndTouchSecondary;

        _controls.Mouse.ScrollWheel.performed -= HandleScrollWheel;

        _controls.Disable();
    }

    private void Update()
    {
        float distance = Vector2.Distance(_primaryStartPosition, GetPrimaryFingerPosition());

        if (distance > _primaryTouchDistance)
        {
            _primaryTouchDistance = distance;
        }
    }

    public void Initialize()
    {
        _controls = new TouchControls();
    }

    public void Clear()
    {
        _controls.Dispose();
    }

    private void StartTouchPrimary(InputAction.CallbackContext context)
    {
        StartPrimaryFinger();
        OnStartTouchPrimary?.Invoke(GetPrimaryFingerPosition(), (float)context.startTime);
    }

    private void EndTouchPrimary(InputAction.CallbackContext context)
    {
        OnEndTouchPrimary?.Invoke(GetPrimaryFingerPosition(), (float)context.startTime);
    }

    private void StartPressPrimary(InputAction.CallbackContext context)
    {
        StartPrimaryFinger();
        OnStartPressPrimary?.Invoke(GetPrimaryFingerPosition(), (float)context.startTime);
    }

    private void StartTouchSecondary(InputAction.CallbackContext context)
    {
        OnStartTouchSecondary?.Invoke(GetSecondaryFingerPosition(), (float)context.startTime);
        _gestureDetection = StartCoroutine(CheckGestures());
    }

    private void EndTouchSecondary(InputAction.CallbackContext context)
    {
        StopCoroutine(_gestureDetection);
        OnEndTouchSecondary?.Invoke(GetSecondaryFingerPosition(), (float)context.startTime);
    }

    private void HandleScrollWheel(InputAction.CallbackContext context)
    {
        OnScroll?.Invoke(_controls.Mouse.ScrollWheel.ReadValue<Vector2>().y);
    }

    public Vector2 GetMousePosition()
    {
        return _controls.Mouse.Position.ReadValue<Vector2>();
    }

    public float GetPrimaryFingerDistance()
    {
        return _primaryTouchDistance;
    }

    public bool IsPrimaryFingerPressed()
    {
        return _controls.Touch.PrimaryTouchContact.phase == InputActionPhase.Performed;
    }

    public Vector2 GetPrimaryFingerPosition()
    {
        return _controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>();
    }

    public Vector2 GetPrimaryDeltaPosition()
    {
        return _controls.Touch.PrimaryDeltaPosition.ReadValue<Vector2>();
    }

    public bool IsSecondaryFingerPressed()
    {
        return _controls.Touch.SecondaryTouchContact.phase == InputActionPhase.Performed;
    }

    public Vector2 GetSecondaryFingerPosition()
    {
        return _controls.Touch.SecondaryFingerPosition.ReadValue<Vector2>();
    }

    public Vector2 GetSecondaryDeltaPosition()
    {
        return _controls.Touch.SecondaryDeltaPosition.ReadValue<Vector2>();
    }

    private void StartPrimaryFinger()
    {
        _primaryStartPosition = GetPrimaryFingerPosition();
        _primaryTouchDistance = 0f;
    }

    private IEnumerator CheckGestures()
    {
        while (true)
        {
            Vector2 centerPosition = (GetPrimaryFingerPosition() + GetSecondaryFingerPosition()) / 2;

            if (Vector2.Dot(GetPrimaryDeltaPosition(), GetSecondaryDeltaPosition()) < 0f)
            {
                Vector2 primaryPosition = GetPrimaryFingerPosition();
                Vector2 secondaryPosition = GetSecondaryFingerPosition();
                Vector2 primaryDeltaPosition = GetPrimaryDeltaPosition();
                Vector2 secondaryDeltaPosition = GetSecondaryDeltaPosition();

                Vector2 touchZeroPrevPos = primaryPosition - primaryDeltaPosition;
                Vector2 touchOnePrevPos = secondaryPosition - secondaryDeltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (primaryPosition - secondaryPosition).magnitude;

                float distance = currentMagnitude - prevMagnitude;

                OnPinch(centerPosition, distance);
            }

            else if (Vector2.Dot(GetPrimaryDeltaPosition(), GetSecondaryDeltaPosition()) > 0f)
            {
                Vector2 delta = (GetPrimaryDeltaPosition() + GetSecondaryDeltaPosition()) / 2;
                OnDrag(delta);
            }

            yield return null;
        }
    }
}