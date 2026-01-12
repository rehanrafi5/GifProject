using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace XDPaint.Demo.UI
{
	public class ToggleDoubleClick : MonoBehaviour, IPointerDownHandler
	{
		[Serializable]
		public class OnDoubleClickEvent : UnityEvent<float>
		{
		}
		
		public Toggle Toggle;
		public OnDoubleClickEvent OnDoubleClick = new OnDoubleClickEvent();
		public float TimeBetweenTaps = 0.5f;
		
		private float _firstTapTime;
		private bool _doubleTapInitialized;
		
		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			if (Time.time - _firstTapTime >= TimeBetweenTaps)
			{
				_doubleTapInitialized = false;
			}
			else if (_doubleTapInitialized)
			{
				OnDoubleClick.Invoke(transform.position.x);
				_doubleTapInitialized = false;
			}

			if (!_doubleTapInitialized)
			{
				_doubleTapInitialized = true;
				_firstTapTime = Time.time;
			}
		}
	}
}