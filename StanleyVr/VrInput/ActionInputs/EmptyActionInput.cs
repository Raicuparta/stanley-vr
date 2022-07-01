using UnityEngine;
using Valve.VR;

namespace StanleyVr.VrInput.ActionInputs;

public class EmptyActionInput : ActionInput<ISteamVR_Action_In>
{
    public EmptyActionInput(string texturePath = null) : base(null)
    {
        TexturePath = texturePath;
    }

    public string TexturePath { get; }

    protected override Vector2 GetValue(SteamVR_Input_Sources source)
    {
        return Vector2.zero;
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