using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StanleyVr;

public static class VrAssetManager
{
	public static GameObject VrPlayerPrefab { get; private set; }
	public static GameObject VrUi { get; private set; }
	public static AssetBundle LivShadersBundle { get; private set; }
	public static Object[] XrManagerAssets { get; private set; }

	private const string assetsDir = "/BepInEx/plugins/StanleyVr/Assets/";

	public static void Initialize()
	{
		LivShadersBundle = LoadBundle("liv-shaders");
		VrPlayerPrefab = LoadBundle("player").LoadAsset<GameObject>("StanleyVrPlayer");
		VrUi = LoadBundle("vr-ui").LoadAsset<GameObject>("VrUi");
		XrManagerAssets = LoadBundle("xrmanager").LoadAllAssets();
	}
	private static AssetBundle LoadBundle(string assetName)
	{
		Debug.Log($"loading bundle {assetName}...");
		var bundle = AssetBundle.LoadFromFile($"{Directory.GetCurrentDirectory()}{assetsDir}{assetName}");

		if (bundle == null)
		{
			throw new Exception("Failed to load asset bundle" + assetName);
		}

		Debug.Log($"Loaed bundle {bundle.name}");

		return bundle;
	}
}