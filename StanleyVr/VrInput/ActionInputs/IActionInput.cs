using UnityEngine;
using Valve.VR;

namespace StanleyVr.VrInput.ActionInputs;

public interface IActionInput
{
    ISteamVR_Action_In Action { get; }
    Vector2 Position { get; }
    bool ButtonValue { get; }
    bool ButtonUp { get; }
    bool ButtonDown { get; }
    SteamVR_Input_Sources ActiveSource { get; }
}