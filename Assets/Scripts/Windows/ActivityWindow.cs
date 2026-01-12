using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActivityWindow : Window
{
    protected ConfirmationPopup _confirmationPopup;

    protected delegate void CloseAction();
    protected CloseAction _closeAction;

    public override void Show()
    {
        base.Show();
        //SetupConfirmationPopup();
    }

    protected virtual void SetupConfirmationPopup()
    {
        _confirmationPopup = PopupManager.Instance.GetPopup<ConfirmationPopup>();
        _confirmationPopup.SetHeader("Leaving");
        _confirmationPopup.SetDescription("Are you sure you want to leave?");
        _confirmationPopup.SetConfirmAction(() => { _canClose = true; _closeAction?.Invoke(); }, "Yes");
    }

    public override void HandleBackButton()
    {
        base.HandleBackButton();

        if (!CanClose)
        {
            _closeAction = HandleBackButton;
            _confirmationPopup.Show();
        }
    }

    public override void AttemptClose()
    {
        base.AttemptClose();

        if (!CanClose)
        {
            _closeAction = () => WindowManager.Instance.DisplayWindow(WindowManager.Instance.NextWindow.Type.ToString());
            _confirmationPopup.Show();
        }
    }
}
