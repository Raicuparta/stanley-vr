using AmplifyBloom;
using HarmonyLib;
using StanleyVr.VrUi;
using UnityEngine;

namespace StanleyVr.Effects.Patches;

[HarmonyPatch]
public static class EffectsPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(AmplifyBloomBase), nameof(AmplifyBloomBase.Awake))]
	private static void DisableBloom(AmplifyBloomBase __instance)
	{
		// This bloom looks terrible in VR, so removing it.
		Object.Destroy(__instance);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MobileBlur), nameof(MobileBlur.OnRenderImage))]
	private static bool RemovePauseBlur(MobileBlur __instance)
	{
		__instance.enabled = false;
		return false;
	}
	
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Eyelids), nameof(Eyelids.Start))]
	private static void RenderEyelidsToUiCamera(Eyelids __instance)
	{
		var canvas = __instance.GetComponent<Canvas>();
		canvas.worldCamera = VrUiManager.Instance.UiCamera;
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		canvas.planeDistance = 1f;
	}
}