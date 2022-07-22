using System;
using System.Collections.Generic;
using BepInEx;
using StanleyVr.VrInput.ActionInputs;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Valve.VR;

namespace StanleyVr;

public class ModXrManager : MonoBehaviour
{
	private void Awake()
    {
		SteamVR_Actions.PreInitialize();
		SetUpXr();
		SteamVR.Initialize();
		ActionInputDefinitions.Initialize();
		SteamVR.settings.pauseGameWhenDashboardVisible = true;

		foreach (var actionSet in SteamVR_Input.actionSets)
		{
			actionSet.Activate();
		}
		
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
		
        ApplicationManifestHelper.UpdateManifest(Paths.ManagedPath + @"\..\StreamingAssets\stanleyparable.vrmanifest",
	        "steam.app.1703340",
	        "https://steamcdn-a.akamaihd.net/steam/apps/1703340/header.jpg",
	        "The Stanley Parable VR",
	        "StanleyVR mod for Ultra Deluxe",
	        steamBuild: SteamManager.Initialized,
	        steamAppId: 1703340);
    }

    private static void SetUpXr()
    {
	    var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
        var managerSetings = ScriptableObject.CreateInstance<XRManagerSettings>();
        var openVrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();

        generalSettings.Manager = managerSetings;
        managerSetings.SetValue("m_RegisteredLoaders", new HashSet<XRLoader>() {openVrLoader});
        managerSetings.TrySetLoaders(new List<XRLoader> {openVrLoader});

        managerSetings.InitializeLoaderSync();
		if (managerSetings.activeLoader == null) throw new Exception("Cannot initialize OpenVR Loader");

		var openVrSettings = OpenVRSettings.GetSettings(true);
		if (openVrSettings == null) throw new Exception("OpenVRSettings instance is null");
		openVrSettings.EditorAppKey = "steam.app.753640";
		openVrSettings.InitializationType = OpenVRSettings.InitializationTypes.Scene;
		openVrSettings.SetMirrorViewMode(OpenVRSettings.MirrorViewModes.Right);
    }
}