using System;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace StanleyVr;

public class VrCamera: MonoBehaviour
{
	private void Start()
	{
		GetComponent<TrackedPoseDriver>().UseRelativeTransform = true;
	}
}