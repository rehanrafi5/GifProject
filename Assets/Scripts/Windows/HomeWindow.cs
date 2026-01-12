using UnityEngine;
using UnityEngine.UI;

public class HomeWindow : Window
{
    [SerializeField] Button _brochureButton;
    [SerializeField] Button _paintingButton;

    protected override void Start()
    {
        base.Start();
        EnableMenu();
    }

    private void EnableMenu()
    {
        _brochureButton.onClick.AddListener(HandleBrochure);
        _paintingButton.onClick.AddListener(HandlePainting);
    }

    private void DisableMenu()
    {
        _brochureButton.onClick.RemoveListener(HandleBrochure);
        _paintingButton.onClick.RemoveListener(HandlePainting);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DisableMenu();
    }

    private void HandleBrochure()
    {
        //WindowManager.Instance.DisplayWindow<BrochureWindow>();
        
        // This link is Artivive's QR Code. Detects the current device and
        // goes to its play store.
        Application.OpenURL("http://onelink.to/zv6vjz");
    }

    private void HandlePainting()
    {
        WindowManager.Instance.DisplayWindow<PaintingWindow>();
    }
}
