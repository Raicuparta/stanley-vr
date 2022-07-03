﻿using UnityEngine;

namespace StanleyVr.VrUi;

public class VrUiManager: MonoBehaviour
{
    public Camera UiCamera { get; private set; }
    public static VrUiManager Instance { get; private set; }
    
    public static void Create(GameMaster gameMaster)
    {
        if (Instance) Destroy(Instance.gameObject);
        Instance = new GameObject("VrUiManager").AddComponent<VrUiManager>();
        Instance.transform.SetParent(gameMaster.transform, false);
    }

    private void Awake()
    {
        UiCamera = gameObject.AddComponent<Camera>();
        UiCamera.cullingMask = LayerMask.GetMask("UI");
        UiCamera.clearFlags = CameraClearFlags.Nothing;
        
        // High depth to make this camera draw on top of the others.
        UiCamera.depth = 1000;
    }
}