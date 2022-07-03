using System;
using StanleyVr.VrCamera;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
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
    }

    private void Start()
    {
        var vrCameraController = GetComponentInChildren<VrCameraController>();

        var poses = gameObject.GetComponentsInChildren<SteamVR_Behaviour_Pose>();
        foreach (var pose in poses)
        {
            pose.origin = vrCameraController.transform.parent;
        }
    }
    
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F3))
		{
			Recenter();
		}
	}

	private void Recenter()
	{
		enabled = false;
		
		InputDevices.GetDeviceAtXRNode(XRNode.CenterEye)
			.TryGetFeatureValue(CommonUsages.centerEyePosition, out var centerEyePosition);
		
		InputDevices.GetDeviceAtXRNode(XRNode.CenterEye)
			.TryGetFeatureValue(CommonUsages.centerEyeRotation, out var centerEyerotation);
		
		transform.localPosition = -centerEyePosition;
		
		transform.localRotation = Quaternion.Inverse(centerEyerotation);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

		enabled = true;
	}
}