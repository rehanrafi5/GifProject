/// <summary>
/// Created by SWAN DEV 2019
/// </summary>
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

namespace SDev
{
    /// <summary>
    /// Save/Load files and contents from the persistentDataPath on supported platforms. 
    /// Works on WebGL with the provided EasyIOHandler plugin to sync files in IndexedDB.
    /// (Supports platform: Mac, Windows, Android, iOS, WebGL tested)
    /// </summary>
    public class EasyIO
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void EasyIO_WebGL_SyncFiles();

        [DllImport("__Internal")]
        private static extern void EasyIO_WebGL_WindowAlert(string message);

        [DllImport("__Internal")]
        private static extern void EasyIO_WebGL_SaveToLocal(byte[] array, int byteLength, string fileName);
#endif

        #region ----- Texture2D Save / Load -----
        public enum ImageEncodeFormat
        {
            PNG,
            JPG,
        }

        public static string SaveImage(Texture2D texture2D, ImageEncodeFormat imageEncodeFormat, string fileNameWithExtension, string subfolder_Optional = "")
        {
            byte[] imageData = imageEncodeFormat == ImageEncodeFormat.JPG ? texture2D.EncodeToJPG() : texture2D.EncodeToPNG();
            return SaveBytes(imageData, fileNameWithExtension, subfolder_Optional);
        }

        public static Texture2D LoadImage(string fileNameWithExtension, string subfolder_Optional = "", bool markLoadedTextureAsNonReadable = false)
        {
            string fullFilePath = GetFilePath(fileNameWithExtension, subfolder_Optional);
            return LoadImage(fullFilePath, markLoadedTextureAsNonReadable);
        }

        public static Texture2D LoadImage(string fullFilePath, bool markLoadedTextureAsNonReadable = false)
        {
            byte[] imageData = LoadBytes(fullFilePath);
            Texture2D texture2D = new Texture2D(1, 1);
            if (texture2D.LoadImage(imageData, markLoadedTextureAsNonReadable))
            {
                return texture2D;
            }
            return null;
        }

        /// <summary>
        /// Delete an image from persistent data path, by previously used filename and subfolder name.
        /// </summary>
        /// <param name="subfolder_Optional"> The name of the folder previously used to save images (if any). </param>
        public static void DeleteImage(string fileNameWithExtension, string subfolder_Optional = "")
        {
            DeleteFile(fileNameWithExtension, subfolder_Optional);
        }

        public static void DeleteImage(string fullFilePath)
        {
            _Delete(fullFilePath);
        }
        #endregion

        #region ----- File Byte Array Save / Load -----
        public static string SaveBytes(byte[] bytes, string fileNameWithExtension, string subfolder_Optional = "")
        {
            string fullFilePath = GetFilePath(fileNameWithExtension, subfolder_Optional);
            if (_Save(fullFilePath, bytes))
            {
                return fullFilePath;
            }
            return string.Empty;
        }

        public static byte[] LoadBytes(string fileNameWithExtension, string subfolder_Optional = "")
        {
            string dataPath = GetFilePath(fileNameWithExtension, subfolder_Optional);
            return LoadBytes(dataPath);
        }

        public static byte[] LoadBytes(string fullFilePath)
        {
            if (!File.Exists(fullFilePath))
            {
                PlatformSafeMessage("File not exists!");
                return null;
            }
            return File.ReadAllBytes(fullFilePath);
        }

        public static void DeleteFile(string fileNameWithExtension, string subfolder_Optional = "")
        {
            string fullFilePath = GetFilePath(fileNameWithExtension, subfolder_Optional);
            _Delete(fullFilePath);
        }

        public static void DeleteFile(string fullFilePath)
        {
            _Delete(fullFilePath);
        }

        public static bool _Save(string fullFilePath, byte[] bytes)
        {
            FileStream fileStream;
            try
            {
                if (File.Exists(fullFilePath))
                {
                    File.WriteAllText(fullFilePath, string.Empty);
                    fileStream = File.Open(fullFilePath, FileMode.Open);
                }
                else
                {
                    fileStream = File.Create(fullFilePath);
                }
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Close();
                WebGL_SyncFiles();
            }
            catch (Exception e)
            {
                PlatformSafeMessage("Failed to Save: " + e.Message);
                return false; // Fail
            }
            return true; // Success
        }

        public static void _Delete(string fullFilePath)
        {
            if (!File.Exists(fullFilePath))
            {
                //PlatformSafeMessage("File not exists: " + fullFilePath);
                return;
            }
            File.Delete(fullFilePath);
            WebGL_SyncFiles();
        }
        #endregion

