using System;
using StanleyVr.VrInput.ActionInputs;
using StanleyVr.VrPlayer;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

namespace StanleyVr.VrStage;

public class VrStageController: MonoBehaviour
{
    private const float minPositionOffset = 0;
    private const float maxPositionOffset = 1f;
    
	public static Action OnRecenter;
	private Vector3 prevCameraPosition;
	private StanleyController stanleyController;
	private MainCamera mainCamera;

	public static void Create(MainCamera mainCamera)
	{
		var instance = new GameObject("VrStage").AddComponent<VrStageController>();
		instance.transform.SetParent(mainCamera.transform.parent, false);
		instance.mainCamera = mainCamera;

		mainCamera.transform.SetParent(instance.transform, true);
	}

	private void Awake()
	{
		stanleyController = GetComponentInParent<StanleyController>();
	}

	private void Start()
	{
		VrPlayerController.Create(transform);
		
		Invoke(nameof(Recenter), 1f);
	}

	private void Update()
	{
		// TODO cleanup repeated code between here and VrPlayerController.
		if (ActionInputDefinitions.Recenter.ButtonDown)
		{
			Recenter();
		}

		if (Input.GetKeyDown(KeyCode.F2))
		{
			UpdateRoomScalePosition();
		}
		
		UpdateRoomScalePosition();
	}

	private void Recenter()
	{
		InputDevices.GetDeviceAtXRNode(XRNode.CenterEye)
			.TryGetFeatureValue(CommonUsages.centerEyePosition, out var centerEyePosition);

		prevCameraPosition = centerEyePosition;
		
		InputDevices.GetDeviceAtXRNode(XRNode.CenterEye)
			.TryGetFeatureValue(CommonUsages.centerEyeRotation, out var centerEyerotation);
		
		transform.localPosition -= transform.parent.InverseTransformPoint(mainCamera.transform.position);
		
		transform.localRotation = Quaternion.Inverse(centerEyerotation);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

		OnRecenter?.Invoke();
	}
	
    private void UpdateRoomScalePosition()
    {
	    if (!stanleyController) return;
	    
	    InputDevices.GetDeviceAtXRNode(XRNode.CenterEye)
			.TryGetFeatureValue(CommonUsages.centerEyePosition, out var centerEyePosition);

	    var localPositionDelta = centerEyePosition - prevCameraPosition;
        localPositionDelta.y = 0;

        var worldPositionDelta = transform.TransformVector(localPositionDelta);

        if (worldPositionDelta.sqrMagnitude < minPositionOffset || !stanleyController.grounded ||
            !stanleyController.enabled) return;

        prevCameraPosition = centerEyePosition;

        if (worldPositionDelta.sqrMagnitude > maxPositionOffset) return;

        var groundNormal = Vector3.up; // TODO check ground normal.
        var groundedPositionDelta = Vector3.ProjectOnPlane(worldPositionDelta, groundNormal);

        var prevStanleyPosition = stanleyController.transform.position;
        
        stanleyController.character.Move(groundedPositionDelta);

        var stanleyOffset = stanleyController.transform.position - prevStanleyPosition;

		// trackedPoseDriver.enabled = false;

		transform.position -= stanleyOffset;
		// transform.position -= stanleyOffset;

		// When a Tracked Pose Driver has UseRelativePosition = true,
		// that relative position is only taken into account during Awake.
		// trackedPoseDriver.Invoke("Awake", 0);

		// trackedPoseDriver.enabled = true;
    }
}