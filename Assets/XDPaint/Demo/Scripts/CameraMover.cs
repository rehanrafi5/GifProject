using UnityEngine;

namespace XDPaint.Demo
{
	public class CameraMover : MonoBehaviour
	{
		public Transform Target;
		public float Distance = 10.0f;
		public float AxisRatio = 0.02f;
		public int MinDistance = 3;
		public int MaxDistance = 10;

		private Transform _transform;
		private int _fingerId = -1;
		private float _x, _y;
		private float _defaultDistance;
		private readonly Vector2 _axisMoveSpeedMouse = new Vector2(170f, 70f);
		private readonly Vector2 _axisMoveSpeedTouch = new Vector2(17f, 7f);

		void Awake()
		{
			_defaultDistance = Distance;
			_transform = transform;
		}
		
		void Update()
		{
			Distance += Input.GetAxis("Mouse ScrollWheel") * Distance;
			Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);
			
			if (!Input.touchSupported || Input.mousePresent)
			{
				if (Input.GetMouseButton(0))
				{
					_x += Input.GetAxis("Mouse X") * _axisMoveSpeedMouse.x * AxisRatio;
					_y -= Input.GetAxis("Mouse Y") * _axisMoveSpeedMouse.y * AxisRatio;
				}
			}
			
			if (Input.touchSupported)
			{
				foreach (var touch in Input.touches)
				{
					if (touch.phase == TouchPhase.Began && _fingerId == -1)
					{
						_fingerId = touch.fingerId;
					}
					if (touch.fingerId == _fingerId)
					{
						if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
						{
							_x += Input.touches[_fingerId].deltaPosition.x * _axisMoveSpeedTouch.x * AxisRatio;
							_y -= Input.touches[_fingerId].deltaPosition.y * _axisMoveSpeedTouch.y * AxisRatio;
						}
						if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
						{
							_fingerId = -1;
						}
					}
				}
			}
			
			var rotation = Quaternion.Euler(_y, _x, 0);
			var position = rotation * new Vector3(0.0f, 0.0f, -Distance) + Target.position;
			_transform.position = position;
			_transform.rotation = rotation;
		}

		public void Reset()
		{
			Distance = _defaultDistance;
			_x = 0;
			_y = 0;
			Update();
		}
	}
}