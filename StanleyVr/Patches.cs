using System;
using System.Globalization;
using System.IO;
using System.Linq;
using AmplifyBloom;
using HarmonyLib;
using InControl;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using UnityEngine.Video;
using Valve.VR;

namespace StanleyVr;

[HarmonyPatch]
public static class Patches
{
    private static readonly string[] canvasesToIgnore =
    {
        "com.sinai.unityexplorer_Root", // UnityExplorer.
        "com.sinai.unityexplorer.MouseInspector_Root" // UnityExplorer.
    };
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
    private static void MoveCanvasesToWorldSpace(CanvasScaler __instance)
    {
	    var canvas = __instance.GetComponent<Canvas>();

        if (!canvas || canvasesToIgnore.Contains(canvas.name)) return;

        Debug.Log($"Found CanvasScaler {__instance.name}");

        if (!VrUi.Instance)
        {  
	        // TODO don't do this here dummy.
	        VrUi.Instance = VrUi.Create();
        }
		VrUi.Instance.SetUpCanvas(canvas);
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StanleyController), nameof(StanleyController.ClickingOnThings))]
	private static bool LaserInteraction(StanleyController __instance)
	{
		if (!VrLaser.Instance)
		{
			return true;
		}
		if (Singleton<GameMaster>.Instance.FullScreenMoviePlaying || !Singleton<GameMaster>.Instance.stanleyActions.UseAction.WasPressed)
		{
			return false;
		}
		RaycastHit hitInfo;
		// TODO just replace __instance.camparent during this method instead of copying everything.
		if (Physics.Raycast(VrLaser.Instance.transform.position, VrLaser.Instance.transform.forward, out hitInfo, __instance.armReach, __instance.clickLayers, QueryTriggerInteraction.Ignore))
		{
			GameObject gameObject = hitInfo.collider.gameObject;
			HammerEntity component = gameObject.GetComponent<HammerEntity>();
			if (component != null)
			{
				component.Use();
			}
			else
			{
				__instance.PlayKeyboardSound();
			}
			if (StanleyController.OnInteract != null)
			{
				StanleyController.OnInteract(gameObject);
			}
		}
		else
		{
			__instance.PlayKeyboardSound();
			if (StanleyController.OnInteract != null)
			{
				StanleyController.OnInteract(null);
			}
		}

		return false;
	}
    
	[HarmonyPrefix]
    [HarmonyPatch(typeof(CanvasOrdering), nameof(CanvasOrdering.Update))]
    [HarmonyPatch(typeof(SetEventCameraOnStart), nameof(SetEventCameraOnStart.Start))]
    private static bool DisableCanvasOrdering()
    {
	    return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Start))]
    private static void FixMainMenuCanvas(MainMenu __instance)
    {
	    __instance.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AmplifyColorBase), nameof(AmplifyColorBase.Start))]
    private static void TrackCameraVrRotation(AmplifyColorBase __instance)
    {
	    if (__instance.GetComponent<TrackedPoseDriver>()) return;

	    var poseDriver = __instance.gameObject.AddComponent<TrackedPoseDriver>();
	    poseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationOnly;
	    __instance.transform.localScale = Vector3.one * 0.5f;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainCamera), nameof(MainCamera.Start))]
    private static void FixCameraScale(MainCamera __instance)
    {

        __instance.gameObject.AddComponent<StereoPortalRenderer>();
        __instance.transform.localPosition = Vector3.down;
        __instance.transform.localRotation = Quaternion.identity;

        if (!__instance.GetComponent<TrackedPoseDriver>())
        {
			var cameraPoseDriver = __instance.gameObject.AddComponent<TrackedPoseDriver>();	        
			cameraPoseDriver.UseRelativeTransform = true;
        }

        var camera = __instance.GetComponent<Camera>();
        camera.transform.parent.localScale = Vector3.one * 0.5f;
        VrUi.Instance.SetUpCamera(camera);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MobileBlur), nameof(MobileBlur.OnRenderImage))]
    private static bool PreventPauseBlur(MobileBlur __instance)
    {
	    __instance.enabled = false;
	    return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainCamera), nameof(MainCamera.OnPreCull))]
    private static bool DisableOnPreCull()
    {
	    return false;
    }

    // This definitely doesn't need to be called this often,
    // but I'm not sure if there's some mechanism that can affect these values.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StanleyController), nameof(StanleyController.View))]
    private static void PreventCameraVerticalRotation(StanleyController __instance)
    {
        __instance.controllerSensitivityY = 0;
        __instance.mouseSensitivityY = 0;
    }
    
    // This is a copy paste of the entire StanleyController.Movement method,
    // with just small modifications to use the VR camera rotation for movement direction.
    // Wouldn't need to do this trash if I just rotated the player body with the camera,
    // but that's harder to do so for now I'll just do this.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StanleyController), nameof(StanleyController.Movement))]
    private static bool CameraBasedMovementDirection(StanleyController __instance)
    {
        __instance.grounded = __instance.character.isGrounded;
		float y = Singleton<GameMaster>.Instance.stanleyActions.Movement.Y;
		float x = Singleton<GameMaster>.Instance.stanleyActions.Movement.X;
		__instance.movementInput.x = x;
		__instance.movementInput.z = y;
		if (PlatformSettings.Instance.isStandalone.GetBooleanValue() && __instance.mouseWalkConfigurable.GetBooleanValue() && Input.GetMouseButton(1) && Input.GetMouseButton(0))
		{
			__instance.movementInput.z = 1f;
		}
		__instance.movementInput = Vector3.ClampMagnitude(__instance.movementInput, 1f) * (__instance.executeJump ? 0.5f : 1f);
		if (__instance.movementInput.magnitude > 0f)
		{
			__instance.movementGoal = Vector3.Lerp(__instance.movementGoal, __instance.movementInput, __instance.DeltaTime * __instance.runAcceleration);
		}
		else
		{
			__instance.movementGoal = Vector3.Lerp(__instance.movementGoal, __instance.movementInput, __instance.DeltaTime * __instance.runDeacceleration);
		}
		if (!__instance.executeJump && __instance.jumpConfigurable.GetBooleanValue() && Singleton<GameMaster>.Instance.stanleyActions.JumpAction.WasPressed)
		{
			if (!__instance.executeJump && StanleyController.OnActuallyJumping != null)
			{
				StanleyController.OnActuallyJumping();
			}
			__instance.executeJump = true;
		}
		if (__instance.executeJump)
		{
			__instance.jumpTime += __instance.DeltaTime * __instance.jumpAccerlation;
			__instance.jumpValue = __instance.jumpCurve.Evaluate(Mathf.Clamp01(__instance.jumpTime)) * __instance.jumpPower;
		}
		if (__instance.jumpTime >= 1f / __instance.jumpAccerlation * __instance.jumpAccerlation)
		{
			__instance.executeJump = false;
			__instance.jumpValue = 0f;
			__instance.jumpTime = 0f;
		}
		bool flag = Singleton<GameMaster>.Instance.stanleyActions.Crouch.IsPressed;
		if (__instance.wasCrouching && __instance.ForceStayCrouched)
		{
			flag = true;
		}
		if (__instance.ForceCrouched)
		{
			flag = true;
		}
		float num = ((!flag) ? __instance.uncrouchedColliderHeight : __instance.crouchedColliderHeight);
		__instance.character.height = Mathf.SmoothStep(__instance.character.height, num, __instance.crouchSmoothing);
		if (__instance.SnapToNewHeightNextFrame)
		{
			__instance.character.height = num;
			__instance.SnapToNewHeightNextFrame = false;
		}
		__instance.cameraParent.localPosition = Vector3.up * __instance.character.height / 2f * __instance.characterHeightMultipler;
		__instance.camParentOrigLocalPos = __instance.camParent.localPosition;
		__instance.wasCrouching = flag;
		__instance.movement = __instance.movementGoal * __instance.walkingSpeed * __instance.WalkingSpeedMultiplier;
		
		// The original line was this:
		// __instance.movement = __instance.transform.TransformDirection(__instance.movement);
		// With this change, we can use the VR camera rotation as a basis for movement direction:
		var cameraForward = Camera.main.transform.forward;
		cameraForward.y = 0;

		var forwardRotation = Quaternion.LookRotation(cameraForward, Vector3.up);
		
		__instance.movement = forwardRotation * __instance.movement;
		
		__instance.movement += Vector3.up * __instance.jumpValue;
		Action<Vector3> onPositionUpdate = BackgroundCamera.OnPositionUpdate;
		if (onPositionUpdate != null)
		{
			onPositionUpdate(new Vector3(0f, __instance.character.velocity.y, 0f));
		}
		RotatingDoor rotatingDoor = __instance.WillHitDoor(__instance.movement * Singleton<GameMaster>.Instance.GameDeltaTime);
		if (rotatingDoor == null)
		{
			if (__instance.lastHitRotatingDoor != null)
			{
				__instance.lastHitRotatingDoor.PlayerTouchingDoor = false;
				__instance.lastHitRotatingDoor = null;
			}
		}
		else
		{
			if (__instance.lastHitRotatingDoor != null && __instance.lastHitRotatingDoor != rotatingDoor)
			{
				Debug.LogWarning("Player is hitting multiple doors this should not happen!\n" + __instance.lastHitRotatingDoor.name + "\n" + rotatingDoor.name);
			}
			__instance.lastHitRotatingDoor = rotatingDoor;
			__instance.lastHitRotatingDoor.PlayerTouchingDoor = true;
		}
		__instance.UpdateInAir(!__instance.grounded);
		if (!__instance.grounded)
		{
			__instance.gravityMultiplier = Mathf.Lerp(__instance.gravityMultiplier, 1f, Singleton<GameMaster>.Instance.GameDeltaTime * __instance.gravityFallAcceleration);
			__instance.movement *= __instance.inAirMovementMultiplier;
		}
		else
		{
			__instance.gravityMultiplier = __instance.groundedGravityMultiplier;
		}
		if (flag)
		{
			__instance.movement *= __instance.crouchMovementMultiplier;
		}
		if (__instance.character.enabled)
		{
			__instance.character.Move((__instance.movement + Vector3.up * __instance.maxGravity * __instance.gravityMultiplier) * Singleton<GameMaster>.Instance.GameDeltaTime);
		}
		return false;
    }
    
    // This is a copy paste of the entire StanleyController.Update method,
    // with just small modifications to stop trying to change the FOV
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StanleyController), nameof(StanleyController.Update))]
    private static bool PreventChangingFov(StanleyController __instance)
    {
	    if (!Singleton<GameMaster>.Instance.IsLoading && GameMaster.ONMAINMENUORSETTINGS)
		{
			AudioListener.volume = Singleton<GameMaster>.Instance.masterVolume;
		}
		StanleyController.StanleyPosition = __instance.transform.position;
		
		// Trying to change the FOV in VR causes warnings.
		// __instance.cam.fieldOfView = FieldOfViewBase + FieldOfViewAdditiveModifier;
		
		if (!__instance.viewFrozen)
		{
			__instance.View();
		}
		if (!__instance.motionFrozen)
		{
			__instance.Movement();
			__instance.UpdateCurrentlyStandingOn();
			__instance.Footsteps();
			__instance.ClickingOnThings();
		}
		else if (__instance.character.enabled)
		{
			__instance.character.Move(Vector2.zero);
		}
		if (!__instance.viewFrozen)
		{
			__instance.FloatCamera();
		}
		if (BucketController.HASBUCKET)
		{
			if (__instance.character.enabled && __instance.grounded)
			{
				__instance.Bucket.SetWalkingSpeed(__instance.character.velocity.magnitude / (__instance.walkingSpeed * __instance.WalkingSpeedMultiplier));
			}
			else
			{
				__instance.Bucket.SetWalkingSpeed(0f);
			}
		}
	    return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SteamVR_Input), nameof(SteamVR_Input.GetActionsFileFolder))]
    private static bool GetActionsFileFromMod(ref string __result)
    {
        __result = $"{Directory.GetCurrentDirectory()}/BepInEx/plugins/StanleyVr/Bindings";
        return false;
    }

    private const string RotateHorizontal = "analog 3";
    private const string RotateVertical = "analog 4";
    private const string MoveVertical = "analog 1";
    private const string MoveHorizontal = "analog 0";
    private const string Interact = "button 0";

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetAxisRaw))]
    private static void ReadVrAnalogInput(string axisName, ref float __result)
    {
	    if (axisName.EndsWith(RotateHorizontal))
		    __result = SteamVR_Actions._default.Rotate.axis.x;
	    else if (axisName.EndsWith(RotateVertical))
		    __result = -SteamVR_Actions._default.Rotate.axis.y;
	    else if (axisName.EndsWith(MoveVertical))
		    __result = -SteamVR_Actions._default.Move.axis.y;
	    else if (axisName.EndsWith(MoveHorizontal))
		    __result = SteamVR_Actions._default.Move.axis.x;
	    else
		    __result = __result;
	    // Debug.Log($"### ReadRawAnalogValue axisName {axisName}");
	    // return true;
    }
    

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetKey), typeof(string))]
    private static void ReadVrButtonInput(string name, ref bool __result)
    {
	    if (name.EndsWith(Interact))
	    {
			// Debug.Log($"buttonName {name}");
			__result = SteamVR_Actions._default.Interact.state;
	    }
	    // Debug.Log($"### ReadRawAnalogValue axisName {axisName}");
	    // return true;
    }
    

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VideoPlayer), nameof(VideoPlayer.Play))]
    private static void FixVideoPlayer(VideoPlayer __instance)
    {
	    Debug.Log($"######## found video player {__instance.name}");
	    var camera = __instance.GetComponent<Camera>();

	    if (!camera) return;

	    camera.targetTexture = VrUi.Instance.GetComponentInChildren<Camera>().targetTexture;

	    // Debug.Log($"### ReadRawAnalogValue axisName {axisName}");
	    // return true;
    }
}