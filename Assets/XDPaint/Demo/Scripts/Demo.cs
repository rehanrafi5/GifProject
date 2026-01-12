using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Demo.UI;
using XDPaint.Tools.Image;

namespace XDPaint.Demo
{
	public class Demo : MonoBehaviour
	{
		[Serializable]
		public class PaintManagersData
		{
			public PaintManager PaintManager;
			public string Text;
			public bool RequiresOrthographicCamera;
		}
		
		[Serializable]
		public class PaintItem
		{
			public Image Image;
			public Button Button;
		}
		
		public PaintManagersData[] PaintManagers;
		public CameraMover CameraMover;

		public GameObject TutorialObject;
		public EventTrigger Tutorial;
		
		//top panel
		public Button TutorialButton;
		public Toggle BrushTool;
		public ToggleDoubleClick BrushToolDoubleClick;
		public Toggle EraseTool;
		public ToggleDoubleClick EraseToolDoubleClick;
		public Toggle EyedropperTool;
		public Toggle BrushSamplerTool;
		public Toggle CloneTool;
		public Toggle BlurTool;
		public Toggle GaussianBlurTool;
		public ToggleDoubleClick BlurToolDoubleClick;
		public ToggleDoubleClick GaussianBlurToolDoubleClick;
		public Toggle RotateToggle;
		public Toggle PlayPauseToggle;
		public RawImage BrushPreview;
		public RectTransform BrushPreviewTransform;
		public Button BrushButton;
		public EventTrigger TopPanel;
		public EventTrigger ColorPalette;
		public EventTrigger BrushesPanel;
		public EventTrigger BlurPanel;
		public EventTrigger GaussianBlurPanel;
		public Slider BlurSlider;
		public Slider GaussianBlurSlider;
		public PaintItem[] Colors;
		public PaintItem[] Brushes;

		//right panel
		public Slider OpacitySlider;
		public Slider BrushSizeSlider;
		public Slider HardnessSlider;
		public Button UndoButton;
		public Button RedoButton;
		public EventTrigger RightPanel;

		//bottom panel
		public Button NextButton;
		public Button PreviousButton;
		public Text BottomPanelText;
		public EventTrigger BottomPanel;

		public EventTrigger AllArea;
		
		private PaintManager PaintManager
		{
			get { return PaintManagers[_currentPaintManagerId].PaintManager; }
		}

		private Camera _camera;
		private Animator _animator;
		private PaintTool _previousTool;
		private int _currentPaintManagerId;
		private const int TutorialShowCount = 3;
		
