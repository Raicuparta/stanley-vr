using HarmonyLib;
using StanleyVr.VrCamera;
using StanleyVr.VrUi;
using UnityEngine;
using UnityEngine.Video;

namespace StanleyVr.Video.Patches;

[HarmonyPatch]
public static class VideoPatches
{
    [HarmonyPostfix]
	[HarmonyPatch(typeof(VideoPlayer), nameof(VideoPlayer.Play))]
	private static void FixVideoPlayer(VideoPlayer __instance)
	{

		// The camera that comes with the video player acts weird so I'm just using the UI camera instead.
		// For some reason everything is broken if I try to show video in the UI Camera.
		// So I'm using the Bucket camera for now.
		__instance.targetCamera = VrCameraController.GetBucketCamera();
		
		// Bucket camera has no clear flags by default, so we need to change that temporarily.
		__instance.targetCamera.clearFlags = CameraClearFlags.Color;
		__instance.targetCamera.backgroundColor = Color.black;
		
		// We're using the Bucket camera as a UI camera.
		__instance.gameObject.layer = LayerMask.NameToLayer("Bucket"); // TODO organize layers.

		// The default video player camera gets enabled via script so I'm forcing it to get disabled.
		var forceDisable = __instance.gameObject.AddComponent<ForceDisableBehaviour>();
		forceDisable.behaviour = __instance.GetComponent<Camera>();
	}
	
	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameMaster.MoviePlaybackContext), nameof(GameMaster.MoviePlaybackContext.StopMovie))]
	private static void ResetUiCameraClearFlags()
	{
		VrCameraController.GetBucketCamera().clearFlags = CameraClearFlags.Nothing;
	}
}