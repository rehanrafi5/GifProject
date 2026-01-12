using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    [SerializeField] private List<GenericPopup> _popups = new List<GenericPopup>();

    public T GetPopup<T>() where T : GenericPopup
    {
        for (int i = 0; i < _popups.Count; i++)
        {
            if (_popups[i] is T)
            {
                return _popups[i] as T;
            }
        }

        return null;
    }

    public T ShowPopup<T>() where T : GenericPopup
    {
        for (int i = 0; i < _popups.Count; i++)
        {
            if (_popups[i] is T)
            {
                _popups[i].Show();

                return _popups[i] as T;
            }
        }

        return null;
    }

    public void HidePopup<T>() where T : GenericPopup
    {
        for (int i = 0; i < _popups.Count; i++)
        {
            if (_popups[i] is T)
            {
                _popups[i].Hide();
            }
        }
    }

    public void HideAllPopups()
    {
        for (int i = 0; i < _popups.Count; i++)
        {
            _popups[i].Hide();
        }
    }
}
