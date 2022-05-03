using System;
using System.IO;
using System.Reflection;
using AmplifyBloom;
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
        private VrUi vrUi;
        
        private void Awake()
        {
            Debug.Log("####### Stanley Parable Ultra Deluxe version " + Application.version);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            InputTracking.disablePositionalTracking = true;
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
        }

        private void DisableAll<TBehaviour>() where TBehaviour: MonoBehaviour
        {
            var components = FindObjectsOfType<TBehaviour>();
            foreach (var component in components)
            {
                component.enabled = false;
            }
        }
    }
}