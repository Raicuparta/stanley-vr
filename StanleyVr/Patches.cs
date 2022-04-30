using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace StanleyVr;

[HarmonyPatch]
public static class Patches
{
    private static readonly string[] canvasesToIgnore =
    {
        "com.sinai.unityexplorer_Root", // UnityExplorer.
        "com.sinai.unityexplorer.MouseInspector_Root" // UnityExplorer.
    };
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
    private static void MoveCanvasesToWorldSpace(CanvasScaler __instance)
    {
        Debug.Log($"Found CanvasScaler {__instance.name}");
    
        var canvas = __instance.GetComponent<Canvas>();

        if (canvasesToIgnore.Contains(canvas.name)) return;
        
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            __instance.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        }
        else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            canvas.worldCamera = Camera.current;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            __instance.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainCamera), "Start")]
    private static void FixCameraScale(MainCamera __instance)
    {
        __instance.transform.parent.localScale = Vector3.one * 0.5f;
    }
}