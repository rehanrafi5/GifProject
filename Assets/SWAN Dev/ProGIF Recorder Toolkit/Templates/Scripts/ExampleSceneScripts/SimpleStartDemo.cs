// Created by SwanDev 2017
using UnityEngine;

public class SimpleStartDemo : MonoBehaviour
{
	public TextMesh textGameState;
	public TextMesh textGifState;

	public float gameTimingToStopRecord = 12f;
	private bool gameEnd = false;

	public Camera mCamera = null;

	/// <summary> The recorder will save gif using this filename if this is provided. The new gif will replace the old one if their filename are the same. </summary>
	[Space()]
	[Tooltip("The recorder will save gif using this filename if this is provided. The new gif will replace the old one if their filename are the same.")]
	public string optionalGifFileName = "MyGif";

	[Header("Native Save (+MobileMediaPlugin)")]
	public bool saveToNative = false;
	public bool deleteOriginGif = false;
	public string folderName = "GIF Demo";


	void Start()
	{
		// Get the instance of ProGifManager
		ProGifManager gifMgr = ProGifManager.Instance;

		// Make some changes to the record settings, you can let it auto aspect based on screen size...
		gifMgr.SetRecordSettings(true, 300, 300, 3, 15, 1, 30);
		// Or, give an aspect ratio for cropping gif frames just before encoding:
		//gifMgr.SetRecordSettings(new Vector2(1, 1), 300, 300, 1, 15, 0, 30);

		// Start gif recording
		gifMgr.StartRecord((mCamera != null) ? mCamera : Camera.main,
			onRecordProgress: (progress) => // onRecordProgress callback
			{
				Debug.Log("[SimpleStartDemo] On record progress: " + progress);
			},
			onRecordDurationMax: () =>      // onRecordDurationMax callback
			{
				Debug.Log("[SimpleStartDemo] On recorder buffer max.");
			});

		textGameState.text = "Game Started";
		textGifState.text = "Start Record..";
	}

	private float nextUpdateTime = 0f;
	void Update()
	{
		if (gameEnd) return;

		if (Time.time > nextUpdateTime)
		{
			//nextUpdateTime = Time.time + 0.5f;
			Camera.main.backgroundColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
		}

		if (Time.time > gameTimingToStopRecord - 1f)
		{
			textGameState.text = "Game Over";
		}

		if (Time.time > gameTimingToStopRecord)
		{
			gameEnd = true;
			ProGifManager gifMgr = ProGifManager.Instance;

			// Stop the recording
			gifMgr.StopAndSaveRecord(

				onRecorderPreProcessingDone: () => // onRecorderPreProcessingDone callback
				{
					Debug.Log("[SimpleStartDemo] On pre-processing done.");
				},

				onFileSaveProgress: (id, progress) => // onFileSaveProgress callback
				{
					if (progress < 1f)
					{
						textGifState.text = "Making Gif : " + Mathf.CeilToInt(progress * 100) + "%";
					}
					else
					{
						textGifState.text = "The gif file is created, find the path in the debug console.";
					}
				},

				onFileSaved: (id, path) => // onFileSaved callback
				{
					// Clear the existing recorder, player and reset gif manager
					gifMgr.Clear();
					Debug.Log("[SimpleStartDemo] On saved, origin save path: " + path);

					if (saveToNative)
					{
#if SDEV_MobileMedia
						// Copy the newly created GIF file from internal path to external, i.e. Android/iOS device gallery
						MobileMedia.CopyMedia(path, folderName, System.IO.Path.GetFileNameWithoutExtension(path), ".gif", true);
						/*MobileMedia.SaveBytes(System.IO.File.ReadAllBytes(path), "YourGifFolderName", "YourGifFileName", ".gif", true);*/

						if (deleteOriginGif) System.IO.File.Delete(path);
#else
						Debug.LogWarning("To save media files to device Gallery, please add 'SDEV_MobileMedia' to: Build Settings > Player Settings > Player > Other > Scripting Define Symbols");
#endif
					}
					else
					{
						// Get target save path
						string targetDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), folderName);
						string moveToPath = System.IO.Path.Combine(targetDirectory, System.IO.Path.GetFileName(path));

						// Move the GIF to target path
						FilePathName.Instance.MoveFile(path, moveToPath, replaceIfExist: true);
#if UNITY_EDITOR
						// Editor only: view the GIF in editor
						UnityEditor.EditorUtility.RevealInFinder(targetDirectory);
						Debug.Log("[SimpleStartDemo] On saved, moved to path: " + moveToPath);
#endif
					}
				},

				optionalGifFileName);
		}
	}
}