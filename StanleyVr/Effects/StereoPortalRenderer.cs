using UnityEngine;
using UnityEngine.XR;

namespace StanleyVr.Effects;

public class StereoPortalRenderer : MonoBehaviour
{
	// TODO: should also try moving the portal depending on which eye is being rendered?
	// To prevent cases where one eye goes in first.
	// TODO: also detect when head is actually going through the portal instead of just player body.
	private void OnPreRender()
	{
		var camera = Camera.current;
		
		var cameraPosition = camera.transform.position;
		var cameraRotation = camera.transform.rotation;

		UpdateCameraTransform(camera);
		
		foreach (var portal in MainCamera.Portals)
		{
			if (!portal.disabled)
			{
				portal.playerCam = camera;
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
		camera.transform.rotation = cameraRotation;
	}

	private void UpdateCameraTransform(Camera camera)
	{
		if (!camera.stereoEnabled)
		{
			return;
		}
		
		var isLeft = camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left;

		// TODO also fix rotation?
		
		InputDevices.GetDeviceAtXRNode(XRNode.CenterEye)
			.TryGetFeatureValue(CommonUsages.centerEyePosition, out var centerEyePosition);
		
		InputDevices.GetDeviceAtXRNode(isLeft ? XRNode.LeftEye : XRNode.RightEye)
			.TryGetFeatureValue(isLeft ? CommonUsages.leftEyePosition : CommonUsages.rightEyePosition, out var eyePosition);

		var eyeOffset = eyePosition - centerEyePosition;
		camera.transform.position += camera.transform.TransformVector(eyeOffset);
	}
}