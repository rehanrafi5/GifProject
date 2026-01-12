using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Tools;

namespace XDPaint.AdditionalComponents
{
	public class AverageColorCalculator : MonoBehaviour
	{
		public PaintManager PaintManager;
		public event ColorHandler OnGetAverageColor;
		public delegate void ColorHandler(Color color);
		
		private Material _averageColorMaterial;
		private RenderTexture _percentRenderTexture;
		private RenderTargetIdentifier _rti;
		private CommandBuffer _commandBuffer;
		private Mesh _mesh;
		private int _accuracy = 64;

		private const string AccuracyShaderParam = "_Accuracy";

		#region MonoBehaviour Methods

		IEnumerator Start()
		{
			yield return null;
			Initialize();
		}

		void OnDestroy()
		{
			ReleaseRenderTexture();
			if (_mesh != null)
			{
				Destroy(_mesh);
			}
			if (_commandBuffer != null)
			{
				_commandBuffer.Release();
			}
			if (_averageColorMaterial != null)
			{
				Destroy(_averageColorMaterial);
				_averageColorMaterial = null;
			}
		}

		void Update()
		{
			if (OnGetAverageColor != null && PaintManager.PaintObject.IsPainted)
			{
				UpdateAverageColor();
			}
		}

		#endregion

		private void Initialize()
		{
			if (_averageColorMaterial == null)
			{
				_averageColorMaterial = new Material(Settings.Instance.AverageColorShader);
				SetAccuracy(_accuracy);
			}
			_averageColorMaterial.mainTexture = PaintManager.GetResultRenderTexture();
			_commandBuffer = new CommandBuffer {name = "AverageColor"};
			_percentRenderTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32);
			_rti = new RenderTargetIdentifier(_percentRenderTexture);
			_mesh = MeshGenerator.GenerateQuad(Vector3.one, Vector3.zero);
		}

		/// <summary>
		/// Calculates average color
		/// </summary>
		private void CalcAverageColor()
		{
			var prevRenderTextureT = RenderTexture.active;
			RenderTexture.active = _percentRenderTexture;
			var averageColorTexture = new Texture2D(_percentRenderTexture.width, _percentRenderTexture.height, TextureFormat.ARGB32, false, true);
			averageColorTexture.ReadPixels(new Rect(0, 0, _percentRenderTexture.width, _percentRenderTexture.height), 0, 0);
			averageColorTexture.Apply();
			RenderTexture.active = prevRenderTextureT;
			var averageColor = averageColorTexture.GetPixel(0, 0);
			OnGetAverageColor(averageColor);
		}

		/// <summary>
		/// Releases RenderTexture
		/// </summary>
		private void ReleaseRenderTexture()
		{
			if (_percentRenderTexture != null && _percentRenderTexture.IsCreated())
			{
				_percentRenderTexture.Release();
				Destroy(_percentRenderTexture);
			}
		}

		private void UpdateAverageColor()
		{
			GL.LoadOrtho();
			_commandBuffer.Clear();
			_commandBuffer.SetRenderTarget(_rti);
			_commandBuffer.ClearRenderTarget(false, true, Constants.ClearBlack);
			_commandBuffer.DrawMesh(_mesh, Matrix4x4.identity, _averageColorMaterial);
			Graphics.ExecuteCommandBuffer(_commandBuffer);
			CalcAverageColor();
		}
		
		public void SetAccuracy(int accuracy)
		{
			_accuracy = accuracy;
			if (_averageColorMaterial != null)
			{
				_averageColorMaterial.SetInt(AccuracyShaderParam, _accuracy);
			}
		}
	}
}