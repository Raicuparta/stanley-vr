using System.Reflection;
using BepInEx;
using HarmonyLib;
using LIV.SDK.Unity;
using UnityEngine;
using StanleyVr.VrInput;
using StanleyVr.VrUi;

namespace StanleyVr;

[BepInPlugin("StanleyVr", "StanleyVr", "0.2.0")]
public class StanleyVrPlugin : BaseUnityPlugin
{
	private void Awake()
	{
		Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
		
		VrAssetManager.Initialize();

		Debug.Log("####### Stanley Parable Ultra Deluxe version " + Application.version);
		Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

		var shaderBundle = VrAssetManager.LivShadersBundle;
		Debug.Log($"###### using bundl for LIV {shaderBundle}");
		SDKShaders.LoadFromAssetBundle(shaderBundle);

		gameObject.AddComponent<ModXrManager>();
		ActionMap.Initialize();
		
		MainMenuSceneFix.Create();
	}

	private void Update()
	{
		Cursor.lockState = CursorLockMode.None;
		
		if (Input.GetKeyDown((KeyCode.Equals)))
		{
			Time.timeScale = Time.timeScale == 0 ? 1 : 0;
		}

		if (Input.GetKeyDown(KeyCode.F1))
		{
			FindObjectOfType<MainMenu>().ExitMenu();
			FindObjectOfType<MainMenu>().BeginTheGame();
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