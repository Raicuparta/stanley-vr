using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace StanleyVr.VrUi.Patches;

[HarmonyPatch]
public static class UiPatches
{
    private static readonly string[] canvasesToIgnore =
	{
		"com.sinai.unityexplorer_Root", // UnityExplorer.
		"com.sinai.unityexplorer.MouseInspector_Root", // UnityExplorer.
	};

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
	private static void MoveCanvasesToWorldSpace(CanvasScaler __instance)
	{
		var canvas = __instance.GetComponent<Canvas>();

		if (!canvas || canvas.renderMode == RenderMode.WorldSpace || canvasesToIgnore.Contains(canvas.name))
		{
			return;
		};
	
		Debug.Log($"Found CanvasScaler {__instance.name}");

		var parent = canvas.transform.parent;
		var aspectRationScaler = parent ? parent.GetComponent<AspectRatioScaler>() : null;
		if (aspectRationScaler)
		{
			aspectRationScaler.enabled = false;
			aspectRationScaler.transform.localScale = Vector3.one * 0.2f;
		}
		else
		{
			VrUiController.Create(canvas);
		}
		
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CanvasOrdering), nameof(CanvasOrdering.Update))]
	[HarmonyPatch(typeof(SetEventCameraOnStart), nameof(SetEventCameraOnStart.Start))]
	private static bool DisableCanvasOrdering()
	{
		// This behaviour changes the canvas plane distance and world camera, so I'm disabling it.
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Start))]
	private static void FixMainMenuCanvas(MainMenu __instance)
	{
		// The MainMenu behaviour changes the canvas rendermode to overlay, so I need ot change it to camera here too.
		__instance.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
	}
}