#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class Texture2DAssetUtility
{
    /// <summary>
    /// Saves a Texture2D as a PNG asset in the project.
    /// </summary>
    /// <param name="texture">The Texture2D to save</param>
    /// <param name="assetPath">Relative path in Assets folder, e.g. "Assets/MyTexture.png"</param>
    public static void SaveTextureAsAsset(Texture2D texture, string assetPath)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is null!");
            return;
        }

        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(assetPath, bytes);

        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.Refresh();

        Debug.Log("Texture saved as asset at: " + assetPath);
    }
}
#endif