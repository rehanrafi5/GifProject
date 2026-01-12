using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Tools;
using static GameConstants;

public class PaintingWindow : ActivityWindow
{
    public Texture2D DefaultTexture;
    public Color DefaultColor;
    public GameObject PaintPrefab;
    public Toggle ObjectTool;
    public Toggle BackgroundTool;
    public Toggle ShapeTool;
    public Toggle SetTool;
    public RawImage[] BrushPreviews;
    public EventTrigger TopPanel;
    public EventTrigger ImageArea;
    public Transform InfoPanel;

    public Button PlaybackButton;

    public EventTrigger AllArea;

    public DrawingManager DrawingManager;

    private float _previousBrushSize;
    private bool _isDragging;
    private bool _isHovering;
    private bool _isZooming;
    private bool _touchEnabled;
    private Color _toggleColor;
    private Texture _defaultTexture;

    public event Action OnSetupComplete;

    protected override void Awake()
    {
        base.Awake();

        InitDrawingManagers();

        ObjectManager.Instance.OnObjectSelected += HandleObjectSelected;

        var hoverEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        hoverEnter.callback.AddListener(HoverEnter);
        var hoverExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        hoverExit.callback.AddListener(HoverExit);

        var beginDrag = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        beginDrag.callback.AddListener(DragEnter);

        var endDrag = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        endDrag.callback.AddListener(DragExit);

        ObjectTool.onValueChanged.AddListener(SetObjectTool);
        BackgroundTool.onValueChanged.AddListener(SetBackgroundTool);
        ShapeTool.onValueChanged.AddListener(SetShapeTool);
        SetTool.onValueChanged.AddListener(SetSetTool);

        TopPanel.triggers.Add(hoverEnter);
        TopPanel.triggers.Add(hoverExit);

        PlaybackButton.onClick.AddListener(OnPlayback);

        var onDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        onDown.callback.AddListener(ResetPlates);

        AllArea.triggers.Add(onDown);
        ImageArea.triggers.Add(onDown);
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(Setup());

#if UNITY_EDITOR
        _touchEnabled = true;
#endif
    }

