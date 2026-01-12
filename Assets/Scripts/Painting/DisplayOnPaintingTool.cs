using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XDPaint.Controllers;
using XDPaint.Core;

[Serializable]
public class PaintToolDisplay
{
    public PaintTool PaintToolType;
    public List<Transform> DisplayedTranforms = new List<Transform>();
}

public class DisplayOnPaintingTool : MonoBehaviour
{
    [SerializeField] private List<PaintToolDisplay> PaintToolDisplays = new List<PaintToolDisplay>();
    [SerializeField] private LayoutGroup ParentLayout;
    [SerializeField] private RectTransform workArea;
    [SerializeField] private RectTransform scrollAllowedArea;
    public RectTransform WorkArea => workArea;
    public RectTransform ScrollAllowedArea => scrollAllowedArea;

    void Start()
    {
        PaintController.Instance.ToolsManager.OnToolUpdated += HandleToolUpdated;
    }

    void OnDestroy()
    {
        PaintController.Instance.ToolsManager.OnToolUpdated -= HandleToolUpdated;
    }

    void HandleToolUpdated()
    {
        foreach(PaintToolDisplay display in PaintToolDisplays)
        {
            if(display.PaintToolType == PaintController.Instance.ToolsManager.CurrentTool.Type)
            {
                foreach(Transform child in this.transform.GetComponentsInChildren<Transform>(true))
                {
                    if(child.parent == this.transform)
                    {
                        child.gameObject.SetActive(display.DisplayedTranforms.Contains(child));
                    }
                }
            }
        }

        Rebuild();
    }

    public void Rebuild()
    {
        if((gameObject != null) && (gameObject.activeInHierarchy == true) && ParentLayout != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(ParentLayout.transform as RectTransform);
        }
    }
}
