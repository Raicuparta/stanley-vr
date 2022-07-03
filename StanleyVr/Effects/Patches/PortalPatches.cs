using HarmonyLib;

namespace StanleyVr.Effects.Patches;

[HarmonyPatch]
public static class PortalPatches
{
    [HarmonyPrefix]
	[HarmonyPatch(typeof(MainCamera), nameof(MainCamera.OnPreCull))]
	private static bool DisablePortalRendering()
	{
		// Portal rendering is now done by StereoPortalRenderer, so no need to render it here too.
		return false;
	}
}