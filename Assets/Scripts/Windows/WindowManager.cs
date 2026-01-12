using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static GameConstants;

public class WindowManager : MonoBehaviour
{
    public static WindowManager Instance;
    
    [SerializeField] private Window[] _windows;

    public Window PreviousWindow { get; private set; }
    public Window CurrentWindow { get; private set; }
    public Window NextWindow { get; private set; }
    private Stack<Window> WindowStack = new Stack<Window>();

    [System.NonSerialized] public UnityAction OnInitialized;
    [System.NonSerialized] public UnityAction<WindowType> OnWindowShown;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public T GetWindow<T>() where T : Window
    {
        for (int i = 0; i < _windows.Length; i++)
        {
            if (_windows[i] is T) return (T)_windows[i];
        }

        return null;
    }

    public void DisplayWindow<T>() where T : Window
    {
        for (int i = 0; i < _windows.Length; i++)
        {
            if (_windows[i] is T)
            {
                if (CurrentWindow == _windows[i])
                {
                    _windows[i].Reload();
                }

                else if (CurrentWindow == null || CurrentWindow.CanClose)
                {
                    _windows[i].Show();
                }

                else
                {
                    NextWindow = _windows[i];
                    CurrentWindow.AttemptClose();
                }
            }
        }
    }

    public void DisplayWindow(string type)
    {
        if (CurrentWindow != null && CurrentWindow.Type.ToString() == type)
        {
            CurrentWindow.Reload();
            return;
        }

        for (int i = 0; i < _windows.Length; i++)
        {
            if (_windows[i].Type.ToString() == type)
            {
                if (CurrentWindow.CanClose == false)
                {
                    NextWindow = _windows[i];
                    CurrentWindow.AttemptClose();
                }

                else
                {
                    _windows[i].Show();
                }
            }
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < _windows.Length; i++)
        {
            _windows[i].OnWindowShown += HandleWindowShown;
            _windows[i].OnWindowBack += HandleWindowBack;
            _windows[i].OnWindowClosing += HandleWindowRefresh;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < _windows.Length; i++)
        {
            _windows[i].OnWindowShown -= HandleWindowShown;
            _windows[i].OnWindowBack -= HandleWindowBack;
            _windows[i].OnWindowClosing -= HandleWindowRefresh;
        }
    }

    private void Start()
    {
        OnInitialized?.Invoke();
    }

    private void HandleWindowShown(Window window)
    {
        if (CurrentWindow != null)
        {
            CurrentWindow.Hide();
            WindowStack.Push(CurrentWindow);
        }

        PreviousWindow = CurrentWindow;
        CurrentWindow = window;
        NextWindow = null;
        OnWindowShown?.Invoke(CurrentWindow.Type);
    }

    private void HandleWindowBack(Window window)
    {
        CurrentWindow = null;

        if (WindowStack.Count > 0)
        {
            WindowStack.Pop().Show();
        }
    }

    private void HandleWindowRefresh(Window window)
    {
        if (window == CurrentWindow)
        {
            OnWindowShown?.Invoke(CurrentWindow.Type);
        }
    }

    private void ClearWindowStack()
    {
        WindowStack.Clear();
    }
}
