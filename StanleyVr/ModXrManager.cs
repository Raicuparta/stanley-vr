using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace StanleyVr;

public class ModXrManager : MonoBehaviour
{
    private static bool isVrEnabled;
    private static OpenXRLoaderBase openXrLoader;
    private bool isXrSetUp;

    private void Awake()
    {
        ToggleXr();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ToggleXr();
        }
    }

    private void ToggleXr()
    {
            if (!isXrSetUp) SetUpXr();

            if (!isVrEnabled)
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
                XRGeneralSettings.Instance.Manager.activeLoader.Initialize();
                XRGeneralSettings.Instance.Manager.activeLoader.Start();
            }
            else
            {
                XRGeneralSettings.Instance.Manager.activeLoader.Stop();
                XRGeneralSettings.Instance.Manager.activeLoader.Deinitialize();
            }

            isVrEnabled = !isVrEnabled;
    }

    private void SetUpXr()
    {
        isXrSetUp = true;

        var xrManagerBundle = VrAssetManager.LoadBundle("xrmanager");

        foreach (var xrManager in xrManagerBundle.LoadAllAssets())
            Debug.Log($"######## Loaded xrManager: {xrManager.name}");

        var inputSettings = xrManagerBundle.LoadAsset<InputSettings>("InputSystem.inputsettings");
        if (!inputSettings)
        {
            throw new Exception("Input Settings is null");
        }
        
        InputSystem.settings = inputSettings;
        InputSystem.RegisterLayout<OculusTouchControllerProfile.OculusTouchController>();
        InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>();
        InputSystem.AddDevice<OculusTouchControllerProfile.OculusTouchController>();
        InputSystem.FindControl()

        var instance = XRGeneralSettings.Instance;
        if (instance == null) throw new Exception("XRGeneralSettings instance is null");

        var xrManagerSettings = instance.Manager;
        if (xrManagerSettings == null) throw new Exception("XRManagerSettings instance is null");

        xrManagerSettings.InitializeLoaderSync();
        if (xrManagerSettings.activeLoader == null) throw new Exception("Cannot initialize OpenVR Loader");

        openXrLoader = xrManagerSettings.ActiveLoaderAs<OpenXRLoaderBase>();

        // Reference OpenXRSettings just to make this work.
        // TODO figure out how to do this properly.
        OpenXRSettings unused;
    }
}