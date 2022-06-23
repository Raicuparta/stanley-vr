using System;
using UnityEngine;

namespace StanleyVr;

public class VrUi : MonoBehaviour
{
	private Canvas canvas;

	public static VrUi Create(Canvas canvas)
	{
		var instance = canvas.gameObject.AddComponent<VrUi>();
		instance.canvas = canvas;
		return instance;
	}

	private void Start()
	{
		canvas.planeDistance = 0.2f;
		canvas.transform.parent.localScale = Vector3.one * 0.5f;
	}

	private void Update()
	{
		// Inneficient way of doing this.
		// I just want to make sure the camera-space canvas use the correct camera.
		// Should be easy if we use a reference from the VrCamera behaviour.
		canvas.worldCamera = Camera.main ? Camera.main : Camera.current;
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
	}
}