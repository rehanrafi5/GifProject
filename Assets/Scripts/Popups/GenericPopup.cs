using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameConstants;

public class GenericPopup : MonoBehaviour
{
    [SerializeField] protected RectTransform _root;
    [SerializeField] protected PopupType _type;
    [SerializeField] protected TextMeshProUGUI _headerLabel;
    [SerializeField] protected TextMeshProUGUI _descriptionLabel;
    [SerializeField] protected Animator _transitionAnimator;

    public bool IsOpen { get { return _root.gameObject.activeSelf; } }
    public PopupType Type { get { return _type; } private set { _type = value; } }
    public Action OnClose;

    private void Start()
    {
        AddEventListeners();
    }

    private void OnDestroy()
    {
        RemoveEventListeners();
    }

    protected virtual void AddEventListeners()
    {
        
    }

    protected virtual void RemoveEventListeners()
    {
        
    }

    public virtual void Show()
    {
        _root.gameObject.SetActive(true);
        _transitionAnimator.SetBool(ANIMATOR_SHOWN, true);
    }

    public virtual void Hide()
    {
        _root.gameObject.SetActive(false);
        _transitionAnimator.SetBool(ANIMATOR_SHOWN, false);
    }

    public void SetHeader(string headerText)
    {
        if(_headerLabel != null)
        {
            _headerLabel.text = headerText;
        }
    }

    public void SetDescription(string descriptionText)
    {
        if(_descriptionLabel != null)
        {
            _descriptionLabel.text = descriptionText;
        }
    }

    protected virtual void HandleClose()
    {
        Hide();
        OnClose?.Invoke();
    }
}