		void Awake()
		{
#if !UNITY_WEBGL
			Application.targetFrameRate = 60;
#endif
			_camera = Camera.main;

			InitPaintManagers();
			UpdateButtons();

			foreach (var paintManager in PaintManagers)
			{
				paintManager.PaintManager.gameObject.SetActive(false);
			}
			
			//tutorial
			var tutorialShowsCount = PlayerPrefs.GetInt("XDPaintDemoTutorialShowsCount", 0);
			var tutorialClick = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
			tutorialClick.callback.AddListener(delegate
			{
				PlayerPrefs.SetInt("XDPaintDemoTutorialShowsCount", tutorialShowsCount + 1);
				OnTutorial(false);
			});				
			Tutorial.triggers.Add(tutorialClick);
			if (tutorialShowsCount < TutorialShowCount)
			{
				TutorialObject.gameObject.SetActive(true);
				InputController.Instance.enabled = false;
			}
			else
			{
				OnTutorial(false);
			}
			
			var hoverEnter = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
			hoverEnter.callback.AddListener(HoverEnter);
			var hoverExit = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
			hoverExit.callback.AddListener(HoverExit);
			
			//top panel
			TutorialButton.onClick.AddListener(delegate { OnTutorial(true); });
			BrushTool.onValueChanged.AddListener(SetBrushTool);
			BrushToolDoubleClick.OnDoubleClick.AddListener(OpenBrushPanel);
			EraseTool.onValueChanged.AddListener(SetEraseTool);
			EraseToolDoubleClick.OnDoubleClick.AddListener(OpenBrushPanel);
			EyedropperTool.onValueChanged.AddListener(SetEyedropperTool);
			BrushSamplerTool.onValueChanged.AddListener(SetBrushSamplerTool);
			CloneTool.onValueChanged.AddListener(SetCloneTool);
			BlurTool.onValueChanged.AddListener(SetBlurTool);
			BlurToolDoubleClick.OnDoubleClick.AddListener(OpenBlurPanel);
			GaussianBlurTool.onValueChanged.AddListener(SetGaussianBlurTool);
			GaussianBlurToolDoubleClick.OnDoubleClick.AddListener(OpenGaussianBlurPanel);
			RotateToggle.onValueChanged.AddListener(SetRotateMode);
			PlayPauseToggle.onValueChanged.AddListener(OnPlayPause);
			BrushButton.onClick.AddListener(OpenColorPalette);
			TopPanel.triggers.Add(hoverEnter);
			TopPanel.triggers.Add(hoverExit);
			ColorPalette.triggers.Add(hoverEnter);
			ColorPalette.triggers.Add(hoverExit);
			BrushesPanel.triggers.Add(hoverEnter);
			BrushesPanel.triggers.Add(hoverExit);
			BlurPanel.triggers.Add(hoverEnter);
			BlurPanel.triggers.Add(hoverExit);
			BlurSlider.onValueChanged.AddListener(OnBlurSlider);
			GaussianBlurPanel.triggers.Add(hoverEnter);
			GaussianBlurPanel.triggers.Add(hoverExit);
			GaussianBlurSlider.onValueChanged.AddListener(OnGaussianBlurSlider);
			
			BrushSizeSlider.value = PaintController.Instance.Brush.Size;
			HardnessSlider.value = PaintController.Instance.Brush.Hardness;
			OpacitySlider.value = PaintController.Instance.Brush.Color.a;

			//right panel
			OpacitySlider.onValueChanged.AddListener(OnOpacitySlider);
			BrushSizeSlider.onValueChanged.AddListener(OnBrushSizeSlider);
			HardnessSlider.onValueChanged.AddListener(OnHardnessSlider);
			UndoButton.onClick.AddListener(OnUndo);
			RedoButton.onClick.AddListener(OnRedo);
			RightPanel.triggers.Add(hoverEnter);
			RightPanel.triggers.Add(hoverExit);
			
			//bottom panel
			NextButton.onClick.AddListener(delegate { SwitchPaintManager(true); });
			PreviousButton.onClick.AddListener(delegate { SwitchPaintManager(false); });
			BottomPanel.triggers.Add(hoverEnter);
			BottomPanel.triggers.Add(hoverExit);	
			
			var onDown = new EventTrigger.Entry {eventID = EventTriggerType.PointerDown};
			onDown.callback.AddListener(ResetPlates);
			AllArea.triggers.Add(onDown);

			//colors
			foreach (var colorItem in Colors)
			{
				colorItem.Button.onClick.AddListener(delegate { ColorClick(colorItem.Image.color); });
			}
			
			//brushes
			foreach (var brushItem in Brushes)
			{
				brushItem.Button.onClick.AddListener(delegate { BrushClick(brushItem.Image.mainTexture); });
			}

			LoadPrefs();
		}

		IEnumerator Start()
		{
			while (PaintController.Instance.Brush.RenderTexture == null)
			{
				yield return null;
			}
			BrushPreview.texture = PaintController.Instance.Brush.RenderTexture;
		}

		void Update()
		{
			if (PaintManager.Initialized)
			{
				UndoButton.interactable = PaintManager.PaintObject.TextureKeeper.CanUndo();
				RedoButton.interactable = PaintManager.PaintObject.TextureKeeper.CanRedo();
			}
		}

		private void LoadPrefs()
		{
			//opacity
			OpacitySlider.value = PlayerPrefs.GetFloat("XDPaintDemoBrushOpacity", 1f);
			//size
			BrushSizeSlider.value = PlayerPrefs.GetFloat("XDPaintDemoBrushSize", 0.4f);
			//hardness
			HardnessSlider.value = PlayerPrefs.GetFloat("XDPaintDemoBrushHardness", 0.99f);
			//color
			Color color;
			ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("XDPaintDemoBrushColor", "#FFFFFF"), out color);
			ColorClick(color);
		}

		private void OnTutorial(bool show)
		{
			TutorialObject.gameObject.SetActive(show);
			PaintManagers[_currentPaintManagerId].PaintManager.gameObject.SetActive(!show);
			InputController.Instance.enabled = !show;
		}

		private void InitPaintManagers()
		{
			for (var i = 0; i < PaintManagers.Length; i++)
			{
				PaintManagers[i].PaintManager.gameObject.SetActive(i == _currentPaintManagerId);
				if (_animator == null)
				{
					var skinnedMeshRenderer = PaintManagers[i].PaintManager.ObjectForPainting.GetComponent<SkinnedMeshRenderer>();
					if (skinnedMeshRenderer != null)
					{
						var animator = PaintManagers[i].PaintManager.GetComponentInChildren<Animator>(true);
						if (animator != null)
						{
							_animator = animator;
						}
					}
				}
			}
		}

