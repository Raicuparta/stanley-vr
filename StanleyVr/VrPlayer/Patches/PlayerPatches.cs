using HarmonyLib;
using StanleyVr.VrStage;
using UnityEngine;

namespace StanleyVr.VrPlayer.Patches;

[HarmonyPatch]
public static class PlayerPatches
{
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
	
	// This definitely doesn't need to be called this often,
	// but I'm not sure if there's some mechanism that can affect these values.
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StanleyController), nameof(StanleyController.View))]
	private static void PreventCameraVerticalRotation(StanleyController __instance)
	{
		__instance.controllerSensitivityY = 0;
		__instance.mouseSensitivityY = 0;
		__instance.viewPitch = 0;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(BucketController), nameof(BucketController.OnBucketPickup), typeof(bool), typeof(bool))]
	private static void MoveBucketToHand(BucketController __instance)
	{
		if (!VrStageController.Instance) return;
		
		VrStageController.Instance.AttachToLeftHand(__instance.transform);
		__instance.transform.localScale = Vector3.one * 2;
		__instance.transform.localPosition = new Vector3(-0.152f, -0.1276f, -0.24f);
		__instance.transform.localEulerAngles = new Vector3(38.8052f, 321.044f, 295.3439f);
	}
	
	[HarmonyPostfix]
	[HarmonyPatch(typeof(BucketController), nameof(BucketController.OnBucketRemoval))]
	[HarmonyPatch(typeof(BucketController), nameof(BucketController.Awake))]
	private static void HideBucket(BucketController __instance)
	{
		__instance.transform.localScale = Vector3.zero;
	}
}