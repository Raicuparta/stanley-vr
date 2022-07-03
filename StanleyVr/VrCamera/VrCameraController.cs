using System;
using StanleyVr.VrInput.ActionInputs;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;
using UnityStandardAssets.ImageEffects;

namespace StanleyVr.VrCamera;

public class VrCameraController: MonoBehaviour
{
	private static Camera bucketCamera;
	private const string bucketCameraName = "Bucket Camera";
	private Camera camera;
	private TrackedPoseDriver trackedPoseDriver;

	public static Action OnRecenter;
	
	private void Start()
	{
		trackedPoseDriver = gameObject.AddComponent<TrackedPoseDriver>();
		trackedPoseDriver.UseRelativeTransform = true;
		transform.localScale = Vector3.one * 0.5f;

		trackedPoseDriver.trackingType = GetComponent<MainCamera>() ? TrackedPoseDriver.TrackingType.RotationAndPosition : TrackedPoseDriver.TrackingType.RotationOnly;

		camera = GetComponent<Camera>();
		camera.backgroundColor = Color.black;

		if (camera.targetTexture)
		{
			// This is to fix the default main menu camera.
			// Maybe shouldn't be done for all cases where there is a render texture.
			camera.targetTexture = null;
			Debug.LogWarning($"#### VrCameraController has removed the target texture from {name}");
		}

		var blur = GetComponent<Blur>();
		if (blur)
		{
			Destroy(blur);
		}
		
		if (name == bucketCameraName)
		{
			bucketCamera = camera;
		}
		
		Invoke(nameof(Recenter), 1f);
	}

	public static Camera GetBucketCamera()
	{
		if (bucketCamera && bucketCamera.isActiveAndEnabled)
		{
			return bucketCamera;
		}

		return Camera.main ? Camera.main : Camera.current;
	}

	private void Update()
	{
		// TODO cleanup repeated code between here and VrPlayerController.
		if (ActionInputDefinitions.Recenter.ButtonDown)
		{
			Recenter();
		}
	}

	private void Recenter()
	{
		InputDevices.GetDeviceAtXRNode(XRNode.CenterEye)
			.TryGetFeatureValue(CommonUsages.centerEyePosition, out var centerEyePosition);
		
		InputDevices.GetDeviceAtXRNode(XRNode.CenterEye)
			.TryGetFeatureValue(CommonUsages.centerEyeRotation, out var centerEyerotation);
		
		trackedPoseDriver.enabled = false;

		transform.localPosition = -centerEyePosition;
		
		transform.localRotation = Quaternion.Inverse(centerEyerotation);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

		// When a Tracked Pose Driver has UseRelativePosition = true,
		// that relative position is only taken into account during Awake.
		trackedPoseDriver.Invoke("Awake", 0);

		trackedPoseDriver.enabled = true;
		
		OnRecenter?.Invoke();
	}
}