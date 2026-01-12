using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowLoader : MonoBehaviour
{
    [SerializeField] private PaintingWindow paintWindow;

    private void Awake()
    {
        paintWindow.OnSetupComplete += OpenPaintWindow;
    }

    private void OnDestroy()
    {
        paintWindow.OnSetupComplete -= OpenPaintWindow;
    }

    private void OpenPaintWindow()
    {
        WindowManager.Instance.DisplayWindow<PaintingWindow>();
    }
}
