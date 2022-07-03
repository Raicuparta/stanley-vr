using UnityEngine;
using UnityEngine.XR;

namespace StanleyVr.Effects;

public class StereoPortalRenderer : MonoBehaviour
{
	private void OnPreRender()
	{
		var camera = Camera.current;

		var isLeft = camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left;
		
		InputDevices.GetDeviceAtXRNode(XRNode.CenterEye)
			.TryGetFeatureValue(CommonUsages.centerEyePosition, out var centerEyePosition);
		
		InputDevices.GetDeviceAtXRNode(isLeft ? XRNode.LeftEye : XRNode.RightEye)
			.TryGetFeatureValue(isLeft ? CommonUsages.leftEyePosition : CommonUsages.rightEyePosition, out var eyePosition);

		var cameraPosition = camera.transform.position;
		camera.transform.localPosition = eyePosition - centerEyePosition;
		
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