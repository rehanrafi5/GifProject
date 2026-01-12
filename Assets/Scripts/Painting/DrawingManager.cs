using UnityEngine;
using UnityEngine.UI;
using XDPaint;
using XDPaint.Controllers;
using XDPaint.Core;

public class DrawingManager : PaintManager
{
    [SerializeField] private PaintTool DefaultTool;
    [SerializeField] private Texture DefaultBrush;
    [SerializeField] private Color DefaultColor = Color.white;
    [SerializeField] private float DefaultSize = 1.0f;
    [SerializeField] private float DefaultHardness = 0.99f;

    private PaintTool _lastTool;
    private Texture _lastBrush;
    private Color _lastColor = Color.white;
    private float _lastSize = 1.0f;
    private float _lastHardness = 0.99f;

    private void Awake()
    {
        _lastBrush = DefaultBrush;
        _lastTool = DefaultTool;
        _lastColor = DefaultColor;
        _lastSize = DefaultSize;
        _lastHardness = DefaultHardness;
    }

    private void OnEnable()
    {
        PaintController.Instance.Tool = _lastTool;
        PaintController.Instance.Brush.SetTexture(_lastBrush);
        PaintController.Instance.Brush.SetColor(_lastColor);
        PaintController.Instance.Brush.Size = _lastSize;
        PaintController.Instance.Brush.Hardness = _lastHardness;
        Material.SetPreviewTexture(PaintController.Instance.Brush.RenderTexture);
    }

    private void OnDisable()
    {
        _lastTool = PaintController.Instance.Tool;
        _lastBrush = PaintController.Instance.Brush.SourceTexture;
        _lastColor = PaintController.Instance.Brush.Color;
        _lastSize = PaintController.Instance.Brush.Size;
        _lastHardness = PaintController.Instance.Brush.Hardness;
    }

    public void SetLastColor(Color color)
    {
        _lastColor = color;
    }

    public void SetDefaultBrush(Texture texture)
    {
        _lastBrush = texture;
    }

    public void SetDefaultSize(float size)
    {
        DefaultSize = size;
    }

    public float GetDefaultSize()
    {
        return DefaultSize;
    }

    public RawImage GetObjectImage()
    {
        return ObjectForPainting.GetComponent<RawImage>();
    }

    public void Reset()
    {
        ClearTexture(false);

        _lastTool = DefaultTool;
        _lastBrush = DefaultBrush;
        _lastColor = DefaultColor;
        _lastSize = DefaultSize;
        _lastHardness = DefaultHardness;

        OnEnable();
    }

    public void ClearTexture(bool writeToUndo)
    {
        if(!writeToUndo)
        {
            PaintObject.TextureKeeper.Reset();
            PaintObject.ClearTexture();
            Render();
        }
        
        else
        {
            PaintObject.ClearTexture(writeToUndo);
        }
    }
}
