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
        private const string assetsDir = "/BepInEx/plugins/StanleyVr/Assets/";
        private AssetBundle vrUiBundle;
        
        private void Awake()
        {
            Debug.Log("####### Stanley Parable Ultra Deluxe version " + Application.version);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            InputTracking.disablePositionalTracking = true;
            
            vrUiBundle = LoadBundle("vr-ui");
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

            if (Input.GetKeyDown(KeyCode.F8))
            {
                var vrUiPrefab = vrUiBundle.LoadAsset<GameObject>("VrUi");
                Debug.Log($"###### vrUiPrefab {vrUiPrefab}");
                var instance = Instantiate(vrUiPrefab, Camera.main ? Camera.main.transform : null);
                Debug.Log($"###### instance {instance}");
                var vrUi = instance.AddComponent<VrUi>();
                Debug.Log($"###### vrUi {vrUi}");
                vrUi.transform.localPosition = Vector3.forward;
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                DisableAll<MobileBlur>();
                DisableAll<MobileBloom>();
                DisableAll<AmplifyBloomEffect>();
                DisableAll<PostEffectsCamera>();
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
        
        private static AssetBundle LoadBundle(string assetName)
        {
            var bundle = AssetBundle.LoadFromFile(string.Format("{0}{1}{2}", Directory.GetCurrentDirectory(), assetsDir,
	            assetName));

            if (bundle == null)
            {
	            throw new Exception("Failed to load asset bundle" + assetName);
            }

            return bundle;
        }
    }
}