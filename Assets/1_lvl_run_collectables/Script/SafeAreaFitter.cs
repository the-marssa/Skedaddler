
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    public void FitToSafeArea()
    {
        var rectTransform = GetComponent<RectTransform>();
        var rootCanvas = GetComponentInParent<Canvas>();
        if (null == rootCanvas)
        {
            return;
        }

        var safeArea = Screen.safeArea;
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;
        var pixelRect = rootCanvas.pixelRect;
        anchorMin.x /= pixelRect.width;
        anchorMin.y /= pixelRect.height;
        anchorMax.x /= pixelRect.width;
        anchorMax.y /= pixelRect.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}
