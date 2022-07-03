using HarmonyLib;
using InControl;
using Object = UnityEngine.Object;

namespace StanleyVr.VrInput.Patches;

public static class InputPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(StanleyActions), MethodType.Constructor)]
	private static void SetUpMenuInputs(StanleyActions __instance)
	{
		var inputModule = Object.FindObjectOfType<InControlInputModule>();
		inputModule.SubmitAction = GameMaster.Instance.stanleyActions.UseAction; // TODO make a new action for this.
		inputModule.CancelAction = GameMaster.Instance.stanleyActions.MenuBack;
		inputModule.direction = GameMaster.Instance.stanleyActions.Movement; // TODO make a new action for this.
	}
	
	[HarmonyPrefix]
	[HarmonyPatch(typeof(TwoAxisInputControl), nameof(TwoAxisInputControl.UpdateWithAxes))]
	private static void ReadVrTwoAxisInput(TwoAxisInputControl __instance, ref float x, ref float y)
	{
		string actionName;

		// Two axis actions don't have names in the game code, so we assign them manually.
		if (__instance == GameMaster.Instance.stanleyActions.View) actionName = ActionNames.View;
		else if (__instance == GameMaster.Instance.stanleyActions.Movement) actionName = ActionNames.Movement;
		else return;
		
		var vrInput = ActionMap.InputMap[actionName];
		x = vrInput.Position.x;
		y = vrInput.Position.y;
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