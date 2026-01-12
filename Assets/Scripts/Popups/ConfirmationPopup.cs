using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPopup : GenericPopup
{
    protected delegate void ConfirmationAction();
    protected delegate void CancelAction();

    [SerializeField] protected Button _confirmButton;
    [SerializeField] protected TextMeshProUGUI _confirmLabel;

    protected ConfirmationAction _confirmAction;

    protected override void AddEventListeners()
    {
        base.AddEventListeners();

        _confirmButton.onClick.AddListener(HandleConfirmation);
    }

    protected override void RemoveEventListeners()
    {
        base.RemoveEventListeners();

        _confirmButton.onClick.RemoveListener(HandleConfirmation);
    }

    public void SetConfirmAction(Action action, string label = "Confirm")
    {
        _confirmLabel.text = label;
        _confirmAction = () => action?.Invoke();
    }

    protected void HandleConfirmation()
    {
        _confirmAction?.Invoke();
        HandleClose();
    }
}