    protected IEnumerator Setup()
    {
        while (PaintController.Instance.Brush.RenderTexture == null)
        {
            yield return null;
        }

        Vector3 halfVector = new Vector3(0.5f, 0.5f, 0.5f);

        foreach (RawImage preview in BrushPreviews)
        {
            preview.rectTransform.localScale = halfVector * PaintController.Instance.Brush.Size + halfVector;
        }

        _previousBrushSize = PaintController.Instance.Brush.Size;

        if (DrawingManager.GetObjectImage().material.HasProperty(LINEART_TEXTURE))
        {
            _defaultTexture = DrawingManager.GetObjectImage().material.GetTexture(LINEART_TEXTURE);
        }

        else
        {
            _defaultTexture = DrawingManager.GetObjectImage().material.GetTexture(MAIN_TEXTURE);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        InputManager.Instance.OnStartTouchPrimary += (c, t) => HandleStartTouch();
#else
        InputManager.Instance.OnStartPressPrimary += (c, t) => HandleStartTouch();
#endif
        InputManager.Instance.OnEndTouchPrimary += (c, t) => HandleEndTouch();
        //InputManager.Instance.OnStartTouchSecondary += (c, t) => HandleGestures();

        SetupActivity(_defaultTexture);
        StartCoroutine(SetupFrame());
    }

    private void Update()
    {
        if(DrawingManager.Initialized)
        {
            if(_previousBrushSize != PaintController.Instance.Brush.Size)
            {
                OnBrushSizeSlider(PaintController.Instance.Brush.Size);
            }
        }
    }

    private void ClearDrawing()
    {
        DrawingManager.Reset();
        DrawingManager.Render();

        Hide();
    }

    private IEnumerator SetupFrame()
    {
        while((FrameManager.Instance.gameObject.activeInHierarchy == false)
            || (PoolManager.Instance.gameObject.activeInHierarchy == false))
        {
            yield return null;
        }

        FrameManager.Instance.AddFrame();

        OnSetupComplete?.Invoke();
    }

    public override void Hide()
    {
        SetPaintManager();
        //BrushTool.isOn = true;
        InfoPanel.gameObject.SetActive(false);

        base.Hide();
    }

    private void InitDrawingManagers()
    {
        DrawingManager.gameObject.SetActive(true);
        DrawingManager.GetObjectImage().raycastTarget = true;
        DrawingManager.Init();
    }

    protected override void SetupConfirmationPopup()
    {
        base.SetupConfirmationPopup();

        _confirmationPopup.SetDescription("Are you sure you want to leave?\nYour art will not be saved.");
    }

    private void HandleStartTouch()
    {
        if(InputManager.Instance.IsSecondaryFingerPressed() == false)
        {
            _touchEnabled = true;
            ProcessInputs(true);
        }
    }

    private void HandleEndTouch()
    {
        ProcessInputs(false);
        _touchEnabled = false;
        _isZooming = false;
    }

    private void HandleGestures()
    {
        if(InputManager.Instance.GetPrimaryFingerDistance() < GESTURES_DISTANCE)
        {
            if (DrawingManager.PaintObject.IsPainting)
            {
                DrawingManager.PaintObject.FinishPainting();
                DrawingManager.PaintObject.TextureKeeper.Undo();
                DrawingManager.PaintObject.TextureKeeper.RemoveLastTexture();
            }

            ProcessInputs(false);
            _isZooming = true;
        }
    }

    private void ProcessInputs(bool isOn)
    {
        if ((_touchEnabled == true && _isHovering == false && _isDragging == false && _isZooming == false) || isOn == false)
        {
            DrawingManager.PaintObject.ProcessInput = isOn;
            PaintController.Instance.Preview = isOn && PaintController.Instance.ToolsManager.CurrentTool.ShowPreview;

            if (!isOn)
            {
                DrawingManager.PaintObject.FinishPainting();
            }
        }
    }

    public void SetupActivity(Texture texture)
    {
        Clear();
        DrawingManager.GetObjectImage().material.SetTexture(LINEART_TEXTURE, texture == null ? _defaultTexture : texture);
        PaintController.Instance.Tool = PaintTool.Object;
    }

    private void HandleObjectSelected()
    {
        BackgroundTool.isOn = false;
        ObjectTool.isOn = false;
        ShapeTool.isOn = false;
        SetTool.isOn = false;
    }

    public void SetPaintManager()
    {
        DrawingManager.SetLastColor(_toggleColor);
        DrawingManager.gameObject.SetActive(true);
        DrawingManager.GetObjectImage().raycastTarget = true;

        DrawingManager.PaintObject.ProcessInput = false;
    }

    private void SetObjectTool(bool isOn)
    {
        if(isOn)
        {
            OpenInfoPanel();
            PaintController.Instance.Tool = PaintTool.Object;
        }

        else if(ObjectTool.group.AnyTogglesOn() == false)
        {
            InfoPanel.gameObject.SetActive(false);
        }
    }

    private void SetBackgroundTool(bool isOn)
    {
        if (isOn)
        {
            OpenInfoPanel();
            PaintController.Instance.Tool = PaintTool.Background;
        }

        else if (BackgroundTool.group.AnyTogglesOn() == false)
        {
            InfoPanel.gameObject.SetActive(false);
        }
    }

    private void SetShapeTool(bool isOn)
    {
        if(isOn)
        {
            OpenInfoPanel();
            PaintController.Instance.Tool = PaintTool.Shape;
        }

        else if(ShapeTool.group.AnyTogglesOn() == false)
        {
            InfoPanel.gameObject.SetActive(false);
        }
    }

    private void SetSetTool(bool isOn)
    {
        if (isOn)
        {
            OpenInfoPanel();
            PaintController.Instance.Tool = PaintTool.Set;
        }

        else if (ShapeTool.group.AnyTogglesOn() == false)
        {
            InfoPanel.gameObject.SetActive(false);
        }
    }

    private void OpenInfoPanel()
    {
        SetPaintManager();
        InfoPanel.gameObject.SetActive(true);
    }

    private void OnBrushSizeSlider(float value)
    {
        PaintController.Instance.Brush.Size = value;
        _previousBrushSize = value;
        Vector3 halfVector = new Vector3(0.5f, 0.5f, 0.5f);

        foreach (RawImage preview in BrushPreviews)
        {
            preview.rectTransform.localScale = halfVector * PaintController.Instance.Brush.Size + halfVector;
        }
    }


    private void OnPlayback()
    {
        PlaybackWindow window = WindowManager.Instance.GetWindow<PlaybackWindow>();
        window.SetFrames(FrameManager.Instance.GetFrames());

        WindowManager.Instance.DisplayWindow<PlaybackWindow>();
    }

    private void DragEnter(BaseEventData data)
    {
        ProcessInputs(false);
        _isDragging = true;
    }

    private void DragExit(BaseEventData data)
    {
        _isDragging = false;
        ProcessInputs(true);
    }

    private void HoverEnter(BaseEventData data)
    {
        ProcessInputs(false);
        _isHovering = true;
    }

    private void HoverExit(BaseEventData data)
    {
        _isHovering = false;
        ProcessInputs(true);
    }

    private void Clear()
    {
        ClearDrawing();
    }

    private void ResetPlates(BaseEventData data)
    {
        InfoPanel.gameObject.SetActive(false);
        BackgroundTool.isOn = false;
        ObjectTool.isOn = false;
        ShapeTool.isOn = false;
        SetTool.isOn = false;
        ObjectManager.Instance.DeselectObject();
        HoverExit(null);
    }
}
