using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CanvasGroupFade : MonoBehaviour
{
    [SerializeField] private Transform _transformTarget;

    private CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = GetComponentInParent<CanvasGroup>();
    }

    void Update()
    {
        _transformTarget.gameObject.SetActive(_canvasGroup.alpha >= 1.0f);
    }
}