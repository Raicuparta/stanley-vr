using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VrUi : MonoBehaviour
{
    [SerializeField]
    private Camera uiCamera;

    [SerializeField]
    private RenderTexture renderTexture;
    
    // Start is called before the first frame update
    void Start()
    {
        var canvases = FindObjectsOfType<Canvas>();

        foreach (var canvas in canvases)
        {
            canvas.worldCamera = uiCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
