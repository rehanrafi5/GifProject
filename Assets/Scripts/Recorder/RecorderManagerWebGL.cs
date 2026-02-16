using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class RecorderManagerWebGL : MonoBehaviour
{
    public static RecorderManagerWebGL Instance;

    [Header("GIF Settings")]
    public int width = 256;
    public int height = 256;
    public int fps = 10;
    public float duration = 3f;
    public string fileName = "unity_gif";

    [Header("Target UI")]
    public RectTransform targetRect;      // 👈 Assign the UI element to capture
    public Canvas targetCanvas;           // 👈 Canvas (ScreenSpace-Camera)
    public Camera uiCamera;               // 👈 Camera used by canvas

    [Header("Optional UI")]
    public GameObject successBanner;
    
    //[SerializeField] LoadingPopup _loadingPopup;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void InitGifJS();
    [DllImport("__Internal")] private static extern void StartGifRecordingJS(int width, int height, int fps, float duration, string fileName);
    [DllImport("__Internal")] private static extern void AddFrameToGifJS(byte[] pngData, int length);
    [DllImport("__Internal")] private static extern void FinishGifRecordingJS();
#endif

    private Texture2D captureTexture;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        captureTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    public void RecordRectTransformGif(string optionalName = "")
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!string.IsNullOrEmpty(optionalName))
            fileName = optionalName;

        InitGifJS();
        StartCoroutine(RecordRoutine());
#else
        Debug.LogWarning("WebGL only feature");
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private IEnumerator RecordRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        StartGifRecordingJS(width, height, fps, duration, fileName);

        int totalFrames = Mathf.CeilToInt(fps * duration);
        float interval = 1f / fps;

//_loadingPopup.SetHeader("GIF Creation");
        //_loadingPopup.SetDescription("Recording GIF...");
        //_loadingPopup.Show();

        for (int i = 0; i < totalFrames; i++)
        {
            yield return new WaitForEndOfFrame();

            CaptureRectTransform();

            byte[] pngBytes = captureTexture.EncodeToPNG();
            AddFrameToGifJS(pngBytes, pngBytes.Length);

            yield return new WaitForSeconds(interval);
        }

        FinishGifRecordingJS();

//_loadingPopup.Hide();

        if (successBanner != null)
            successBanner.SetActive(true);

        Debug.Log("GIF DONE");
    }

    private void CaptureRectTransform()
    {
        Vector3[] worldCorners = new Vector3[4];
        targetRect.GetWorldCorners(worldCorners);

        // Convert world corners → screen space
        Vector2 min = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCorners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCorners[2]);

        float rectWidth = max.x - min.x;
        float rectHeight = max.y - min.y;

        // FIX: y is inverted in ReadPixels
        Rect readRect = new Rect(min.x, min.y, rectWidth, rectHeight);

        // Temporary full size texture
        Texture2D temp = new Texture2D((int)rectWidth, (int)rectHeight, TextureFormat.RGB24, false);
        temp.ReadPixels(readRect, 0, 0);
        temp.Apply();

        // Scale into final gif size
        ScaleTexture(temp, captureTexture);

        Destroy(temp);
    }

    // 🔥 Built-in scaler (no TextureScale dependency)
    private void ScaleTexture(Texture2D src, Texture2D dst)
    {
        Color[] srcPixels = src.GetPixels();
        Color[] dstPixels = new Color[dst.width * dst.height];

        float incX = (1.0f / dst.width) * src.width;
        float incY = (1.0f / dst.height) * src.height;

        for (int px = 0; px < dstPixels.Length; px++)
        {
            int x = px % dst.width;
            int y = px / dst.width;

            int srcX = Mathf.FloorToInt(x * incX);
            int srcY = Mathf.FloorToInt(y * incY);

            dstPixels[px] = srcPixels[srcY * src.width + srcX];
        }

        dst.SetPixels(dstPixels);
        dst.Apply();
    }
#endif
}
