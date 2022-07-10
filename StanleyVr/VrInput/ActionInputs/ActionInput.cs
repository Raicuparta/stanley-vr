using UnityEngine;
using Valve.VR;

namespace StanleyVr.VrInput.ActionInputs;

public abstract class ActionInput<TAction> : IActionInput where TAction : ISteamVR_Action_In
{
    protected readonly TAction SpecificAction;

    private bool leftHandedMode = false;
    private bool swapSticks = false;

    protected ActionInput(TAction action)
    {
        SpecificAction = action;
    }

    private SteamVR_Input_Sources HandSource
    {
        get
        {
            var isLeftHanded = leftHandedMode;
            var isSwappedSticks = swapSticks;
            if (SpecificAction.actionSet == SteamVR_Actions.dominant_hand)
                return isLeftHanded ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
            if (SpecificAction.actionSet == SteamVR_Actions.non_dominant_hand)
                return isLeftHanded ? SteamVR_Input_Sources.RightHand : SteamVR_Input_Sources.LeftHand;
            if (SpecificAction.actionSet == SteamVR_Actions.rotation_hand)
                return isSwappedSticks ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
            if (SpecificAction.actionSet == SteamVR_Actions.movement_hand)
                return isSwappedSticks ? SteamVR_Input_Sources.RightHand : SteamVR_Input_Sources.LeftHand;
            return SteamVR_Input_Sources.Any;
        }
    }

    public ISteamVR_Action_In Action => SpecificAction;
    public Vector2 Position => GetValue(HandSource);
    public bool ButtonValue => GetButtonValue(HandSource);
    public bool ButtonUp => GetButtonUp(HandSource);
    public bool ButtonDown => GetButtonDown(HandSource);

    public SteamVR_Input_Sources ActiveSource
    {
        get
        {
            if (HandSource != SteamVR_Input_Sources.Any) return HandSource;

            return Action != null && Action.active ? Action.activeDevice : SteamVR_Input_Sources.Any;
        }
    }

    private bool GetButtonValue(SteamVR_Input_Sources source)
    {
        return Action.active && GetValue(source) != Vector2.zero;
    }

    private bool GetButtonUp(SteamVR_Input_Sources source)
    {
        return Action.active && GetValueUp(source);
    }

    private bool GetButtonDown(SteamVR_Input_Sources source)
    {
        return Action.active && GetValueDown(source);
    }

    protected abstract Vector2 GetValue(SteamVR_Input_Sources source);
    protected abstract bool GetValueUp(SteamVR_Input_Sources source);
    protected abstract bool GetValueDown(SteamVR_Input_Sources source);
}