using System.Collections.Generic;
using StanleyVr.VrInput.ActionInputs;
using StanleyVr.VrPlayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

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

	private void Awake()
	{
		InputTracking.nodeAdded += HandleXrNodeAdded;
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
	}

	private void Start()
	{
		previousMotionFrozen = stanleyController.motionFrozen;
		SetUpStageTransform(true);
		LivManager.Create(this);
	}

	private void OnDestroy()
	{
		InputTracking.nodeAdded -= HandleXrNodeAdded;
		SceneManager.activeSceneChanged -= OnActiveSceneChanged;
	}

	private void HandleXrNodeAdded(XRNodeState xrNode)
	{
		CreateVrPlayer();
	}
	
	private void SetUpStageTransform(bool shouldScaleCamera)
	{
		if (shouldScaleCamera)
		{
			transform.localScale = Vector3.one * 0.5f;
		}
		else
		{
			transform.localScale = Vector3.one;
		}
	}

	private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
	{
		SetUpStageTransform(ShouldScaleCameraInScene(newScene));
	}

	private static bool ShouldScaleCameraInScene(Scene scene)
	{
		return scene.name != "Firewatch_UD_MASTER";
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

	public void RecenterDelayed()
	{
		Invoke(nameof(Recenter), 1f);
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
        
        stanleyController.transform.position += groundedPositionDelta;

        transform.position -= groundedPositionDelta;
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