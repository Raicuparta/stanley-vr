using HarmonyLib;
using UnityEngine;

namespace StanleyVr.VrPlayer.Patches;

[HarmonyPatch]
public class LaserPatches
{
	private static Transform camParent;
	
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StanleyController), nameof(StanleyController.ClickingOnThings))]
	private static void UseLaserForInteractionDirection(StanleyController __instance)
	{
		if (!VrPlayerController.Laser) return;
		
		camParent = __instance.camParent;
		__instance.camParent = VrPlayerController.Laser;
	}	
	
	[HarmonyPostfix]
	[HarmonyPatch(typeof(StanleyController), nameof(StanleyController.ClickingOnThings))]
	private static void ResetStanleyControllerCamParent(StanleyController __instance)
	{
		if (!camParent) return;
		
		__instance.camParent = camParent;
	}
}