		private void SetBrushTool(bool isOn)
		{
			if (isOn)
			{
				PaintController.Instance.Tool = PaintTool.Brush;
			}
		}

		private void SetEraseTool(bool isOn)
		{
			if (isOn)
			{
				PaintController.Instance.Tool = PaintTool.Erase;
			}
		}

		private void SetEyedropperTool(bool isOn)
		{
			if (isOn)
			{
				PaintController.Instance.Tool = PaintTool.Eyedropper;
			}
		}

		private void SetBrushSamplerTool(bool isOn)
		{
			if (isOn)
			{
				PaintController.Instance.Tool = PaintTool.BrushSampler;
			}
		}
		
		private void SetCloneTool(bool isOn)
		{
			if (isOn)
			{
				PaintController.Instance.Tool = PaintTool.Clone;
			}
		}

		private void SetBlurTool(bool isOn)
		{
			if (isOn)
			{
				PaintController.Instance.Tool = PaintTool.Blur;
			}
		}
		
		private void SetGaussianBlurTool(bool isOn)
		{
			if (isOn)
			{
				PaintController.Instance.Tool = PaintTool.BlurGaussian;
			}
		}
		
		private void OpenColorPalette()
		{
			BrushesPanel.gameObject.SetActive(false);
			BlurPanel.gameObject.SetActive(false);
			GaussianBlurPanel.gameObject.SetActive(false);
			ColorPalette.gameObject.SetActive(!ColorPalette.gameObject.activeInHierarchy);
		}

		private void OpenBrushPanel(float xPosition)
		{
			ColorPalette.gameObject.SetActive(false);
			BlurPanel.gameObject.SetActive(false);
			GaussianBlurPanel.gameObject.SetActive(false);
			BrushesPanel.gameObject.SetActive(true);
			var brushesPanelTransform = BrushesPanel.transform;
			brushesPanelTransform.position = new Vector3(xPosition, brushesPanelTransform.position.y, brushesPanelTransform.position.z);
		}
		
		private void OpenBlurPanel(float xPosition)
		{
			ColorPalette.gameObject.SetActive(false);
			BrushesPanel.gameObject.SetActive(false);
			GaussianBlurPanel.gameObject.SetActive(false);
			BlurPanel.gameObject.SetActive(true);
			var blurPanelTransform = BlurPanel.transform;
			blurPanelTransform.position = new Vector3(xPosition, blurPanelTransform.position.y, blurPanelTransform.position.z);
		}
		
		private void OpenGaussianBlurPanel(float xPosition)
		{
			ColorPalette.gameObject.SetActive(false);
			BrushesPanel.gameObject.SetActive(false);
			BlurPanel.gameObject.SetActive(false);
			GaussianBlurPanel.gameObject.SetActive(true);
			var blurPanelTransform = GaussianBlurPanel.transform;
			blurPanelTransform.position = new Vector3(xPosition, blurPanelTransform.position.y, blurPanelTransform.position.z);
		}

		private void SetRotateMode(bool isOn)
		{
			CameraMover.enabled = isOn;
			if (isOn)
			{
				PaintManager.PaintObject.FinishPainting();
			}
			InputController.Instance.enabled = !isOn;
		}

		private void OnPlayPause(bool isOn)
		{
			if (_animator != null)
			{
				_animator.enabled = !isOn;
			}
		}

		private void OnOpacitySlider(float value)
		{
			var color = PaintController.Instance.Brush.Color;
			color.a = value;
			PaintController.Instance.Brush.SetColor(color);
			PlayerPrefs.SetFloat("XDPaintDemoBrushOpacity", value);
		}
		
		private void OnBrushSizeSlider(float value)
		{
			PaintController.Instance.Brush.Size = value;
			BrushPreviewTransform.localScale = Vector3.one * value;
			PlayerPrefs.SetFloat("XDPaintDemoBrushSize", value);
		}

		private void OnHardnessSlider(float value)
		{
			PaintController.Instance.Brush.Hardness = value;
			PlayerPrefs.SetFloat("XDPaintDemoBrushHardness", value);
		}
		
		private void OnBlurSlider(float value)
		{
			var blurTool = PaintController.Instance.ToolsManager.AllTools.First(x => x.Key == PaintTool.Blur).Value as BlurTool;
			if (blurTool != null)
			{
				if (value < 0.1f)
				{
					blurTool.Iterations = 1;
				}
				else if (value < 0.5f)
				{
					blurTool.Iterations = 2;
				}
				else
				{
					blurTool.Iterations = 3;
				}
				blurTool.BlurStrength = value;
			}
		}
		
