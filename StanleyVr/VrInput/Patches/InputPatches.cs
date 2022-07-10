using System.IO;
using HarmonyLib;
using InControl;
using StanleyVr.VrInput.ActionInputs;
using UnityEngine;
using Valve.VR;
using Object = UnityEngine.Object;

namespace StanleyVr.VrInput.Patches;

[HarmonyPatch]
public static class InputPatches
{
	private static InControlInputModule inputModule;
	
	[HarmonyPostfix]
	[HarmonyPatch(typeof(StanleyActions), MethodType.Constructor)]
	private static void SetUpMenuInputs(StanleyActions __instance)
	{
		inputModule = Object.FindObjectOfType<InControlInputModule>();
		inputModule.SubmitAction = __instance.CreatePlayerAction(ActionNames.MenuInteract);
		inputModule.CancelAction = __instance.MenuBack;
		inputModule.direction = __instance.CreateTwoAxisPlayerAction(
			__instance.MoveLeft,
			__instance.MoveRight,
			__instance.MoveBackward,
			__instance.MoveForward);
	}
	
	[HarmonyPrefix]
	[HarmonyPatch(typeof(TwoAxisInputControl), nameof(TwoAxisInputControl.UpdateWithAxes))]
	private static void ReadVrTwoAxisInput(TwoAxisInputControl __instance, ref float x, ref float y)
	{
		IActionInput actionInput;

		// Two axis actions don't have names in the game code, so we assign them manually.
		if (__instance == GameMaster.Instance.stanleyActions.View) actionInput = ActionInputDefinitions.Rotate;
		else if (__instance == GameMaster.Instance.stanleyActions.Movement) actionInput = ActionInputDefinitions.Move;
		else if (__instance == inputModule.direction) actionInput = ActionInputDefinitions.MenuDirection;
		else return;

		x = actionInput.Position.x;
		y = actionInput.Position.y;
	}

	private static string GetActionName(OneAxisInputControl inputControl)
	{
		if (inputControl is not PlayerAction action) return default;
		
		var actionName = action.Name;

		return !ActionMap.InputMap.ContainsKey(actionName) ? default : actionName;
	}
	
	[HarmonyPrefix]
	[HarmonyPatch(typeof(OneAxisInputControl), nameof(OneAxisInputControl.UpdateWithState))]
	private static void ReadVrOneAxisInputState(OneAxisInputControl __instance, ref bool state)
	{
		var actionName = GetActionName(__instance);
		if (string.IsNullOrEmpty(actionName)) return;

		state = ActionMap.InputMap[actionName].ButtonValue;
	}
	
	[HarmonyPrefix]
	[HarmonyPatch(typeof(OneAxisInputControl), nameof(OneAxisInputControl.UpdateWithValue))]
	[HarmonyPatch(typeof(OneAxisInputControl), nameof(OneAxisInputControl.UpdateWithRawValue))]
	[HarmonyPatch(typeof(OneAxisInputControl), nameof(OneAxisInputControl.SetValue))]
	private static void ReadVrOneAxisInputValue(OneAxisInputControl __instance, ref float value)
	{
		var actionName = GetActionName(__instance);
		if (string.IsNullOrEmpty(actionName)) return;
		
		value = ActionMap.InputMap[actionName].ButtonValue ? 1 : 0;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GameMaster), nameof(GameMaster.RegisterInputDeviceTypeChange))]
	private static void ForceXboxDeviceType(ref GameMaster.InputDevice newDeviceType)
	{
		// Force it to show Xbox button icons.
		newDeviceType = GameMaster.InputDevice.GamepadXBOXOneOrGeneric;
	}


	// TODO check if it's better to use simplified controls.
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StanleyActions), nameof(StanleyActions.UseAction), MethodType.Getter)]
	private static bool ForceUseActionToUse(ref PlayerAction __result, StanleyActions __instance)
	{
		__result = __instance.Use;
		return false;
	}
	
	// TODO check if it's better to use simplified controls.
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StanleyActions), nameof(StanleyActions.JumpAction), MethodType.Getter)]
	private static bool ForceJumpActionToJump(ref PlayerAction __result, StanleyActions __instance)
	{
		__result = __instance.Jump;
		return false;
	}
}