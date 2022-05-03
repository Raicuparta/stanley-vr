using System;
using System.IO;
using UnityEngine;

namespace StanleyVr;

public static class VrAssetManager
{
    private const string assetsDir = "/BepInEx/plugins/StanleyVr/Assets/";
        
	public static AssetBundle LoadBundle(string assetName)
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