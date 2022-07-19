using HarmonyLib;
using StanleyVr.Effects;
using UnityEngine;

namespace StanleyVr.VrCamera.Patches;

[HarmonyPatch]
public static class CameraPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MainCamera), nameof(MainCamera.Start))]
	private static void SetUpVrCameraMain(MainCamera __instance)
	{
		__instance.gameObject.AddComponent<StereoPortalRenderer>();
		__instance.gameObject.AddComponent<VrCameraController>();
	}
	
	[HarmonyPostfix]
	[HarmonyPatch(typeof(BackgroundCamera), nameof(BackgroundCamera.Awake))]
	private static void FixBackgroundCameraScale(BackgroundCamera __instance)
	{
		// BackgroundCamera is what renders the sky in the Epilogue.
		// So it looks much better if we resize it to make the skybox look huge.
		// I'm not sure if BackgorundCamera is used for anything else. Might look bad in other usages.
		__instance.transform.localScale = Vector3.one * 0.1f;

		// TODO is still broken since the sky camera won't rotate when the player rotates with the mouse / controller.
		__instance.backgroundCamera.gameObject.AddComponent<VrCameraController>();
		__instance.enabled = false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(AmplifyColorBase), nameof(AmplifyColorBase.Start))]
	private static void SetUpVrCameraGeneral(AmplifyColorBase __instance)
	{
		// TODO: I think this only gets skipped on the main camera because MainCamera.Start happens to run first.
		// Should make this more reliable.

		if (__instance.GetComponent<VrCameraController>() || __instance.GetComponent<ViewControl>()) return;

		__instance.gameObject.AddComponent<VrCameraController>();
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ViewControl), nameof(ViewControl.Awake))]
	private static void SetUpCutsceneCameras(ViewControl __instance)
	{
		__instance.transform.localScale = Vector3.one * 0.5f;
	}
	
	
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Camera), nameof(Camera.fieldOfView), MethodType.Setter)]
	private static bool PreventChangingFov(Camera __instance)
	{
		return !__instance.stereoEnabled;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SetFOVBasedOnAspectRatio), nameof(SetFOVBasedOnAspectRatio.LateUpdate))]
	private static bool PreventChangingFov(SetFOVBasedOnAspectRatio __instance)
	{
		Object.Destroy(__instance);
		return false;
	}
}