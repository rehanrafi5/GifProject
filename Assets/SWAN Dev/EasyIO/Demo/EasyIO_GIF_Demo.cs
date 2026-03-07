// Created by SWAN DEV 2019

using System.IO;
using UnityEngine;
using UnityEngine.UI;

using SDev;

/// <summary>
/// Shows how to save and load GIF using EasyIO.
/// 1. Download the GIF with URL, save to the persistent data path(for WebGL: IndexDB) with the filename beside the Play button;
/// 2. Click the Play button load it from the persistent data path and Play the GIF with ProGIF player. (Requires Pro GIF assets to play GIF)
/// </summary>
public class EasyIO_GIF_Demo : MonoBehaviour
{
    public InputField m_InputField_URL;

    public InputField m_InputField_FileName;

    public Slider m_SliderProgress;

    public Image m_Image;

    public DImageDisplayHandler m_ImageDisplayHandler;

    public Text m_StatusMessage;

    /// <summary> Play GIF with filename using EasyIO (persistence data path/WebGL IndexDB). </summary>
    public void PlayGif()
    {
#if PRO_GIF
        string fileNameWithExtension = m_InputField_FileName.text;

        m_StatusMessage.text = "";
        if (string.IsNullOrEmpty(fileNameWithExtension))
        {
            m_StatusMessage.text = "Please enter a filename!";
            return;
        }

        if (!fileNameWithExtension.EndsWith(".gif", System.StringComparison.OrdinalIgnoreCase))
        {
            fileNameWithExtension += ".gif";
        }

        string gifPath = EasyIO.GetFilePath(fileNameWithExtension);

        bool isPathExist = File.Exists(gifPath);
        m_StatusMessage.text = "Start play GIF: " + gifPath + (isPathExist ? "" : " (File Not Exist!)");
        if (!isPathExist) return;
        
        PGif.iSetAdvancedPlayerDecodeSettings(ProGifPlayerComponent.Decoder.ProGif_Coroutines); // use coroutine because threading Not supported on WebGL
        PGif.iPlayGif(gifPath, m_Image, m_Image.name,
            (f) =>
            {
                m_SliderProgress.value = f;
            }
            );

        PGif.iGetPlayer(m_Image.name).SetOnFirstFrameCallback(
            (gifFrame) =>
            {
                m_ImageDisplayHandler.SetImage(m_Image, gifFrame.width, gifFrame.height);
                m_StatusMessage.text = "Playing..";
            }
            );
#else
        Debug.Log("To play GIF, please insert the scripting define symbol 'PRO_GIF' in the PlayerSettings if you have Pro GIF asset in your project.");
#endif
    }

    public void PauseGif()
    {
#if PRO_GIF
        PGif.iGetPlayer(m_Image.name).Pause();
#endif
    }

    public void ResumeGif()
    {
#if PRO_GIF
        PGif.iGetPlayer(m_Image.name).Resume();
#endif
    }

    public void StopGif()
    {
#if PRO_GIF
        PGif.iGetPlayer(m_Image.name).Stop();
#endif
    }

    /// <summary> Download GIF by inputfield URL, save to persistence data path/WebGL IndexDB. </summary>
    public void DownloadGif()
    {
        string fileNameWithExtension = m_InputField_FileName.text;

        m_StatusMessage.text = "";
        if (string.IsNullOrEmpty(fileNameWithExtension))
        {
            m_StatusMessage.text = "Please enter a filename!";
            return;
        }

        if (!fileNameWithExtension.EndsWith(".gif", System.StringComparison.OrdinalIgnoreCase))
        {
            fileNameWithExtension += ".gif";
        }

        string url = m_InputField_URL.text;

        FilePathName fpn = new FilePathName();
        StartCoroutine(fpn.LoadFileUWR(url,
            (gifBytes) =>
            {
                EasyIO.SaveBytes(gifBytes, fileNameWithExtension);  // Save as file in the persistent data path (for WebGL: IndexDB)
                m_StatusMessage.text = "Download completed..";
            }
        ));
    }

    /// <summary> Delete the saved file. </summary>
    public void DeleteGif()
    {
        string fileNameWithExtension = m_InputField_FileName.text;

        m_StatusMessage.text = "";
        if (string.IsNullOrEmpty(fileNameWithExtension))
        {
            m_StatusMessage.text = "Please enter a filename!";
            return;
        }

        if (!fileNameWithExtension.EndsWith(".gif", System.StringComparison.OrdinalIgnoreCase))
        {
            fileNameWithExtension += ".gif";
        }

        EasyIO.DeleteFile(fileNameWithExtension);
    }
}
