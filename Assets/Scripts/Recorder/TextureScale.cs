using UnityEngine;
using System.Threading;

public class TextureScale
{
    public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
    {
        Texture2D newTex = new Texture2D(newWidth, newHeight, tex.format, false);

        float ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
        float ratioY = 1.0f / ((float)newHeight / (tex.height - 1));

        for (int y = 0; y < newHeight; y++)
        {
            int yy = (int)Mathf.Floor(y * ratioY);
            for (int x = 0; x < newWidth; x++)
            {
                int xx = (int)Mathf.Floor(x * ratioX);
                newTex.SetPixel(x, y, tex.GetPixel(xx, yy));
            }
        }

        newTex.Apply();

        tex.Reinitialize(newWidth, newHeight);
        tex.SetPixels(newTex.GetPixels());
        tex.Apply();
    }
}