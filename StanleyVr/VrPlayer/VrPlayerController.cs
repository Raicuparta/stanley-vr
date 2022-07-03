﻿using UnityEngine;
using Valve.VR.Extras;

namespace StanleyVr.VrPlayer;

public class VrPlayerController: MonoBehaviour
{
    public static Transform Laser { get; private set; }
    private static AssetBundle playerBundle;
    
    public static void Create(Transform parent)
    {
        if (!playerBundle)
        {
            playerBundle = VrAssetManager.LoadBundle("player");
        }
        var playerPrefab = playerBundle.LoadAsset<GameObject>("StanleyVrPlayer");
		var instance = Instantiate(playerPrefab, parent, false);
        instance.AddComponent<VrPlayerController>();
    }

    private void Awake()
    {
        Laser = GetComponentInChildren<SteamVR_LaserPointer>().transform;
		transform.localPosition = Vector3.down;
    }
}