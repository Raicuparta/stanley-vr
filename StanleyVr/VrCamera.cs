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
		trackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationOnly;
		transform.localScale = Vector3.one * 0.5f;

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