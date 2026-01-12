using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GameConstants;

public class Window : MonoBehaviour
{
    [System.NonSerialized] public UnityAction<Window> OnWindowShown;
    [System.NonSerialized] public UnityAction<Window> OnWindowBack;
    [System.NonSerialized] public UnityAction<Window> OnWindowClosing;

    [SerializeField] protected WindowType _type;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected Button _backButton;
    [SerializeField] protected bool _confirmClose;

    protected bool _canClose;
    public bool CanClose { get { return _canClose == true || _confirmClose == false; } }
    public bool IsShown { get { return _animator.GetBool(ANIMATOR_SHOWN); } }
    public WindowType Type { get { return _type; } private set { _type = value; } }

    protected ScrollRect _scrollRect;

    protected virtual void Awake()
    {
        _scrollRect = GetComponentInChildren<ScrollRect>();
    }

    protected virtual void Start()
    {
        if (_backButton != null)
        {
            _backButton.onClick.AddListener(HandleBackButton);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_backButton != null)
        {
            _backButton.onClick.RemoveListener(HandleBackButton);
        }
    }

    public virtual void Show()
    {
        if (_scrollRect != null)
        {
            _scrollRect.verticalNormalizedPosition = 1f;
        }

        if (_confirmClose)
        {
            _canClose = false;
        }

        _animator.SetBool(ANIMATOR_SHOWN, true);
        OnWindowShown?.Invoke(this);
    }

    public virtual void Hide()
    {
        _animator.SetBool(ANIMATOR_SHOWN, false);
    }

    public virtual void Reload()
    {
        //Override to do something
    }

    public virtual void AttemptClose()
    {
        OnWindowClosing?.Invoke(this);
    }

    public virtual void HandleBackButton()
    {
        if (CanClose)
        {
            Hide();
            OnWindowBack?.Invoke(this);
        }
    }

    public void ConfirmClose()
    {
        _canClose = true;
        HandleBackButton();
    }
}
