using System.Collections.Generic;
using StanleyVr.VrInput.ActionInputs;

namespace StanleyVr.VrInput;

public static class ActionMap
{
	public static Dictionary<string, IActionInput> InputMap;

    public static void Initialize()
    {
	    InputMap = new Dictionary<string, IActionInput>
	    {
		    {ActionNames.Up, ActionInputDefinitions.MenuUp},
		    {ActionNames.Down, ActionInputDefinitions.MenuDown},
		    {ActionNames.Left, ActionInputDefinitions.MenuLeft},
		    {ActionNames.Right, ActionInputDefinitions.MenuRight},
		    {ActionNames.Crouch, ActionInputDefinitions.Crouch},
		    {ActionNames.Use, ActionInputDefinitions.Interact},
		    {ActionNames.Jump, ActionInputDefinitions.Jump},
		    {ActionNames.Start, ActionInputDefinitions.Menu},
		    {ActionNames.MenuTabLeft, ActionInputDefinitions.MenuTabLeft},
		    {ActionNames.MenuTabRight, ActionInputDefinitions.MenuTabRight},
		    {ActionNames.MenuConfirm, ActionInputDefinitions.Interact},
		    {ActionNames.AnyButton, ActionInputDefinitions.Interact},
		    {ActionNames.MenuBack, ActionInputDefinitions.Menu},
		    {ActionNames.MenuOpen, ActionInputDefinitions.Menu},
		    {ActionNames.Movement, ActionInputDefinitions.Move},
		    {ActionNames.View, ActionInputDefinitions.Rotate},
	    };
    }
}