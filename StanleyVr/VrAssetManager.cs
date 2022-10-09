﻿using System;
using System.IO;
using BepInEx;
using UnityEngine;

namespace StanleyVr;

public static class VrAssetManager
{
	public static AssetBundle LivShadersBundle { get; private set; }

	private const string assetsDir = "StanleyVr/Assets/";

	public static void Initialize()
	{
		LivShadersBundle = LoadBundle("liv-shaders");
	}

	public static AssetBundle LoadBundle(string assetName)
	{
		Debug.Log($"loading bundle {assetName}...");
		var bundle = AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, Path.Combine(assetsDir, assetName)));

		if (bundle == null)
		{
			throw new Exception("Failed to load asset bundle" + assetName);
		}

		Debug.Log($"Loaed bundle {bundle.name}");

		return bundle;
	}
}