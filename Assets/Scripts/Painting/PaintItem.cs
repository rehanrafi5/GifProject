using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaintItem : MonoBehaviour
{
    public Image Image;
    public Toggle Toggle;
    public Transform OutlineRoot;

    public void ShowOutline(bool show)
    {
        OutlineRoot.gameObject.SetActive(show);
    }
}
