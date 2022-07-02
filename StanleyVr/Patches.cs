using System.Collections.Generic;
using System.IO;
using System.Linq;
using AmplifyBloom;
using HarmonyLib;
using InControl;
using StanleyVr.VrInput;
using StanleyVr.VrInput.ActionInputs;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR;
using Valve.VR;
using Valve.VR.Extras;
using InputDevice = UnityEngine.XR.InputDevice;

namespace StanleyVr;

[HarmonyPatch]
public static class Patches
{
	private static readonly string[] canvasesToIgnore =
	{
		"com.sinai.unityexplorer_Root", // UnityExplorer.
		"com.sinai.unityexplorer.MouseInspector_Root", // UnityExplorer.
	};

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
	private static void MoveCanvasesToWorldSpace(CanvasScaler __instance)
	{
		var canvas = __instance.GetComponent<Canvas>();

		if (!canvas || canvas.renderMode == RenderMode.WorldSpace || canvasesToIgnore.Contains(canvas.name))
		{
			Debug.Log($"Ignored CanvasScaler {__instance.name}");
			Debug.Log($"!canvas {!canvas}");
			Debug.Log($"canvas.renderMode {canvas.renderMode}");
			return;
		};
	
		Debug.Log($"Found CanvasScaler {__instance.name}");
		
		VrUi.Create(canvas);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StanleyController), nameof(StanleyController.ClickingOnThings))]
	private static bool LaserInteraction(StanleyController __instance)
	{
		if (laser == null) return false;
		
		if (Singleton<GameMaster>.Instance.FullScreenMoviePlaying || !Singleton<GameMaster>.Instance.stanleyActions.UseAction.WasPressed)
		{
			return false;
		}
		RaycastHit hitInfo;
		// TODO just replace __instance.camparent during this method instead of copying everything.
		if (Physics.Raycast(laser.position, laser.forward, out hitInfo, __instance.armReach, __instance.clickLayers, QueryTriggerInteraction.Ignore))
		{
			var gameObject = hitInfo.collider.gameObject;
			var component = gameObject.GetComponent<HammerEntity>();
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
		// This behaviour changes the canvas plane distance and world camera, so I'm disabling it.
		return false;
	}
	
	[HarmonyPrefix]
	[HarmonyPatch(typeof(AmplifyBloomBase), nameof(AmplifyBloomBase.Awake))]
	private static void DisableBloom(AmplifyBloomBase __instance)
	{
		// This bloom looks terrible in VR, so removing it.
		Object.Destroy(__instance);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Start))]
	private static void FixMainMenuCanvas(MainMenu __instance)
	{
		// The MainMenu behaviour changes the canvas rendermode to overlay, so I need ot change it to camera here too.
		__instance.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(BackgroundCamera), nameof(BackgroundCamera.Awake))]
	private static void FixBackgroundCameraScale(BackgroundCamera __instance)
	{
		// BackgroundCamera is what renders the sky in the Epilogue.
		// So it looks much better if we resize it to make the skybox look huge.
		// I'm not sure if BackgorundCamera is used for anything else. Might look bad in other usages.
		__instance.transform.localScale = Vector3.one * 0.1f;

		// TODO is still broken since the sky camera won't rotate when the player rotates with the mouse / controller.
		__instance.backgroundCamera.gameObject.AddComponent<VrCamera>();
		__instance.enabled = false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(AmplifyColorBase), nameof(AmplifyColorBase.Start))]
	private static void SetUpVrCameraGeneral(AmplifyColorBase __instance)
	{
		if (__instance.GetComponent<VrCamera>()) return;

		__instance.gameObject.AddComponent<VrCamera>();
	}

	private static Transform laser;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(MainCamera), nameof(MainCamera.Start))]
	private static void SetUpVrCameraMain(MainCamera __instance)
	{
		__instance.gameObject.AddComponent<StereoPortalRenderer>();
		var transform = __instance.transform;
		transform.localPosition = Vector3.down;
		transform.localRotation = Quaternion.identity;
		var cameraParent = transform.parent;

		var playerPrefab = VrAssetManager.LoadBundle("player").LoadAsset<GameObject>("StanleyVrPlayer");
		var vrPlayerInstance = Object.Instantiate(playerPrefab, cameraParent, false);
		vrPlayerInstance.transform.localPosition = Vector3.down;

		laser = vrPlayerInstance.transform.GetComponentInChildren<SteamVR_LaserPointer>().transform;
		
		__instance.gameObject.AddComponent<VrCamera>();

		var camera = __instance.GetComponent<Camera>();
		camera.transform.parent.localScale = Vector3.one * 0.5f;
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
	private static bool DisablePortalRendering()
	{
		// Portal rendering is now done by StereoPortalRenderer, so no need to render it here too.
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
		var y = Singleton<GameMaster>.Instance.stanleyActions.Movement.Y;
		var x = Singleton<GameMaster>.Instance.stanleyActions.Movement.X;
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
		var flag = Singleton<GameMaster>.Instance.stanleyActions.Crouch.IsPressed;
		if (__instance.wasCrouching && __instance.ForceStayCrouched)
		{
			flag = true;
		}
		if (__instance.ForceCrouched)
		{
			flag = true;
		}
		var num = !flag ? __instance.uncrouchedColliderHeight : __instance.crouchedColliderHeight;
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
		var onPositionUpdate = BackgroundCamera.OnPositionUpdate;
		if (onPositionUpdate != null)
		{
			onPositionUpdate(new Vector3(0f, __instance.character.velocity.y, 0f));
		}
		var rotatingDoor = __instance.WillHitDoor(__instance.movement * Singleton<GameMaster>.Instance.GameDeltaTime);
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
	

	// private const string rotateHorizontal = "analog 3";
	// private const string rotateVertical = "analog 4";
	// private const string moveVertical = "analog 1";
	// private const string moveHorizontal = "analog 0";
	// private const string interact = "button 0";
	
	// [HarmonyPostfix]
	// [HarmonyPatch(typeof(Input), nameof(Input.GetAxisRaw))]
	// private static void ReadVrAnalogInput(string axisName, ref float __result)
	// {
	// 	var rightHandDevices = new List<InputDevice>();
	// 	InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);
	//
	// 	if (rightHandDevices.Count == 0) return;
	//
	// 	var device = rightHandDevices[0];
	//
	// 	device.TryGetFeatureValue(CommonUsages.primary2DAxis, out var primaryAxis);
	// 	device.TryGetFeatureValue(CommonUsages.secondary2DAxis, out var secondaryAxis);
	// 	
	// 	Debug.Log($"## reading axis {axisName}: {__result}");
	// 	if (axisName.EndsWith(rotateHorizontal))
	// 		__result = secondaryAxis.x;
	// 	else if (axisName.EndsWith(rotateVertical))
	// 		__result = -secondaryAxis.y;
	// 	else if (axisName.EndsWith(moveVertical))
	// 		__result = -primaryAxis.y;
	// 	else if (axisName.EndsWith(moveHorizontal))
	// 		__result = primaryAxis.x;
	// 	else
	// 		__result = __result;
	// 	// Debug.Log($"### ReadRawAnalogValue axisName {axisName}");
	// 	// return true;
	// }


	// [HarmonyPostfix]
	// [HarmonyPatch(typeof(Input), nameof(Input.GetKey), typeof(string))]
	// private static void ReadVrButtonInput(string name, ref bool __result)
	// {
	// 	if (name.EndsWith(interact))
	// 	{
	// 		// Debug.Log($"buttonName {name}");
	// 		__result = SteamVR_Actions._default.Interact.state;
	// 	}
	// 	// Debug.Log($"### ReadRawAnalogValue axisName {axisName}");
	// 	// return true;
	// }


	[HarmonyPostfix]
	[HarmonyPatch(typeof(VideoPlayer), nameof(VideoPlayer.Play))]
	private static void FixVideoPlayer(VideoPlayer __instance)
	{
		Debug.Log($"######## found video player {__instance.name}");
		var camera = __instance.GetComponent<Camera>();

		if (!camera) return;

		// camera.targetTexture = VrUi.Instance.GetComponentInChildren<Camera>().targetTexture;

		// Debug.Log($"### ReadRawAnalogValue axisName {axisName}");
		// return true;
	}
	
	private static Dictionary<string, IActionInput> inputMap;

	private static StanleyActions stanleyActionsInstance;
	[HarmonyPostfix]
	[HarmonyPatch(typeof(StanleyActions), MethodType.Constructor)]
	private static void SaveStanleyActionsInstance(StanleyActions __instance)
	{
		stanleyActionsInstance = __instance;
		inputMap = new Dictionary<string, IActionInput>()
		{
			// { ActionNames.AnyButton, SteamVR_Actions.triggerButton }, // TODO any button.
			// { ActionNames.MoveForward, SteamVR_Actions.triggerButton },
			// { ActionNames.MoveBackward, SteamVR_Actions.triggerButton },
			// { ActionNames.MoveLeft, SteamVR_Actions.triggerButton },
			// { ActionNames.MoveRight, SteamVR_Actions.triggerButton },
			// { ActionNames.LookUp, SteamVR_Actions.triggerButton },
			// { ActionNames.LookDown, SteamVR_Actions.triggerButton },
			// { ActionNames.LookLeft, SteamVR_Actions.triggerButton },
			// { ActionNames.LookRight, SteamVR_Actions.triggerButton },
			{ ActionNames.Up, ActionInputDefinitions.MenuUp },
			{ ActionNames.Down, ActionInputDefinitions.MenuDown },
			{ ActionNames.Left, ActionInputDefinitions.MenuLeft },
			{ ActionNames.Right, ActionInputDefinitions.MenuRight },
			{ ActionNames.Crouch, ActionInputDefinitions.Crouch },
			{ ActionNames.Use, ActionInputDefinitions.Interact },
			{ ActionNames.Jump, ActionInputDefinitions.Jump },
			{ ActionNames.Start, ActionInputDefinitions.Menu },
			{ ActionNames.MenuTabLeft, ActionInputDefinitions.MenuTabLeft },
			{ ActionNames.MenuTabRight, ActionInputDefinitions.MenuTabRight },
			{ ActionNames.MenuConfirm, ActionInputDefinitions.Interact },
			{ ActionNames.AnyButton, ActionInputDefinitions.Interact },
			{ ActionNames.MenuBack, ActionInputDefinitions.Menu },
			{ ActionNames.MenuOpen, ActionInputDefinitions.Menu },
			// { ActionNames.FastForward, SteamVR_Actions.primaryButton },
			// { ActionNames.SlowDown, SteamVR_Actions.triggerButton },
			{ ActionNames.Movement, ActionInputDefinitions.Move },
			{ ActionNames.View, ActionInputDefinitions.Rotate },
		};

		var inputModule = Object.FindObjectOfType<InControlInputModule>();
		inputModule.SubmitAction = stanleyActionsInstance.UseAction;
		inputModule.CancelAction = stanleyActionsInstance.MenuBack;
		inputModule.direction = stanleyActionsInstance.Movement;
	}
	
	[HarmonyPrefix]
	[HarmonyPatch(typeof(TwoAxisInputControl), nameof(TwoAxisInputControl.UpdateWithAxes))]
	private static void ReadXrTwoAxisInput(TwoAxisInputControl __instance, ref float x, ref float y)
	{
		if (inputMap == null) return;

		string actionName;

		// Two axis actions don't have names in the game code, so we assign them manually.
		if (__instance == stanleyActionsInstance.View) actionName = ActionNames.View;
		else if (__instance == stanleyActionsInstance.Movement) actionName = ActionNames.Movement;
		else return;
		
		var vrInput = inputMap[actionName];
		x = vrInput.Position.x;
		y = vrInput.Position.y;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(OneAxisInputControl), nameof(OneAxisInputControl.UpdateWithState))]
	private static void ReadXrOneAxisInput(OneAxisInputControl __instance, ref bool state)
	{
		if (__instance is not PlayerAction action) return;
		
		var actionName = action.Name;

		if (inputMap == null || !inputMap.ContainsKey(actionName))
		{
			return;
		}

		state = inputMap[actionName].ButtonValue;
	}
	
	[HarmonyPrefix]
	[HarmonyPatch(typeof(OneAxisInputControl), nameof(OneAxisInputControl.UpdateWithValue))]
	[HarmonyPatch(typeof(OneAxisInputControl), nameof(OneAxisInputControl.UpdateWithRawValue))]
	[HarmonyPatch(typeof(OneAxisInputControl), nameof(OneAxisInputControl.SetValue))]
	private static void ReadXrOneAxisInput(OneAxisInputControl __instance, ref float value)
	{
		if (__instance is not PlayerAction action) return;

		var actionName = action.Name;
		
		if (inputMap == null || !inputMap.ContainsKey(actionName))
		{
			return;
		}
		
		value = inputMap[actionName].ButtonValue ? 1 : 0;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StanleyActions), nameof(StanleyActions.UseAction), MethodType.Getter)]
	private static bool ForceUseActionToUse(ref PlayerAction __result, StanleyActions __instance)
	{
		__result = __instance.Use;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StanleyActions), nameof(StanleyActions.JumpAction), MethodType.Getter)]
	private static bool ForceJumpActionToJump(ref PlayerAction __result, StanleyActions __instance)
	{
		__result = __instance.Jump;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.RegisterInputDeviceTypeChange))]
	private static void ForceRegisterInputDevice(ref GameMaster.InputDevice newDeviceType)
	{
		newDeviceType = GameMaster.InputDevice.GamepadXBOXOneOrGeneric;
	}
}