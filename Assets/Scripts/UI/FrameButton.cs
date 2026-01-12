using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class FrameButton : DragDropScrollItem
{
    [SerializeField] private Toggle _toggle;
    [SerializeField] private RawImage _image;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _frameLabel;
    [SerializeField] private TextMeshProUGUI _frameLabel2;
    [SerializeField] private TextMeshProUGUI _frameLabel3;
    [SerializeField] private TextMeshProUGUI _frameMaxLabel;

    private Texture2D _texture2D;
    private bool _isTextureSet = false;
    //[SerializeField] private GameObject _placeholderPrefab;

    public Action<int> OnFrameClicked;
    public Action<int, int> OnFrameSwapped;

    private RectTransform rect;
    private bool _isVisibleOnScroll;

    private int _frameNumber;

    public bool IsVisibleOnScroll => _isVisibleOnScroll;
    public bool IsTextureSet => _isTextureSet;
    public bool IsScreenshotSaved => (_image.texture != null);
    public Texture2D CaptureTexture => _texture2D;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        _toggle.onValueChanged.AddListener(HandleFrameClicked);

        UpdateFrameNumber();
        _frameMaxLabel.text = "/" + PoolManager.Instance.MaxFrames.ToString();
    }

    private void OnEnable()
    {
        if (rect == null)
        {
            rect = GetComponent<RectTransform>();
        }
        FrameManager.Instance.OnFrameScroll += OnFrameScroll;
        HideLabel();
    }

    private void OnDisable()
    {
        FrameManager.Instance.OnFrameScroll -= OnFrameScroll;
    }

    private void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(HandleFrameClicked);
    }

    public void SetToggleGroup(ToggleGroup group)
    {
        _toggle.group = group;
        _toggle.isOn = true;
    }

    public void SetupTexture(int width, int height, TextureFormat format, bool mipChain)
    {
        _texture2D = new Texture2D(width, height, format, mipChain);
        _isTextureSet = true;
    }

    public void SetScreenshot(Texture2D texture)
    {
        _image.texture = texture;
    }

    public void RemoveTexture()
    {
        _image.texture = null;
        _texture2D = null;
        _isTextureSet = false;
    }

    public RectTransform GetRectTransform()
    {
        return _rectTransform;
    }

    public void ShowLabel()
    {
        _frameLabel.gameObject.SetActive(true);
        _image.gameObject.SetActive(false);

        _frameLabel3.gameObject.SetActive(false);
        _frameMaxLabel.gameObject.SetActive(false);
    }

    public void HideLabel()
    {
        _image.gameObject.SetActive(true);
        _frameLabel.gameObject.SetActive(false);

        _frameLabel3.gameObject.SetActive(true);
        _frameMaxLabel.gameObject.SetActive(true);
    }

    public void UpdateFrameNumber()
    {
        _frameNumber = (transform.GetSiblingIndex() + 1);
        _frameLabel.text = _frameNumber.ToString();
        _frameLabel2.text = _frameNumber.ToString();
        _frameLabel3.text = _frameNumber.ToString();
    }

    protected override void OnBeginDragShort(PointerEventData eventData)
    {
        base.OnBeginDragShort(eventData);
        FrameManager.Instance.OnScrollBegin();
    }

    protected override void OnDragShort(PointerEventData eventData)
    {
        base.OnDragShort(eventData);
    }

    protected override void OnEndDragShort(PointerEventData eventData)
    {
        base.OnEndDragShort(eventData);
        FrameManager.Instance.OnScrollEnd();
    }

    protected override void OnBeginDragLong(PointerEventData eventData)
    {
        base.OnBeginDragLong(eventData);
        _canvasGroup.blocksRaycasts = false;
        FrameManager.Instance.OnScrollBegin();
    }

    protected override void OnDragLong(PointerEventData eventData)
    {
        base.OnDragLong(eventData);
    }

    protected override void OnEndDragLong(PointerEventData eventData)
    {
        base.OnEndDragLong(eventData);
        _canvasGroup.blocksRaycasts = true;
        FrameManager.Instance.OnScrollEnd();
    }

    protected override void InvokeSwapItem(int index)
    {
        OnFrameSwapped?.Invoke(_previousIndex, index);
    }

    private void HandleFrameClicked(bool isSelected)
    {
        if(isSelected)
        {
            int index = transform.GetSiblingIndex();
            OnFrameClicked?.Invoke(index);
        }
    }

    private void OnFrameScroll(float min, float max)
    {
        float posX = Mathf.Abs(rect.localPosition.x);
        _isVisibleOnScroll = (posX >= min && posX <= max);

        //gameObject.SetActive(posX >= min && posX <= max);
    }
}
