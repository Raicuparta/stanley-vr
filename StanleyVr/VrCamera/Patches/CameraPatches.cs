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

		var camera = __instance.GetComponent<Camera>();
		camera.transform.parent.localScale = Vector3.one * 0.5f;
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

		if (__instance.GetComponent<VrCameraController>()) return;

		__instance.gameObject.AddComponent<VrCameraController>();
	}
	
	// TODO clean this up.
	// This is a copy paste of the entire StanleyController.Update method,
	// with just small modifications to stop trying to change the FOV
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StanleyController), nameof(StanleyController.Update))]
	private static bool PreventChangingFov(StanleyController __instance)
	{
		if (!Singleton<GameMaster>.Instance.IsLoading && GameMaster.ONMAINMENUORSETTINGS)
		{
			AudioListener.volume = Singleton<GameMaster>.Instance.masterVolume;
		}
		StanleyController.StanleyPosition = __instance.transform.position;

		// Trying to change the FOV in VR causes warnings.
		// __instance.cam.fieldOfView = FieldOfViewBase + FieldOfViewAdditiveModifier;

		if (!__instance.viewFrozen)
		{
			__instance.View();
		}
		if (!__instance.motionFrozen)
		{
			__instance.Movement();
			__instance.UpdateCurrentlyStandingOn();
			__instance.Footsteps();
			__instance.ClickingOnThings();
		}
		else if (__instance.character.enabled)
		{
			__instance.character.Move(Vector2.zero);
		}
		if (!__instance.viewFrozen)
		{
			__instance.FloatCamera();
		}
		if (BucketController.HASBUCKET)
		{
			if (__instance.character.enabled && __instance.grounded)
			{
				__instance.Bucket.SetWalkingSpeed(__instance.character.velocity.magnitude / (__instance.walkingSpeed * __instance.WalkingSpeedMultiplier));
			}
			else
			{
				__instance.Bucket.SetWalkingSpeed(0f);
			}
		}
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SetFOVBasedOnAspectRatio), nameof(SetFOVBasedOnAspectRatio.LateUpdate))]
	private static bool PreventChangingFov(SetFOVBasedOnAspectRatio __instance)
	{
		Object.Destroy(__instance);
		return false;
	}
}