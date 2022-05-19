using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmplifyBloom;
using BepInEx;
using HarmonyLib;
using LIV.SDK.Unity;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Valve.VR;

namespace StanleyVr;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
	private VrUi vrUi;
	private LIV.SDK.Unity.LIV liv;

	private void Awake()
	{
		Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

		SteamVR_Actions.PreInitialize();
		LoadXRModule();

		SteamVR.Initialize();
		SteamVR.settings.pauseGameWhenDashboardVisible = false;

		Debug.Log("####### Stanley Parable Ultra Deluxe version " + Application.version);
		Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

		// InputTracking.disablePositionalTracking = true;

		var shaderBundle = VrAssetManager.LoadBundle("liv-shaders");
		Debug.Log($"###### using bundl for LIV {shaderBundle}");
		SDKShaders.LoadFromAssetBundle(shaderBundle);

		if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null
		                                       && XRGeneralSettings.Instance.Manager.activeLoader != null)
		{
			XRGeneralSettings.Instance.Manager.StartSubsystems();
		}
		else
			throw new Exception("Cannot initialize VRSubsystem");

		//Change tracking origin to headset
		var subsystems = new List<XRInputSubsystem>();
		SubsystemManager.GetInstances(subsystems);
		foreach (var subsystem in subsystems)
		{
			subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
			subsystem.TryRecenter();
		}
	}

	private void Update()
	{
		if (!vrUi)
		{
			vrUi = VrUi.Create();
		}

		if (Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			Camera.main.transform.parent.localScale *= 1.1f;
		}
		if (Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			Camera.main.transform.parent.localScale *= 0.9f;
		}

		if (Input.GetKeyDown(KeyCode.F9))
		{
			DisableAll<MobileBlur>();
			DisableAll<MobileBloom>();
			DisableAll<AmplifyBloomEffect>();
			DisableAll<PostEffectsCamera>();
		}

		if (Input.GetKeyDown(KeyCode.F1))
		{
			FindObjectOfType<MainMenu>().BeginTheGame();
		}

		if (Input.GetKeyDown(KeyCode.F4))
		{
			if (liv)
			{
				Debug.Log("#### LIV already exists, destroying");
				Destroy(liv.gameObject);
			}
			Debug.Log($"Attempting to create LIV with camera {Camera.main.name}");

			var livParent = new GameObject("LIVParent");
			livParent.transform.SetParent(Camera.main.transform.parent, false);
			livParent.transform.localPosition = Vector3.down;

			var livObject = new GameObject("LIV");
			livObject.gameObject.SetActive(false);
			livObject.transform.SetParent(livParent.transform, false);

			var cameraObject = new GameObject("LIVCamera");
			cameraObject.SetActive(false);
			cameraObject.AddComponent<Camera>();

			liv = livObject.AddComponent<LIV.SDK.Unity.LIV>();
			liv.stage = livParent.transform;
			liv.HMDCamera = Camera.main;
			liv.fixPostEffectsAlpha = true;
			liv.spectatorLayerMask = Camera.main.cullingMask;
			liv.excludeBehaviours = liv.excludeBehaviours.Concat(new[]
			{
				"MainCamera",
				"AudioListener",
				"FlareLayer",
				"CanvasRenderer",
				"EnableDepthOnHighQuality",
				"StereoPortalRenderer",
				"TrackedPoseDriver"
			}).ToArray();

			livObject.SetActive(true);

			Debug.Log("Successfully created LIV");
		}
	}

	private void DisableAll<TBehaviour>() where TBehaviour : MonoBehaviour
	{
		var components = FindObjectsOfType<TBehaviour>();
		foreach (var component in components)
		{
			component.enabled = false;
		}
	}

	private static void LoadXRModule()
	{
		var xrManagerBundle = VrAssetManager.LoadBundle("xrmanager");

		foreach (var xrManager in xrManagerBundle.LoadAllAssets())
			Debug.Log($"######## Loaded xrManager: {xrManager.name}");

		var instance = XRGeneralSettings.Instance;
		if (instance == null) throw new Exception("XRGeneralSettings instance is null");

		var xrManagerSettings = instance.Manager;
		if (xrManagerSettings == null) throw new Exception("XRManagerSettings instance is null");

		xrManagerSettings.InitializeLoaderSync();
		if (xrManagerSettings.activeLoader == null) throw new Exception("Cannot initialize OpenVR Loader");

		var openVrSettings = OpenVRSettings.GetSettings(false);
		openVrSettings.EditorAppKey = "steam.app.753640";
		openVrSettings.InitializationType = OpenVRSettings.InitializationTypes.Scene;
		if (openVrSettings == null) throw new Exception("OpenVRSettings instance is null");

		openVrSettings.SetMirrorViewMode(OpenVRSettings.MirrorViewModes.Right);
	}
}