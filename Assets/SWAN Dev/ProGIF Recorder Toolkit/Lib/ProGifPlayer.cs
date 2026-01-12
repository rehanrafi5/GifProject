using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class ProGifPlayer
{
	private ProGifPlayerComponent player = null;
    public ProGifPlayerComponent playerComponent
    {
        get
        {
            return player;
        }
    }

	public ProGifPlayerComponent.PlayerState State
	{
		get{
			return (player == null)? ProGifPlayerComponent.PlayerState.None:player.State;
		}
	}

	/// <summary>
	/// Gif width (only available after the first gif frame is loaded)
	/// </summary>
	public int width
	{
		get{
			return (player == null)? 0:player.width;
		}
	}

	/// <summary>
	/// Gif height (only available after the first gif frame is loaded)
	/// </summary>
	public int height
	{
		get{
			return (player == null)? 0:player.height;
		}
	}

	private bool optimizeMemoryUsage = true;

	public ImageRotator.Rotation rotation = ImageRotator.Rotation.None;

	public string savePath = "";

	/// <summary>
	/// Decoded gif texture list (get all the gif textures at the decoding process finished)
	/// </summary>
	public List<GifTexture> gifTextures
	{
		get{
			return (player == null)? null:player.gifTextures;
		}
	}

	private void _SetupPlayerComponent(UnityEngine.UI.Image destination)
	{
		player = destination.gameObject.GetComponent<ProGifPlayerImage>();
		if(player == null)
		{
			player = destination.gameObject.AddComponent<ProGifPlayerImage>();
		}
		player.displayType = ProGifPlayerComponent.DisplayType.Image;
	}

	private void _SetupPlayerComponent(Renderer destination)
	{
		player = destination.gameObject.GetComponent<ProGifPlayerRenderer>();
		if(player == null)
		{
			player = destination.gameObject.AddComponent<ProGifPlayerRenderer>();
		}
		player.displayType = ProGifPlayerComponent.DisplayType.Renderer;
	}

#if PRO_GIF_GUITEXTURE
	private void _SetupPlayerComponent(GUITexture destination)
	{
		player = destination.gameObject.GetComponent<ProGifPlayerGuiTexture>();
		if(player == null)
		{
			player = destination.gameObject.AddComponent<ProGifPlayerGuiTexture>();
		}
		player.displayType = ProGifPlayerComponent.DisplayType.GuiTexture;
	}
#endif

	private void _SetupPlayerComponent(RawImage destination)
	{
		player = destination.gameObject.GetComponent<ProGifPlayerRawImage>();
		if (player == null)
		{
			player = destination.gameObject.AddComponent<ProGifPlayerRawImage>();
		}
		player.displayType = ProGifPlayerComponent.DisplayType.RawImage;
	}

	/// <summary>
	/// Play gif frames in the specified recorder, display with image.
	/// </summary>
	public void Play(ProGifRecorder recorder, UnityEngine.UI.Image destination, bool optimizeMemoryUsage)
	{
		_SetupPlayerComponent(destination);
		_PlayRecorder(recorder, optimizeMemoryUsage);
	}

	/// <summary>
	/// Play gif frames in the specified recorder, display with renderer.
	/// </summary>
	public void Play(ProGifRecorder recorder, Renderer destination, bool optimizeMemoryUsage)
	{
		_SetupPlayerComponent(destination);
		_PlayRecorder(recorder, optimizeMemoryUsage);
	}

#if PRO_GIF_GUITEXTURE
	/// <summary>
	/// Play gif frames in the specified recorder, display with GUITexture.
	/// </summary>
	public void Play(ProGifRecorder recorder, GUITexture destination, bool optimizeMemoryUsage)
	{
		_SetupPlayerComponent(destination);
		_PlayRecorder(recorder, optimizeMemoryUsage);
	}
#endif

	/// <summary>
	/// Play gif frames in the specified recorder, display with RawImage.
	/// </summary>
	public void Play(ProGifRecorder recorder, RawImage destination, bool optimizeMemoryUsage)
	{
		_SetupPlayerComponent(destination);
		_PlayRecorder(recorder, optimizeMemoryUsage);
	}

	private void _PlayRecorder(ProGifRecorder recorder, bool optimizeMemoryUsage)
	{
		this.optimizeMemoryUsage = optimizeMemoryUsage;
		this.rotation = recorder.Rotation;
		this.savePath = recorder.SavedFilePath;
        recorder.recorderCom.ComputeCropSize();
		player.Play(recorder.Frames, recorder.FPS, recorder.IsCustomRatio, recorder.Width, recorder.Height, this.optimizeMemoryUsage);
	}


	public void Pause()
	{
		player.Pause();
	}

	public void Resume()
	{
		player.Resume();
	}

	public void Stop()
	{
		player.Stop();
	}

	public bool isReversed = false;
    /// <summary>
    /// Reverse the gif texture list. 
    /// You can use this method to implement a reverse playback mode. Call this again to set back to normal playback direction.
    /// Also make sure all textures imported/loaded to the player first.
    /// </summary>
    /// <returns></returns>
    public int Reverse()
	{
		isReversed = !isReversed;

		int newIndex = (gifTextures.Count - 1) - playerComponent.spriteIndex;
		gifTextures.Reverse();
		playerComponent.spriteIndex = newIndex;
		return newIndex;
	}

	public bool isPingPong = false;
    /// <summary>
    /// Sets the target gif player to play with ping-pong play mode.
    /// This method utilizes the OnPlayingCallback. Please make sure not to override the callback.
    /// Also make sure all textures imported/loaded to the player first.
    /// </summary>
    public void PingPong()
	{
		isPingPong = true;
		SetOnPlayingCallback((gifTex)=>{
			if(playerComponent != null)
			{
				int currentSpriteIndex = playerComponent.spriteIndex;
				if(currentSpriteIndex == 0) gifTextures.Reverse();
            }
		});
	}
	/// Cancel the ping-pong play mode if any.
	/// This method will clear the OnPlayingCallback.
	public void CancelPingPong()
	{
		isPingPong = false;
		SetOnPlayingCallback(null);
	}


	/// <summary>
	/// Set the callback for checking the texture import progress for gif instant preview.
	/// </summary>
	/// <param name="onLoading">On loading callback, returns the texture import progress(float).</param>
	public void SetLoadingCallback(Action<float> onLoading)
	{
		if(player != null)
		{
			player.SetLoadingCallback(onLoading);
		}
		else
		{
			Debug.LogWarning("Gif player not exist, please set callback after the player is set!");
		}
	}

	/// <summary>
	/// Set the callback to be fired when the first gif frame ready.
	/// If using a recorder source for playback, this becomes a loading-complete callback with the first GIF frame returned.
	/// </summary>
	/// <param name="onFirstFrame">On first frame callback, returns the first gifTexture and related data.</param>
	public void SetOnFirstFrameCallback(Action<ProGifPlayerComponent.FirstGifFrame> onFirstFrame)
	{
		if(player != null)
		{
			player.SetOnFirstFrameCallback(onFirstFrame);
		}
		else
		{
			Debug.LogWarning("Gif player not exist, please set callback after the player is set!");
		}
	}

	/// <summary>
	/// Set the callback to be fired on every frame during play gif.
	/// </summary>
	/// <param name="onPlaying">On playing callback, returns the gifTexture of the current playing frame.</param>
	public void SetOnPlayingCallback(Action<GifTexture> onPlaying)
	{
		if(player != null)
		{
			player.SetOnPlayingCallback(onPlaying);
		}
		else
		{
			Debug.LogWarning("Gif player not exist, please set callback after the player is set!");
		}
	}

	/// <summary>
	/// Change the destination image for displaying gif.
	/// </summary>
	public void ChangeDestination(UnityEngine.UI.Image destination)
	{
		if(player.GetComponent<ProGifPlayerImage>() != null)
		{
			player.GetComponent<ProGifPlayerImage>().ChangeDestination(destination);
		}
	}

	/// <summary>
	/// Change the destination renderer for displaying gif.
	/// </summary>
	public void ChangeDestination(Renderer destination)
	{
		if(player.GetComponent<ProGifPlayerRenderer>() != null)
		{
			player.GetComponent<ProGifPlayerRenderer>().ChangeDestination(destination);
		}
	}

	/// <summary>
	/// Add an extra destination image for displaying gif.
	/// </summary>
	public void AddExtraDestination(UnityEngine.UI.Image destination)
	{
		if(player.GetComponent<ProGifPlayerImage>() != null)
		{
			player.GetComponent<ProGifPlayerImage>().AddExtraDestination(destination);
		}
	}

	/// <summary>
	/// Add an extra destination renderer for displaying gif.
	/// </summary>
	public void AddExtraDestination(Renderer destination)
	{
		if(player.GetComponent<ProGifPlayerRenderer>() != null)
		{
			player.GetComponent<ProGifPlayerRenderer>().AddExtraDestination(destination);
		}
	}

#if PRO_GIF_GUITEXTURE
	/// <summary>
	/// Add an extra destination guiTexture for displaying gif.
	/// </summary>
	public void AddExtraDestination(GUITexture destination)
	{
		if(player.GetComponent<ProGifPlayerGuiTexture>() != null)
		{
			player.GetComponent<ProGifPlayerGuiTexture>().AddExtraDestination(destination);
		}
	}
#endif

	/// <summary>
	/// Remove a specific extra destination image from the extra list.
	/// </summary>
	public void RemoveFromExtraDestination(UnityEngine.UI.Image destination)
	{
		if(player.GetComponent<ProGifPlayerImage>() != null)
		{
			player.GetComponent<ProGifPlayerImage>().RemoveFromExtraDestination(destination);
		}
	}

	/// <summary>
	/// Remove a specific extra destination renderer from the extra list.
	/// </summary>
	public void RemoveFromExtraDestination(Renderer destination)
	{
		if(player.GetComponent<ProGifPlayerRenderer>() != null)
		{
			player.GetComponent<ProGifPlayerRenderer>().RemoveFromExtraDestination(destination);
		}
	}

#if PRO_GIF_GUITEXTURE
	/// <summary>
	/// Remove a specific extra destination guiTexture from the extra list.
	/// </summary>
	public void RemoveFromExtraDestination(GUITexture destination)
	{
		if(player.GetComponent<ProGifPlayerGuiTexture>() != null)
		{
			player.GetComponent<ProGifPlayerGuiTexture>().RemoveFromExtraDestination(destination);
		}
	}
#endif

	/// <summary>
	/// Clear this instance, clear all textures/sprites.
	/// </summary>
	public void Clear()
	{
		if(player != null)
		{
			player.Clear();
		}
	}
}