		private void OnGaussianBlurSlider(float value)
		{
			var blurTool = PaintController.Instance.ToolsManager.AllTools.First(x => x.Key == PaintTool.BlurGaussian).Value as GaussianBlurTool;
			if (blurTool != null)
			{
				if (value < 0.1f)
				{
					blurTool.KernelSize = 3;
					blurTool.Spread = 1.0f;
				}
				else if (value < 0.5f)
				{
					blurTool.KernelSize = 4;
					blurTool.Spread = 2.0f;
				}
				else
				{
					blurTool.KernelSize = 5;
					blurTool.Spread = 3.0f;
				}
			}
		}
		
		private void OnUndo()
		{
			if (PaintManager.PaintObject.TextureKeeper.CanUndo())
			{
				PaintManager.PaintObject.TextureKeeper.Undo();
			}
		}
		
		private void OnRedo()
		{
			if (PaintManager.PaintObject.TextureKeeper.CanRedo())
			{
				PaintManager.PaintObject.TextureKeeper.Redo();
			}
		}

		private void SwitchPaintManager(bool switchToNext)
		{
			PaintManager.gameObject.SetActive(false);
			if (switchToNext)
			{
				_currentPaintManagerId = (_currentPaintManagerId + 1) % PaintManagers.Length;
			}
			else
			{
				_currentPaintManagerId--;
				if (_currentPaintManagerId < 0)
				{
					_currentPaintManagerId = PaintManagers.Length - 1;
				}
			}
			_camera.orthographic = PaintManagers[_currentPaintManagerId].RequiresOrthographicCamera;
			BrushTool.isOn = true;
			PaintManager.gameObject.SetActive(true);
			PaintManager.Init();
			CameraMover.Reset();
			UpdateButtons();
			
			//clear PaintManager states
			PaintManager.PaintObject.TextureKeeper.Reset();
			PaintManager.PaintObject.ClearTexture();
			PaintManager.Render();
		}

		private void UpdateButtons()
		{
			_camera.orthographic = PaintManagers[_currentPaintManagerId].RequiresOrthographicCamera;
			RotateToggle.interactable = !PaintManagers[_currentPaintManagerId].RequiresOrthographicCamera;

			var skinnedMeshRenderer = PaintManager.ObjectForPainting.GetComponent<SkinnedMeshRenderer>();
			var hasSkinnedMeshRenderer = skinnedMeshRenderer != null;
			if (!hasSkinnedMeshRenderer)
			{
				PlayPauseToggle.isOn = false;
			}
			PlayPauseToggle.interactable = hasSkinnedMeshRenderer;
			if (_animator != null)
			{
				_animator.enabled = hasSkinnedMeshRenderer;
			}
			BottomPanelText.text = PaintManagers[_currentPaintManagerId].Text;
		}
		
		private void HoverEnter(BaseEventData data)
		{
			if (Input.mousePresent)
			{
				PaintManager.PaintObject.ProcessInput = false;
			}
			PaintManager.PaintObject.FinishPainting();
		}
		
		private void HoverExit(BaseEventData data)
		{
			if (Input.mousePresent)
			{
				PaintManager.PaintObject.ProcessInput = true;
			}
		}
		
		private void ColorClick(Color color)
		{
			var brushColor = PaintController.Instance.Brush.Color;
			brushColor = new Color(color.r, color.g, color.b, brushColor.a);
			PaintController.Instance.Brush.SetColor(brushColor);
			BrushTool.isOn = true;

			var colorString = ColorUtility.ToHtmlStringRGB(brushColor);
			PlayerPrefs.SetString("XDPaintDemoBrushColor", colorString);
		}

		private void BrushClick(Texture texture)
		{
			PaintController.Instance.Brush.SetTexture(texture, true, false);
			BrushesPanel.gameObject.SetActive(false);
		}

		private void ResetPlates(BaseEventData data)
		{
			if (ColorPalette.gameObject.activeInHierarchy || BrushesPanel.gameObject.activeInHierarchy || BlurPanel.gameObject.activeInHierarchy || GaussianBlurPanel.gameObject.activeInHierarchy)
			{
				ColorPalette.gameObject.SetActive(false);
				BrushesPanel.gameObject.SetActive(false);
				BlurPanel.gameObject.SetActive(false);
				GaussianBlurPanel.gameObject.SetActive(false);
			}
			HoverExit(null);
		}
	}
}