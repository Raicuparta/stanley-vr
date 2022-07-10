using UnityEngine;
using Valve.VR;

namespace StanleyVr.VrInput.ActionInputs;

public class BooleanActionInput : ActionInput<SteamVR_Action_Boolean>
{
    public BooleanActionInput(SteamVR_Action_Boolean action) :
        base(action)
    {
    }

    protected override Vector2 GetValue(SteamVR_Input_Sources source)
    {
        return SpecificAction.active && SpecificAction.GetState(source) ? Vector2.one : Vector2.zero;
    }

    protected override bool GetValueUp(SteamVR_Input_Sources source)
    {
        return SpecificAction.GetStateUp(source);
    }

    protected override bool GetValueDown(SteamVR_Input_Sources source)
    {
        return SpecificAction.GetStateDown(source);
    }
}