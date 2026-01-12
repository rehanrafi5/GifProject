using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

[DisallowMultipleComponent]
public abstract class ProGifPlayerComponent : MonoBehaviour
{
    private bool _ignoreTimeScale = true;
    public bool ignoreTimeScale
    {
        get
        {
            return _ignoreTimeScale;
        }
        set
        {
            _ignoreTimeScale = value;
            nextFrameTime = value ? Time.unscaledTime : Time.time;
        }
    }

    [HideInInspector] public List<GifTexture> gifTextures = new List<GifTexture>();
    private int totalFrame = 0;

	[HideInInspector] public DisplayType displayType = DisplayType.None;	// Indicates the display target is an Image, Renderer, or GUITexture
	public enum DisplayType
	{
		None = 0,
		Image,
		Renderer,
		GuiTexture,
		RawImage,
	}

	[HideInInspector] public float nextFrameTime = 0.0f;					// The game time to show next frame

	[HideInInspector] public int spriteIndex = 0;                           // The current sprite index to be played

    /// <summary> Default waiting time among frames. </summary> 
    private float _interval = 0.1f;
    /// <summary> Get the current frame waiting time. </summary> 
    public float interval
    {
        get
        {
            if (gifTextures.Count <= 0) return 0.1f;
            // For GIF that the delaySec is incorrectly set, we use 0.1f as its delaySec (or 10 FPS), which is one of the widely used framerate for GIF.
            return (gifTextures[spriteIndex].m_delaySec <= 0.0166f) ? 0.1f : gifTextures[spriteIndex].m_delaySec; // Here we let the maximum FPS of GIF be 60, while in the GIF specification it is 30. 
        }
    }

    /// <summary> Current playback speed of the GIF. </summary> 
    private float _playbackSpeed = 1.0f;
    /// <summary> Get/Set the playback speed of the GIF. (Default is 1.0f) </summary> 
    public float playbackSpeed
    {
        get
        {
            return _playbackSpeed;
        }
        set
        {
            float prevInterval = interval / _playbackSpeed;

            _playbackSpeed = Mathf.Max(0.01f, value);

            // update the next frame time
            float time = ignoreTimeScale ? Time.unscaledTime : Time.time;
            float timeLeftPercent = nextFrameTime > time ? (nextFrameTime - time) / prevInterval : 0f;
            nextFrameTime = time + (interval / _playbackSpeed) * timeLeftPercent;
        }
    }

    /// Set to 'true' to take advantage of the highly optimized ProGif playback solution for significantly save the memory usage.
    public bool optimizeMemoryUsage = true;

	/// <summary>
	/// Gets the progress when load Gif from path/url.
	/// </summary>
	/// <value>The loading progress.</value>
	public float LoadingProgress
	{
		get
        {
            return (float)gifTextures.Count / (float)totalFrame;
        }
	}

    public bool IsLoadingComplete
    {
        get
        {
            return LoadingProgress >= 1f;
        }
    }

	/// <summary>
	/// This component state
	/// </summary>
	public enum PlayerState
	{
		None,
		Loading,
		Ready,
		Playing,
		Pause,
	}
    private PlayerState _state;
    /// <summary> Current state </summary>
    public PlayerState State
    {
        get
        {
            return _state;
        }
        private set
        {
            _state = value;
            nextFrameTime = ignoreTimeScale ? Time.unscaledTime : Time.time;
        }
    }
    public void SetState(PlayerState state)
    {
        State = state;
    }

    /// <summary>
    /// Animation loop count (0 is infinite)
    /// </summary>
    public int loopCount
	{
		get;
		private set;
	}

	/// <summary>
	/// Texture width (px)
	/// </summary>
	public int width
	{
		get;
		private set;
	}

	/// <summary>
	/// Texture height (px)
	/// </summary>
	public int height
	{
		get;
		private set;
	}

	/// Setup to play the stored textures from gif recorder.
	public virtual void Play(RenderTexture[] gifFrames, int fps, bool isCustomRatio, int customWidth, int customHeight, bool optimizeMemoryUsage)
	{
        gifTextures = new List<GifTexture>();

        this.optimizeMemoryUsage = optimizeMemoryUsage;

        _interval = 1.0f / fps;

        Clear();

        totalFrame = gifFrames.Length;

        StartCoroutine(_AddGifTextures(gifFrames, fps, isCustomRatio, customWidth, customHeight, optimizeMemoryUsage, 0, yieldPerFrame: true));

        StartCoroutine(_DelayCallback());

        State = PlayerState.Playing;
    }

