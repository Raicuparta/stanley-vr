using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityStandardAssets.ImageEffects;

namespace StanleyVr.VrCamera;

public class VrCameraController: MonoBehaviour
{
	private static Camera bucketCamera;
	private const string bucketCameraName = "Bucket Camera";
	
	private void Start()
	{
		var trackedPoseDriver = gameObject.AddComponent<TrackedPoseDriver>();
		trackedPoseDriver.UseRelativeTransform = true;
		transform.localScale = Vector3.one * 0.5f;

		trackedPoseDriver.trackingType = GetComponent<MainCamera>() ? TrackedPoseDriver.TrackingType.RotationAndPosition : TrackedPoseDriver.TrackingType.RotationOnly;

		var camera = GetComponent<Camera>();
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
			// bucketCamera.cullingMask |= 1 << LayerMask.NameToLayer("UI");
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