using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

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
        Debug.Log($"Found CanvasScaler {__instance.name}");
    
        var canvas = __instance.GetComponent<Canvas>();

        if (canvasesToIgnore.Contains(canvas.name)) return;
        
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            __instance.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        }
        else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            canvas.worldCamera = Camera.current;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            __instance.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainCamera), nameof(MainCamera.Start))]
    private static void FixCameraScale(MainCamera __instance)
    {
        __instance.transform.parent.localScale = Vector3.one * 0.5f;
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
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StanleyController), nameof(StanleyController.Movement))]
    private static bool CameraBasedMovementDirection(StanleyController __instance)
    {
  //       __instance.grounded = character.isGrounded;
		// float y = Singleton<GameMaster>.Instance.stanleyActions.Movement.Y;
		// float x = Singleton<GameMaster>.Instance.stanleyActions.Movement.X;
		// movementInput.x = x;
		// movementInput.z = y;
		// if (PlatformSettings.Instance.isStandalone.GetBooleanValue() && mouseWalkConfigurable.GetBooleanValue() && Input.GetMouseButton(1) && Input.GetMouseButton(0))
		// {
		// 	movementInput.z = 1f;
		// }
		// movementInput = Vector3.ClampMagnitude(movementInput, 1f) * (executeJump ? 0.5f : 1f);
		// if (movementInput.magnitude > 0f)
		// {
		// 	movementGoal = Vector3.Lerp(movementGoal, movementInput, DeltaTime * runAcceleration);
		// }
		// else
		// {
		// 	movementGoal = Vector3.Lerp(movementGoal, movementInput, DeltaTime * runDeacceleration);
		// }
		// if (!executeJump && jumpConfigurable.GetBooleanValue() && Singleton<GameMaster>.Instance.stanleyActions.JumpAction.WasPressed)
		// {
		// 	if (!executeJump && OnActuallyJumping != null)
		// 	{
		// 		OnActuallyJumping();
		// 	}
		// 	executeJump = true;
		// }
		// if (executeJump)
		// {
		// 	jumpTime += DeltaTime * jumpAccerlation;
		// 	jumpValue = jumpCurve.Evaluate(Mathf.Clamp01(jumpTime)) * jumpPower;
		// }
		// if (jumpTime >= 1f / jumpAccerlation * jumpAccerlation)
		// {
		// 	executeJump = false;
		// 	jumpValue = 0f;
		// 	jumpTime = 0f;
		// }
		// bool flag = Singleton<GameMaster>.Instance.stanleyActions.Crouch.IsPressed;
		// if (wasCrouching && ForceStayCrouched)
		// {
		// 	flag = true;
		// }
		// if (ForceCrouched)
		// {
		// 	flag = true;
		// }
		// float num = ((!flag) ? uncrouchedColliderHeight : crouchedColliderHeight);
		// character.height = Mathf.SmoothStep(character.height, num, crouchSmoothing);
		// if (SnapToNewHeightNextFrame)
		// {
		// 	character.height = num;
		// 	SnapToNewHeightNextFrame = false;
		// }
		// cameraParent.localPosition = Vector3.up * character.height / 2f * characterHeightMultipler;
		// camParentOrigLocalPos = camParent.localPosition;
		// wasCrouching = flag;
		// movement = movementGoal * walkingSpeed * WalkingSpeedMultiplier;
		// movement = base.transform.TransformDirection(movement);
		// movement += Vector3.up * jumpValue;
		// Action<Vector3> onPositionUpdate = BackgroundCamera.OnPositionUpdate;
		// if (onPositionUpdate != null)
		// {
		// 	onPositionUpdate(new Vector3(0f, character.velocity.y, 0f));
		// }
		// RotatingDoor rotatingDoor = WillHitDoor(movement * Singleton<GameMaster>.Instance.GameDeltaTime);
		// if (rotatingDoor == null)
		// {
		// 	if (lastHitRotatingDoor != null)
		// 	{
		// 		lastHitRotatingDoor.PlayerTouchingDoor = false;
		// 		lastHitRotatingDoor = null;
		// 	}
		// }
		// else
		// {
		// 	if (lastHitRotatingDoor != null && lastHitRotatingDoor != rotatingDoor)
		// 	{
		// 		Debug.LogWarning("Player is hitting multiple doors this should not happen!\n" + lastHitRotatingDoor.name + "\n" + rotatingDoor.name);
		// 	}
		// 	lastHitRotatingDoor = rotatingDoor;
		// 	lastHitRotatingDoor.PlayerTouchingDoor = true;
		// }
		// UpdateInAir(!grounded);
		// if (!grounded)
		// {
		// 	gravityMultiplier = Mathf.Lerp(gravityMultiplier, 1f, Singleton<GameMaster>.Instance.GameDeltaTime * gravityFallAcceleration);
		// 	movement *= inAirMovementMultiplier;
		// }
		// else
		// {
		// 	gravityMultiplier = groundedGravityMultiplier;
		// }
		// if (flag)
		// {
		// 	movement *= crouchMovementMultiplier;
		// }
		// if (character.enabled)
		// {
		// 	character.Move((movement + Vector3.up * maxGravity * gravityMultiplier) * Singleton<GameMaster>.Instance.GameDeltaTime);
		// }
		return false;
    }
}