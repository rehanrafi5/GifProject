using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XDPaint.Demo.UI
{
	public class SliderClicker : MonoBehaviour, IPointerDownHandler
	{
		public Slider Slider;

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			Slider.OnDrag(eventData);
		}
	}
}