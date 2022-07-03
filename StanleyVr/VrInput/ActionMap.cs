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
		    {ActionNames.MenuInteract, ActionInputDefinitions.MenuInteract},
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
	    };
    }
}