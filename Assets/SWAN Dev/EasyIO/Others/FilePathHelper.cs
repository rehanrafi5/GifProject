/// <summary>
/// By SwanDEV 2019
/// </summary>

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Display the filenames in the target directory/folder.
/// </summary>
public class FilePathHelper : MonoBehaviour
{
    /// <summary>
    /// Target directory to load file paths.
    /// </summary>
    public FilePathName.AppPath m_DataPath = FilePathName.AppPath.PersistentDataPath;

    public string m_OptionalSubFolder = "";

    public Text m_OptionalDisplayText;

    public string m_DisplayTextTitle = "";

    public bool m_AutoStartLoad = true;

    /// <summary>
    /// Display full file path or just display filename with the Text component.
    /// </summary>
    public bool m_DisplayFullPath = true;

    /// <summary>
    /// File type filter for getting file paths.
    /// </summary>
    public List<string> m_FileExtensions = new List<string>();

    /// <summary>
    /// List all file paths in the selected directory(m_DataPath).
    /// </summary>
    public List<string> m_FilePaths = new List<string>();

    void Start()
    {
        if (m_AutoStartLoad)
        {
            ListFiles();
        }
    }

    public void ListFiles()
    {
        FilePathName fpn = new FilePathName();
        string directory = Path.Combine(fpn.GetAppPath(m_DataPath), m_OptionalSubFolder);
        if (Directory.Exists(directory))
        {
            m_FilePaths = (m_FileExtensions != null && m_FileExtensions.Count > 0) ? fpn.GetFilePaths(directory, m_FileExtensions) : fpn.GetFilePaths(directory);

            if (m_OptionalDisplayText)
            {
                string showStr = string.IsNullOrEmpty(m_DisplayTextTitle) ? "" : m_DisplayTextTitle + m_DataPath.ToString() + "\n";
                for (int i = 0; i < m_FilePaths.Count; i++)
                {
                    showStr += "(" + i + ") " + (m_DisplayFullPath ? m_FilePaths[i] : Path.GetFileName(m_FilePaths[i])) + "\n";
                }
                m_OptionalDisplayText.text = showStr;
            }
        }
        else
        {
            string showStr = "Directory Not Found: " + directory;
            m_OptionalDisplayText.text = showStr;
            Debug.Log(showStr);
        }
    }

}
