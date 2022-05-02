using UnityEngine;

namespace StanleyVr;

public class VrUi : MonoBehaviour
{
    private void Start()
    {
        var canvases = FindObjectsOfType<Canvas>();
        var uiCamera = GetComponentInChildren<Camera>();
        
        foreach (var canvas in canvases)
        {
            canvas.worldCamera = uiCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            if (canvas.gameObject.layer != LayerMask.NameToLayer("Default") ||
                canvas.gameObject.layer != LayerMask.NameToLayer("UI"))
            {
                Debug.LogWarning($"Warning: changing canvas layer from {LayerMask.LayerToName(canvas.gameObject.layer)} to UI");
            }
            
            canvas.gameObject.layer = LayerMask.NameToLayer("UI");
        }
    }
}