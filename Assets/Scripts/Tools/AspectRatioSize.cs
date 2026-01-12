using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class AspectRatioSize : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private float _defaultSize = 1024f;
    [SerializeField] private float _multiplier = 180f;

    void Start()
    {
        float ratio = (float)Screen.width / (float)Screen.height;
        _rectTransform.sizeDelta = new Vector2(_defaultSize + ratio * _multiplier, _rectTransform.sizeDelta.y);
    }
}
