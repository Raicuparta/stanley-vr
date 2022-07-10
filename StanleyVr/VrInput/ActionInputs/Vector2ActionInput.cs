using UnityEngine;
using Valve.VR;

namespace StanleyVr.VrInput.ActionInputs;

public class Vector2ActionInput : ActionInput<SteamVR_Action_Vector2>
{
    private readonly SteamVR_Action_Boolean dpadLeft;
    private readonly SteamVR_Action_Boolean dpadRight;
    private readonly SteamVR_Action_Boolean dpadUp;
    private readonly SteamVR_Action_Boolean dpadDown;
    
    public Vector2ActionInput(SteamVR_Action_Vector2 action,
        SteamVR_Action_Boolean dpadLeft = null,
        SteamVR_Action_Boolean dpadRight = null,
        SteamVR_Action_Boolean dpadUp = null,
        SteamVR_Action_Boolean dpadDown = null) : base(action)
    {
        this.dpadLeft = dpadLeft;
        this.dpadRight = dpadRight;
        this.dpadUp = dpadUp;
        this.dpadDown = dpadDown;
    }

    private static float GetDirectionValue(SteamVR_Action_Boolean positiveAction,
        SteamVR_Action_Boolean negativeAction,
        SteamVR_Input_Sources source)
    {
        return GetSingleAxisValue(positiveAction, source) - GetSingleAxisValue(negativeAction, source);
    }
    
    private static float GetSingleAxisValue(SteamVR_Action_Boolean booleanAction, SteamVR_Input_Sources source)
    {
        if (booleanAction == null || !booleanAction.active) return 0f;

        return booleanAction.GetState(source) ? 1f : 0f;
    }

    private Vector2 GetDpadAxis(SteamVR_Input_Sources source)
    {
        return new Vector2(GetDirectionValue(dpadRight, dpadLeft, source),
            GetDirectionValue(dpadUp, dpadDown, source));
    }

    private Vector2 GetPositionAxis(SteamVR_Input_Sources source)
    {
        return !SpecificAction.active ? Vector2.zero : SpecificAction.GetAxis(source);
    }

    protected override Vector2 GetValue(SteamVR_Input_Sources source)
    {
        var dpad = GetDpadAxis(source);
        return dpad.sqrMagnitude > 0 ? dpad : GetPositionAxis(source);
    }

    protected override bool GetValueUp(SteamVR_Input_Sources source)
    {
        return false;
    }

    protected override bool GetValueDown(SteamVR_Input_Sources source)
    {
        return false;
    }
}