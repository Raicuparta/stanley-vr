using System;
using System.Collections.Generic;
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
		SteamVR.settings.pauseGameWhenDashboardVisible = false;

		foreach (var actionSet in SteamVR_Input.actionSets)
		{
			if (actionSet != SteamVR_Actions.menu) actionSet.Activate();
			else
			{
				actionSet.Deactivate();
			}
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
    }

    private void SetUpXr()
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