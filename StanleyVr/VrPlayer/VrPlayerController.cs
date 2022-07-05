using System;
using System.Linq;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;

namespace StanleyVr.VrPlayer;

public class VrPlayerController: MonoBehaviour
{
    public static Transform Laser { get; private set; }
    private static AssetBundle playerBundle;
    private Transform leftHand;
    private Transform rightHand;

    public static VrPlayerController Create(Transform parent, StanleyController stanleyController)
    {
        if (!playerBundle)
        {
            playerBundle = VrAssetManager.LoadBundle("player");
        }
        var playerPrefab = playerBundle.LoadAsset<GameObject>("StanleyVrPlayer");
		var instance = Instantiate(playerPrefab, parent, false).AddComponent<VrPlayerController>();
        
        var laserObject = instance.GetComponentInChildren<StanleyVrLaserPointer>();
        laserObject.RayCollisionMask = LayerMask.GetMask("Default", "UI");
        laserObject.MaxDistance = stanleyController.armReach;
        
        Laser = laserObject.transform;

        return instance;
    }

    private void Awake()
    {
        var hands = GetComponentsInChildren<Hand>();
        leftHand = hands.First(hand => hand.handType == SteamVR_Input_Sources.LeftHand).transform;
        rightHand = hands.First(hand => hand.handType == SteamVR_Input_Sources.RightHand).transform;
    }

    private void Start()
    {
        var shader = Shader.Find("Stanley/Stanley_Default_ReflectionProbeLit_Low");
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.gameObject.layer = LayerMask.NameToLayer("Bucket");
            foreach (var material in renderer.materials)
            {
                material.shader = shader;
            }
        }
    }

    public void AttachToLeftHand(Transform child)
    {
        child.SetParent(leftHand);
    }
}