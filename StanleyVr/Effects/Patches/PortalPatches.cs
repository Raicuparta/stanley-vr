using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;

namespace StanleyVr.Effects.Patches;

[HarmonyPatch]
public static class PortalPatches
{
	private static readonly int extraTex = Shader.PropertyToID("_ExtraTex");
	private static readonly int mainTex = Shader.PropertyToID("_MainTex");
	private static readonly int staticTex = Shader.PropertyToID("_StaticTex");

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MainCamera), nameof(MainCamera.OnPreCull))]
	private static bool DisablePortalRendering()
	{
		// Portal rendering is now done by StereoPortalRenderer, so no need to render it here too.
		return false;
	}
	
	// Copied from EasyPortal, just replaced Screen.width/height XRSettings.eyeTextureWidth/Height
	[HarmonyPrefix]
	[HarmonyPatch(typeof(EasyPortal), nameof(EasyPortal.CreateStaticTexture))]
	private static bool CreateStaticTexture(EasyPortal __instance)
	{
		if (__instance.staticViewTexture != null &&
		    __instance.staticViewTexture.width == XRSettings.eyeTextureWidth / 2 &&
		    __instance.staticViewTexture.height == XRSettings.eyeTextureHeight / 2) return false;

		if (__instance.staticViewTexture != null)
		{
			__instance.staticViewTexture.Release();
		}

		__instance.staticViewTexture = new RenderTexture(XRSettings.eyeTextureWidth / 2, XRSettings.eyeTextureHeight / 2, 32, RenderTextureFormat.Default);
		__instance.screen.material.SetTexture(staticTex, __instance.staticViewTexture);

		return false;
	}

	// Copied from EasyPortal, just replaced Screen.width/height XRSettings.eyeTextureWidth/Height
	[HarmonyPrefix]
	[HarmonyPatch(typeof(EasyPortal), nameof(EasyPortal.CreateViewTexture))]
	private static bool CreateViewTexture(EasyPortal __instance)
	{
		if (__instance.viewTexture != null && __instance.viewTexture.width == XRSettings.eyeTextureWidth &&
		    __instance.viewTexture.height == XRSettings.eyeTextureHeight) return false;

		if (__instance.viewTexture != null)
		{
			__instance.viewTexture.Release();
		}

		__instance.viewTexture = new RenderTexture(XRSettings.eyeTextureWidth, XRSettings.eyeTextureHeight, 32, RenderTextureFormat.Default);
		__instance.viewTexture.antiAliasing = 2;
		__instance.portalCam.targetTexture = __instance.viewTexture;
		__instance.screen.material.SetTexture(mainTex, __instance.viewTexture);

		return false;
	}

	// Copied from EasyPortal, just replaced Screen.width/height XRSettings.eyeTextureWidth/Height
	[HarmonyPrefix]
	[HarmonyPatch(typeof(EasyPortal), nameof(EasyPortal.CreateExtraTexture))]
	private static bool CreateExtraTexture(EasyPortal __instance)
	{
		if (__instance.extraTexture != null && __instance.extraTexture.width == XRSettings.eyeTextureWidth &&
		    __instance.extraTexture.height == XRSettings.eyeTextureHeight) return false;

		if (__instance.extraTexture != null)
		{
			__instance.extraTexture.Release();
		}

		__instance.extraTexture = new RenderTexture(XRSettings.eyeTextureWidth, XRSettings.eyeTextureHeight, 32, RenderTextureFormat.Default);
		__instance.extraCam.targetTexture = __instance.extraTexture;
		__instance.screen.material.SetTexture(extraTex, __instance.extraTexture);

		return false;
	}

	// Copied from EasyPortal, just replaced Screen.width/height XRSettings.eyeTextureWidth/Height
	[HarmonyPrefix]
	[HarmonyPatch(typeof(EasyPortal), nameof(EasyPortal.CreateRecursiveTexture))]
	private static bool CreateRecursiveTexture(EasyPortal __instance)
	{
		if (__instance.recursiveTexture != null && __instance.recursiveTexture.width == XRSettings.eyeTextureWidth &&
		    __instance.recursiveTexture.height == XRSettings.eyeTextureHeight) return false;

		if (__instance.recursiveTexture != null)
		{
			__instance.recursiveTexture.Release();
		}

		__instance.recursiveTexture = new RenderTexture(XRSettings.eyeTextureWidth, XRSettings.eyeTextureHeight, 32, RenderTextureFormat.Default);

		return false;
	}
}