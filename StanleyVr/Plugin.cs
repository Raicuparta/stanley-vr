using System;
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
            
            Invoke(nameof(StartGame), 1);
        }

        private void StartGame()
        {
            SceneManager.LoadScene("map1_UD_MASTER");
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