using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmplifyBloom;
using BepInEx;
using HarmonyLib;
using InControl;
using LIV.SDK.Unity;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using Mouse = UnityEngine.InputSystem.Mouse;

namespace StanleyVr;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
	// private VrUi vrUi;
	private LIV.SDK.Unity.LIV liv;

	private void Awake()
	{
		Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

		VrAssetManager.Initialize();
		// LoadXRModule();

		Debug.Log("####### Stanley Parable Ultra Deluxe version " + Application.version);
		Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

		// InputTracking.disablePositionalTracking = true;

		var shaderBundle = VrAssetManager.LivShadersBundle;
		Debug.Log($"###### using bundl for LIV {shaderBundle}");
		SDKShaders.LoadFromAssetBundle(shaderBundle);

		gameObject.AddComponent<ModXrManager>();
		
		InputManager.OnSetup += () =>
		{
			var vrDevice = new VrInputDevice();
			InputManager.AttachDevice(vrDevice);
			InputManager.activeDevice = vrDevice;
		};
	}

	private void Update()
	{
		Cursor.lockState = CursorLockMode.None;
		

		
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
}