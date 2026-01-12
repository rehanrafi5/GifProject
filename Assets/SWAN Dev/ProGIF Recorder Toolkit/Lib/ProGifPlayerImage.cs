using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class ProGifPlayerImage : ProGifPlayerComponent
{
	[HideInInspector] public Image destinationImage;						// The image for display sprites
	private List<Image> m_ExtraImages = new List<Image>();

	private Texture2D _displayTexture2D = null;
	private Sprite _displaySprite = null;

	void Awake()
	{
		if(destinationImage == null)
		{
			destinationImage = gameObject.GetComponent<Image>();
            displayType = DisplayType.Image;
        }
	}

	// Update gif frame for the Player (Update is called once per frame)
	void Update()
	{
		if(State == PlayerState.Playing && displayType == ProGifPlayerComponent.DisplayType.Image)
		{
            float time = ignoreTimeScale ? Time.unscaledTime : Time.time;
            float dt = Mathf.Min(time - nextFrameTime, interval); //float dt = time - nextFrameTime;
            if (dt >= 0f)
            {
                spriteIndex = (spriteIndex >= gifTextures.Count - 1) ? 0 : spriteIndex + 1;
                nextFrameTime = time + interval / playbackSpeed - dt;

                if (spriteIndex < gifTextures.Count)
                {
                    if (OnPlayingCallback != null) OnPlayingCallback(gifTextures[spriteIndex]);

                    _SetDisplay(spriteIndex);

                    if (m_ExtraImages != null && m_ExtraImages.Count > 0)
                    {
                        Sprite sp = null;
                        if (optimizeMemoryUsage)
                        {
                            sp = _displaySprite;
                        }
                        else
                        {
                            sp = gifTextures[spriteIndex].GetSprite();
                        }

                        for (int i = 0; i < m_ExtraImages.Count; i++)
                        {
                            if (m_ExtraImages[i] != null)
                            {
                                m_ExtraImages[i].sprite = sp;
                            }
                            else
                            {
                                m_ExtraImages.Remove(m_ExtraImages[i]);
                                m_ExtraImages.TrimExcess();
                            }
                        }
                    }
                }
            }
		}
	}

	public override void Play(RenderTexture[] gifFrames, int fps, bool isCustomRatio, int customWidth, int customHeight, bool optimizeMemoryUsage)
	{
		base.Play(gifFrames, fps, isCustomRatio, customWidth, customHeight, optimizeMemoryUsage);

		if(destinationImage == null) destinationImage = gameObject.GetComponent<UnityEngine.UI.Image>();
        displayType = DisplayType.Image;
        _SetDisplay(0);
	}

	protected override void _OnFrameReady(GifTexture gTex, bool isFirstFrame)
	{
        if (isFirstFrame)
        {
            displayType = DisplayType.Image;
            _SetDisplay(0);
        }
    }

	private void _SetDisplay(int frameIndex)
	{
		if(optimizeMemoryUsage)
		{
			_displaySprite = gifTextures[frameIndex].GetSprite_OptimizeMemoryUsage(ref _displayTexture2D);
		}

		if(destinationImage != null)
		{
			if(optimizeMemoryUsage)
			{
				destinationImage.sprite = _displaySprite;
			}
			else
			{
				destinationImage.sprite = gifTextures[frameIndex].GetSprite();
			}
		}
	}

	public override void Clear()
	{
		//Your Code before base.Clear():
		//		if(destinationImage != null && destinationImage.sprite != null && destinationImage.sprite.texture != null)
		//		{
		//			Texture2D.Destroy(destinationImage.sprite.texture);
		//		}

		if(optimizeMemoryUsage)
		{
			if(_displayTexture2D != null) 
			{
				Texture2D.Destroy(_displayTexture2D);
				_displayTexture2D = null;
			}

			_displaySprite = null;
		}

		base.Clear();
	}


	public void ChangeDestination(Image image)
	{
        if (destinationImage != null) destinationImage.sprite = null;
        destinationImage = image;
	}

	public void AddExtraDestination(Image image)
	{
		if(!m_ExtraImages.Contains(image))
		{
			m_ExtraImages.Add(image);
		}
	}

	public void RemoveFromExtraDestination(Image image)
	{
		if(m_ExtraImages.Contains(image))
		{
			m_ExtraImages.Remove(image);
			m_ExtraImages.TrimExcess();
		}
	}
}
