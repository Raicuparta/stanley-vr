﻿using UnityEngine;
using UnityEngine.SpatialTracking;

namespace StanleyVr;

public class VrCamera: MonoBehaviour
{
	private void Start()
	{
		var trackedPoseDriver = gameObject.AddComponent<TrackedPoseDriver>();
		trackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationOnly;
		transform.localScale = Vector3.one * 0.5f;
	}
}