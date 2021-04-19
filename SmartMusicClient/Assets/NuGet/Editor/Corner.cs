using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class Corner : MonoBehaviour
{
    [MenuItem("UGUI/Anchors to Corners %1")]
    static void AnchorsToCorners()
    {
        RectTransform rect = Selection.activeTransform as RectTransform;
        RectTransform pt = Selection.activeTransform.parent as RectTransform;


        if (rect == null || pt == null) return;

        Vector2 newAnchorsMin = new Vector2(rect.anchorMin.x + rect.offsetMin.x / pt.rect.width,
                                            rect.anchorMin.y + rect.offsetMin.y / pt.rect.height);
        Vector2 newAnchorsMax = new Vector2(rect.anchorMax.x + rect.offsetMax.x / pt.rect.width,
                                            rect.anchorMax.y + rect.offsetMax.y / pt.rect.height);

        rect.anchorMin = newAnchorsMin;
        rect.anchorMax = newAnchorsMax;
        rect.offsetMin = rect.offsetMax = new Vector2(0, 0);
    }

    [MenuItem("UGUI/Corners to Anchors %2")]
    static void CornersToAnchors()
    {
        RectTransform rect = Selection.activeTransform as RectTransform;

        if (rect == null) return;

        rect.offsetMin = rect.offsetMax = new Vector2(0, 0);
    }
}
