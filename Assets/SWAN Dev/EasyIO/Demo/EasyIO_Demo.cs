using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SDev;

/// <summary>
/// Shows how to save and load text and class using EasyIO.
/// </summary>
public class EasyIO_Demo : MonoBehaviour
{
    public Text m_StatusMessage;

    public Texture2D m_TestImage;

    public TextAsset m_TextAsset;

    public void TestSaveData()
    {
        // Save text
        EasyIO.SaveString("Planets: \n1.Mercury\n2.Venus\n3.Earth\n4.Mars\n5.Jupiter\n6.Saturn..", "Planets");

        // Save instance of Serializable class
        EasyIOTestClass testClass = new EasyIOTestClass()
        {
            Name = "TestUser",
            ID = "1234567890",
            Value1 = 835574,
            Value2 = 53368,

            Values = new string[]
            {
                "A 2013-2015",
                "B 2016",
                "C 2019-2020",
                "D 2020",
                "All 2047"
            }
        };
        EasyIO.SaveClassObject<EasyIOTestClass>(testClass, "User1");
    }

    public void TestLoadData()
    {
        // Load instance of Serializable class
        EasyIOTestClass testClass = EasyIO.LoadClassObject<EasyIOTestClass>("User1");
        string testClassDump = testClass == null ? "User not found: User1" : testClass.Dump();

        // Load text
        string loadString = EasyIO.LoadString("Planets");
        Debug.Log(string.IsNullOrEmpty(loadString) ? "Text not found: Planets" : loadString);

        m_StatusMessage.text = "[Load Class Object]\n" + testClassDump + "\n\n[Load Text]\n" + (string.IsNullOrEmpty(loadString) ? "Text not found: Planets" : loadString);
    }

    public void SaveImage()
    {
        string path = EasyIO.SaveImage(m_TestImage, EasyIO.ImageEncodeFormat.PNG, "Image_001.png", "My Images");
        Debug.Log("Image save path: " + path);
    }

    public void LoadImage(RawImage rawImage)
    {
        var tex = EasyIO.LoadImage("Image_001.png", "My Images", true);
        if (tex)
        {
            // display the loaded image
            rawImage.GetComponent<DImageDisplayHandler>().SetRawImage(rawImage, tex);
        }
    }

    public void DeleteClassObject()
    {
        EasyIO.DeleteClassObject("User1");
    }

    public void DeleteText()
    {
        EasyIO.DeleteString("Planets");
    }
    
    public void DeleteImage()
    {
        EasyIO.DeleteImage("Image_001.png", "My Images");
    }

    public void WebGL_SaveImageToLocal()
    {
        EasyIO.WebGL_SaveToLocal(m_TestImage, FilePathName.Instance.GetFileNameWithoutExt(), EasyIO.ImageEncodeFormat.PNG);
    }

    public void WebGL_SaveFileToLocal()
    {
        EasyIO.WebGL_SaveToLocal(m_TextAsset.bytes, m_TextAsset.name + ".pdf");
    }
}
