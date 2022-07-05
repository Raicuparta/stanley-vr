using System;
using UnityEngine;
using Valve.VR.Extras;

namespace StanleyVr.VrPlayer;

public class VrPlayerController: MonoBehaviour
{
    public static Transform Laser { get; private set; }
    private static AssetBundle playerBundle;
    
    public static void Create(Transform parent, StanleyController stanleyController)
    {
        if (!playerBundle)
        {
            playerBundle = VrAssetManager.LoadBundle("player");
        }
        var playerPrefab = playerBundle.LoadAsset<GameObject>("StanleyVrPlayer");
		var instance = Instantiate(playerPrefab, parent, false);
        instance.AddComponent<VrPlayerController>();
        
        var laserObject = instance.GetComponentInChildren<StanleyVrLaserPointer>();
        laserObject.RayCollisionMask = LayerMask.GetMask("Default", "UI");
        laserObject.MaxDistance = stanleyController.armReach;
        
        Laser = laserObject.transform;
    }

    private void Start()
    {
        var shader = Shader.Find("Stanley/Stanley_Default_ReflectionProbeLit_Low");
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                material.shader = shader;
            }
        }
    }
}