        #region ----- String Save / Load -----
        public static bool SaveString(string saveStr, string key, string subfolder_Optional = "")
        {
            string fileNameWithExtension = key + ".dat";
            string dataPath = GetFilePath(fileNameWithExtension, subfolder_Optional);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream;
            try
            {
                if (File.Exists(dataPath))
                {
                    File.WriteAllText(dataPath, string.Empty);
                    fileStream = File.Open(dataPath, FileMode.Open);
                }
                else
                {
                    fileStream = File.Create(dataPath);
                }
                binaryFormatter.Serialize(fileStream, saveStr);
                fileStream.Close();
                WebGL_SyncFiles();
            }
            catch (Exception e)
            {
                PlatformSafeMessage("Failed to Save string: " + e.Message);
                return false; // Fail
            }
            return true; // Success
        }

        public static string LoadString(string key, string subfolder_Optional = "")
        {
            string fileNameWithExtension = key + ".dat";
            string loadStr = null;
            string dataPath = GetFilePath(fileNameWithExtension, subfolder_Optional);
            try
            {
                if (File.Exists(dataPath))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    FileStream fileStream = File.Open(dataPath, FileMode.Open);
                    loadStr = (string)binaryFormatter.Deserialize(fileStream);
                    fileStream.Close();
                }
            }
            catch (Exception e)
            {
                PlatformSafeMessage("Failed to Load string: " + e.Message);
            }
            return loadStr;
        }

        public static void DeleteString(string key, string subfolder_Optional = "")
        {
            string fileNameWithExtension = key + ".dat";
            DeleteFile(fileNameWithExtension, subfolder_Optional);
        }
        #endregion

        #region ----- Serializable 'class' object Save / Load -----
        /// <summary> Save a class instance in the persistent data path (or WebGL IndexDB). The class must be marked as [Serializable]. </summary>
        public static bool SaveClassObject<T>(T classObject, string key, string subfolder_Optional = "") where T : class
        {
            string fileNameWithExtension = key + ".dat";
            string dataPath = GetFilePath(fileNameWithExtension, subfolder_Optional);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream;
            try
            {
                if (File.Exists(dataPath))
                {
                    File.WriteAllText(dataPath, string.Empty);
                    fileStream = File.Open(dataPath, FileMode.Open);
                }
                else
                {
                    fileStream = File.Create(dataPath);
                }
                binaryFormatter.Serialize(fileStream, classObject);
                fileStream.Close();
                WebGL_SyncFiles();
            }
            catch (Exception e)
            {
                PlatformSafeMessage("Failed to Save class object: " + e.Message);
                return false; // Fail
            }
            return true; // Success
        }

        /// <summary> Load a class instance from the Serialized file that saved previously. The class must be marked as [Serializable]. </summary>
        public static T LoadClassObject<T>(string key, string subfolder_Optional = "") where T : class
        {
            string fileNameWithExtension = key + ".dat";
            T classObject = null;
            string dataPath = GetFilePath(fileNameWithExtension, subfolder_Optional);
            try
            {
                if (File.Exists(dataPath))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    FileStream fileStream = File.Open(dataPath, FileMode.Open);
                    classObject = (T)binaryFormatter.Deserialize(fileStream);
                    fileStream.Close();
                }
            }
            catch (Exception e)
            {
                PlatformSafeMessage("Failed to Load class object: " + e.Message);
            }
            return classObject;
        }

        public static void DeleteClassObject(string key, string subfolder_Optional = "")
        {
            string fileNameWithExtension = key + ".dat";
            DeleteFile(fileNameWithExtension, subfolder_Optional);
        }
        #endregion

        #region ----- WebGL : Save to Local Storage -----
        public static void WebGL_SaveToLocal(byte[] data, string filenameWithExtension)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            EasyIO_WebGL_SaveToLocal(data, data.Length, filenameWithExtension);
#endif
        }
        public static void WebGL_SaveToLocal(Texture2D texture, string filename, ImageEncodeFormat imageFormat)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            byte[] data = imageFormat == ImageEncodeFormat.PNG ? texture.EncodeToPNG() : texture.EncodeToJPG();
            string extension = "." + imageFormat.ToString().ToLower();
            EasyIO_WebGL_SaveToLocal(data, data.Length, filename + extension);
#endif
        }
        #endregion

        public static string GetFilePath(string fileNameWithExtension, string subfolder_Optional = "")
        {
            if (string.IsNullOrEmpty(subfolder_Optional)) subfolder_Optional = "Default_Folder"; //"EasyIO";
            string path = string.Format("{0}/" + subfolder_Optional + "/" + fileNameWithExtension, Application.persistentDataPath); // WebGL: persistence data path only
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return path;
        }

        public static void PlatformSafeMessage(string message)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            EasyIO_WebGL_WindowAlert(message);
#else
            Debug.Log(message);
#endif
        }

        public static void WebGL_SyncFiles()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
                EasyIO_WebGL_SyncFiles();
#endif
        }
    }

}
