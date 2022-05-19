using UnityEngine;

namespace StanleyVr;

public class StereoPortalRenderer : MonoBehaviour
{
	private void OnPreRender()
	{
		var camera = Camera.current;
		var cameraRight = camera.transform.right;
		var separation = camera.stereoSeparation * camera.transform.parent.localScale.x;
		var offset = cameraRight.normalized * separation * 0.5f;

		if (camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
		{
			offset *= -1;
		}

		var cameraPosition = camera.transform.position;
		camera.transform.position = cameraPosition + offset;

		foreach (var portal in MainCamera.Portals)
		{
			if (!portal.disabled)
			{
				portal.Render();
			}
		}

		foreach (var portal in MainCamera.Portals)
		{
			if (!portal.disabled)
			{
				portal.PostPortalRender();
			}
		}

		camera.transform.position = cameraPosition;
	}
}