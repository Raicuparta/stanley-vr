﻿using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace StanleyVr
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            InputTracking.disablePositionalTracking = true;
            
            Debug.Log("####### Stanley Parable Ultra Deluxe version " + Application.version);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                Camera.main.transform.parent.localScale *= 1.1f;
            }
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                Camera.main.transform.parent.localScale *= 0.9f;
            }
        }
    }
}