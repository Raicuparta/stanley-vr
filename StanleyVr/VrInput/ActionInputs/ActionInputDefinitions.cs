using static Valve.VR.SteamVR_Actions;

namespace StanleyVr.VrInput.ActionInputs;

public static class ActionInputDefinitions
{
    public static readonly BooleanActionInput Menu = new(non_dominant_hand.Menu);

    public static readonly BooleanActionInput Interact = new(dominant_hand.Interact);

    public static readonly BooleanActionInput MenuTabLeft = new(dominant_hand.MenuTabLeft);
    
    public static readonly BooleanActionInput MenuTabRight = new(dominant_hand.MenuTabRight);
    
    public static readonly Vector2ActionInput MenuDirection = new(non_dominant_hand.MenuDirection);

    public static readonly BooleanActionInput MenuInteract= new(dominant_hand.MenuInteract);

    public static readonly Vector2ActionInput Move = new(movement_hand.Move);

    public static readonly Vector2ActionInput Rotate = new(rotation_hand.Rotate);

    public static readonly BooleanActionInput Teleport = new(movement_hand.Teleport);

    public static readonly BooleanActionInput SnapTurnLeft = new(rotation_hand.SnapTurnLeft);

    public static readonly BooleanActionInput SnapTurnRight = new(rotation_hand.SnapTurnRight);

    public static readonly BooleanActionInput Jump = new(rotation_hand.Jump);
    
    public static readonly BooleanActionInput Crouch = new(non_dominant_hand.Crouch);
}