using UnityEngine;
using Valve.VR;

namespace StanleyVr.VrInput.ActionInputs;

public class Vector2ActionInput : ActionInput<SteamVR_Action_Vector2>
{
    public Vector2ActionInput(SteamVR_Action_Vector2 action, string textureModifier = null) : base(action)
    {
        TextureModifier = textureModifier;
    }

    public string TextureModifier { get; }

    protected override Vector2 GetValue(SteamVR_Input_Sources source)
    {
        return SpecificAction.GetAxis(source);
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