using HarmonyLib;
using UnityEngine;

namespace StanleyVr.VrPlayer.Patches;

[HarmonyPatch]
public class LaserPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StanleyController), nameof(StanleyController.ClickingOnThings))]
	private static bool LaserInteraction(StanleyController __instance)
	{
		var laser = VrPlayerController.Laser;
		if (laser == null) return false;
		
		if (Singleton<GameMaster>.Instance.FullScreenMoviePlaying || !Singleton<GameMaster>.Instance.stanleyActions.UseAction.WasPressed)
		{
			return false;
		}
		RaycastHit hitInfo;
		// TODO just replace __instance.camparent during this method instead of copying everything.
		if (Physics.Raycast(laser.position, laser.forward, out hitInfo, __instance.armReach, __instance.clickLayers, QueryTriggerInteraction.Ignore))
		{
			var gameObject = hitInfo.collider.gameObject;
			var component = gameObject.GetComponent<HammerEntity>();
			if (component != null)
			{
				component.Use();
			}
			else
			{
				__instance.PlayKeyboardSound();
			}
			if (StanleyController.OnInteract != null)
			{
				StanleyController.OnInteract(gameObject);
			}
		}
		else
		{
			__instance.PlayKeyboardSound();
			if (StanleyController.OnInteract != null)
			{
				StanleyController.OnInteract(null);
			}
		}
	
		return false;
	}
}