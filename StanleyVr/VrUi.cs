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
        }
    }
}