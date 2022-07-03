using UnityEngine;
using UnityEngine.SpatialTracking;

namespace StanleyVr;

public class VrCamera: MonoBehaviour
{
	private static Camera bucketCamera;
	private const string bucketCameraName = "Bucket Camera";
	
	private void Start()
	{
		var trackedPoseDriver = gameObject.AddComponent<TrackedPoseDriver>();
		trackedPoseDriver.UseRelativeTransform = true;
		transform.localScale = Vector3.one * 0.5f;

		trackedPoseDriver.trackingType = GetComponent<MainCamera>() ? TrackedPoseDriver.TrackingType.RotationAndPosition : TrackedPoseDriver.TrackingType.RotationOnly;

		if (name == bucketCameraName)
		{
			bucketCamera = GetComponent<Camera>();
			bucketCamera.cullingMask |= 1 << LayerMask.NameToLayer("UI");
		}
	}

	public static Camera GetUiCamera()
	{
		if (bucketCamera && bucketCamera.isActiveAndEnabled)
		{
			return bucketCamera;
		}

		return Camera.main ? Camera.main : Camera.current;
	}
}