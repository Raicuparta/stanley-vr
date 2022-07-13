using System;
using System.Collections.Generic;
using StanleyVr.VrInput.ActionInputs;
using StanleyVr.VrPlayer;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace StanleyVr.VrStage;

public class VrStageController: MonoBehaviour
{
	public static VrStageController Instance;
	
	private const float minPositionOffset = 0;
    private const float maxPositionOffset = 1f;
    
	private Vector3 prevCameraPosition;
	private StanleyController stanleyController;
	private MainCamera mainCamera;
	private bool previousMotionFrozen;
	private VrPlayerController vrPlayerController;

	public static void Create(StanleyController stanleyController)
	{
		Instance = new GameObject("VrStage").AddComponent<VrStageController>();
		Instance.transform.SetParent(stanleyController.transform.Find("CameraParent"), false);
		Instance.stanleyController = stanleyController;
	}

	public void SetUp(MainCamera newMainCamera)
	{
		mainCamera = newMainCamera;
		mainCamera.transform.SetParent(transform, true);
		CreateVrPlayer();
		Recenter();
	}

	private void Start()
	{
		
		previousMotionFrozen = stanleyController.motionFrozen;
		transform.localScale = Vector3.one * 0.5f;
		LivManager.Create(this);
	}

	private void OnEnable()
	{
		InputTracking.nodeAdded += HandleXrNodeAdded;
	}
	
	private void OnDisable()
	{
		InputTracking.nodeAdded -= HandleXrNodeAdded;
	}

	private void HandleXrNodeAdded(XRNodeState xrNode)
	{
		Debug.Log($"Added {xrNode}");
		CreateVrPlayer();
	}

	private static bool AreBothControllersActiove()
	{
		var nodes = new List<XRNodeState>();
		InputTracking.GetNodeStates(nodes);

		var rightActive = false;
		var leftActive = false;
		
		foreach (var node in nodes)
		{
			if (node.nodeType == XRNode.RightHand) rightActive = true;
			if (node.nodeType == XRNode.LeftHand) leftActive = true;
		}

		return rightActive && leftActive;
	}

	private void CreateVrPlayer()
	{
		if (vrPlayerController != null || !AreBothControllersActiove()) return;
		Debug.Log("CreateVrPlayer");
		vrPlayerController = VrPlayerController.Create(transform, stanleyController);
	}

	private void Update()
	{
		UpdateRecenter();
		UpdateRoomScalePosition();
	}

	private void UpdateRecenter()
	{
		if (ActionInputDefinitions.Recenter.ButtonDown)
		{
			Recenter();
		}

		if (!previousMotionFrozen && stanleyController.motionFrozen)
		{
			Recenter();
			previousMotionFrozen = stanleyController.motionFrozen;
		}
	}

	public void Recenter()
	{
		if (!mainCamera) return;

		InputDevices.GetDeviceAtXRNode(XRNode.CenterEye)
			.TryGetFeatureValue(CommonUsages.centerEyePosition, out var centerEyePosition);

		prevCameraPosition = centerEyePosition;
		
		InputDevices.GetDeviceAtXRNode(XRNode.CenterEye)
			.TryGetFeatureValue(CommonUsages.centerEyeRotation, out var centerEyerotation);
		
		transform.localPosition -= transform.parent.InverseTransformPoint(mainCamera.transform.position);
		
		transform.localRotation = Quaternion.Inverse(centerEyerotation);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
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
        stanleyOffset.y = 0;

		// trackedPoseDriver.enabled = false;

		transform.position -= stanleyOffset;
		// transform.position -= stanleyOffset;

		// When a Tracked Pose Driver has UseRelativePosition = true,
		// that relative position is only taken into account during Awake.
		// trackedPoseDriver.Invoke("Awake", 0);

		// trackedPoseDriver.enabled = true;
    }

    public void AttachToLeftHand(Transform child)
    {
	    if (!vrPlayerController)
	    {
		    Debug.LogWarning("Tried to attach an object to the left hand before player controller is ready");
		    return;
	    }
	    vrPlayerController.AttachToLeftHand(child);
    }
}