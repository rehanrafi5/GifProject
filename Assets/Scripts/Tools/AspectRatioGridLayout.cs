using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class AspectRatioGridLayout : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private float _threshold = 1.7f;
    [SerializeField] private int _smallSize = 2;
    [SerializeField] private int _largeSize = 4;

    void Start()
    {
        float ratio = (float)Screen.width / (float)Screen.height;

        if(ratio < _threshold)
        {
            _gridLayout.constraintCount = _largeSize;
        }

        else
        {
            _gridLayout.constraintCount = _smallSize;
        }
    }
}