    private IEnumerator _AddGifTextures(RenderTexture[] gifFrames, float fps, bool isCustomRatio, int customWidth, int customHeight, bool optimizeMemoryUsage, int currentIndex, bool yieldPerFrame)
    {
        int i = currentIndex;

        if (isCustomRatio)
        {
            width = customWidth;
            height = customHeight;
            Texture2D tex = new Texture2D(width, height);
            RenderTexture.active = gifFrames[i];
            tex.ReadPixels(new Rect((gifFrames[i].width - tex.width) / 2, (gifFrames[i].height - tex.height) / 2, tex.width, tex.height), 0, 0);
            tex.Apply();
            gifTextures.Add(new GifTexture(tex, _interval, optimizeMemoryUsage));
        }
        else
        {
            width = gifFrames[0].width;
            height = gifFrames[0].height;
            Texture2D tex = new Texture2D(gifFrames[i].width, gifFrames[i].height);
            RenderTexture.active = gifFrames[i];
            tex.ReadPixels(new Rect(0.0f, 0.0f, gifFrames[i].width, gifFrames[i].height), 0, 0);
            tex.Apply();
            gifTextures.Add(new GifTexture(tex, _interval, optimizeMemoryUsage));
        }

        if (currentIndex == 1) OnLoading(LoadingProgress);

        if (yieldPerFrame) yield return new WaitForEndOfFrame();

        if (OnLoading != null) OnLoading(LoadingProgress);

        currentIndex++;

        if (currentIndex < gifFrames.Length)
        {
            StartCoroutine(_AddGifTextures(gifFrames, fps, isCustomRatio, customWidth, customHeight, optimizeMemoryUsage, currentIndex, yieldPerFrame));
        }
        else
        {
            // Texture import finished
        }
    }

    IEnumerator _DelayCallback()
	{
		yield return new WaitForEndOfFrame();
        _OnFrameReady(gifTextures[0], true);
        if (gifTextures != null && gifTextures.Count > 0) _OnFirstFrameReady(gifTextures[0]);
        //if (_OnLoading != null) _OnLoading(LoadingProgress);
	}

	public void Pause()
	{
		State = PlayerState.Pause;
	}

	public void Resume()
	{
		State = PlayerState.Playing;
	}

	public void Stop()
	{
		State = PlayerState.Pause;
		spriteIndex = 0;
	}

	/// <summary>
	/// This is called on every gif frame decode finish
	/// </summary>
	/// <param name="gTex">GifTexture.</param>
	protected abstract void _OnFrameReady(GifTexture gTex, bool isFirstFrame);

	public void _OnFirstFrameReady(GifTexture gifTex)
	{
		_interval = gifTex.m_delaySec;
		width = gifTex.m_Width;
		height = gifTex.m_Height;
		if(OnFirstFrame != null)
		{
			OnFirstFrame(new FirstGifFrame(){
				gifTexture = gifTex,
				width = this.width,
				height = this.height,
				interval = this.interval,
				totalFrame = this.totalFrame,
			});
		}
		State = PlayerState.Playing;
	}

	public Action<FirstGifFrame> OnFirstFrame = null;
	public void SetOnFirstFrameCallback(Action<FirstGifFrame> onFirstFrame)
	{
		OnFirstFrame = onFirstFrame;
	}

	public class FirstGifFrame
	{
		public GifTexture gifTexture;
		public int width;
		public int height;
		public float interval;
		public int totalFrame;

		public int fps
		{
			get{
				return (int)(1f/interval);
			}
		}
	}

	public Action<float> OnLoading = null;
	public void SetLoadingCallback(Action<float> onLoading)
	{
		OnLoading = onLoading;
	}

	public Action<GifTexture> OnPlayingCallback = null;
	public void SetOnPlayingCallback(Action<GifTexture> onPlayingCallback)
	{
		OnPlayingCallback = onPlayingCallback;
	}

	public bool ByteArrayToFile(string fileName, byte[] byteArray)
	{
		try
		{
			using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				fs.Write(byteArray, 0, byteArray.Length);
				return true;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Exception caught in process: {0}", ex);
			return false;
		}
	}

	/// <summary>
	/// Clear the sprite & texture2D in the list of GifTexture
	/// </summary>
	protected void _ClearGifTextures(List<GifTexture> gifTexList)
	{
		if(gifTexList != null)
		{
			for(int i=0; i<gifTexList.Count; i++)
			{
				if(gifTexList[i] != null)
				{
					if(gifTexList[i].m_texture2d != null)
					{
						Texture2D.Destroy(gifTexList[i].m_texture2d);
						gifTexList[i].m_texture2d = null;
					}

					if(gifTexList[i].m_Sprite != null && gifTexList[i].m_Sprite.texture != null)
					{
						Texture2D.Destroy(gifTexList[i].m_Sprite.texture);
						gifTexList[i].m_Sprite = null;
					}
				}
			}
		}
	}

	public virtual void Clear()
	{
        State = PlayerState.None;
        spriteIndex = 0;
        nextFrameTime = 0f;

        StopAllCoroutines();

		//Clear sprite & texture in gifTextures of the PlayerComponent
		_ClearGifTextures(gifTextures);

		//Clear un-referenced textures
		Resources.UnloadUnusedAssets();
	}

}

