using AmplifyBloom;
using HarmonyLib;
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
}