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
		camera = GetComponent<Camera>();

		// TODO move bucket camera logic elsewhere.
		if (name == bucketCameraName)
		{
			bucketCamera = camera;
			camera.transform.localPosition = Vector3.zero;
			camera.transform.localRotation = Quaternion.identity;
			camera.clearFlags = CameraClearFlags.Depth;
			return;
		}
		
		trackedPoseDriver = gameObject.AddComponent<TrackedPoseDriver>();
		trackedPoseDriver.UseRelativeTransform = false;

		var isMainCamera = GetComponent<MainCamera>() != null;
		trackedPoseDriver.trackingType = isMainCamera ? TrackedPoseDriver.TrackingType.RotationAndPosition : TrackedPoseDriver.TrackingType.RotationOnly;

		if (!isMainCamera)
		{
			transform.localScale = Vector3.one * 0.5f;
		}
		
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