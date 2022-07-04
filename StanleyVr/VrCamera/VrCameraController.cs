using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityStandardAssets.ImageEffects;

namespace StanleyVr.VrCamera;

public class VrCameraController: MonoBehaviour
{
	private static Camera bucketCamera;
	private const string bucketCameraName = "Bucket Camera";
	private Camera camera;
	private TrackedPoseDriver trackedPoseDriver;

	private void Start()
	{
		trackedPoseDriver = gameObject.AddComponent<TrackedPoseDriver>();
		trackedPoseDriver.UseRelativeTransform = false;
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
	}

	public static Camera GetBucketCamera()
	{
		if (bucketCamera && bucketCamera.isActiveAndEnabled)
		{
			return bucketCamera;
		}

		return Camera.main ? Camera.main : Camera.current;
	}
}