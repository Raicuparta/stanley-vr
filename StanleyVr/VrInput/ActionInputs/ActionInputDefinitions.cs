using static Valve.VR.SteamVR_Actions;

namespace StanleyVr.VrInput.ActionInputs;

public static class ActionInputDefinitions
{
    public static BooleanActionInput Menu {get; private set;}

    public static BooleanActionInput Interact {get; private set;}

    public static BooleanActionInput MenuTabLeft {get; private set;}
    
    public static BooleanActionInput MenuTabRight {get; private set;}
    
    public static Vector2ActionInput MenuDirection {get; private set;}

    public static BooleanActionInput MenuInteract {get; private set;}

    public static Vector2ActionInput Move {get; private set;}

    public static Vector2ActionInput Rotate {get; private set;}

    public static BooleanActionInput Teleport {get; private set;}

    public static BooleanActionInput SnapTurnLeft {get; private set;}

    public static BooleanActionInput SnapTurnRight {get; private set;}

    public static BooleanActionInput Jump {get; private set;}
    
    public static BooleanActionInput Crouch {get; private set;}
    
    public static BooleanActionInput Recenter {get; private set;}

    public static void Initialize()
    {
        Menu = new(non_dominant_hand.Menu);

        Interact = new(dominant_hand.Interact);

        MenuTabLeft = new(dominant_hand.MenuTabLeft);
    
        MenuTabRight = new(dominant_hand.MenuTabRight);
    
        MenuDirection = new(non_dominant_hand.MenuDirection,
            non_dominant_hand.MenuLeft,
            non_dominant_hand.MenuRight,
            non_dominant_hand.MenuUp,
            non_dominant_hand.MenuDown);

        MenuInteract= new(dominant_hand.MenuInteract);

        Move = new(movement_hand.Move,
            movement_hand.MoveLeft,
            movement_hand.MoveRight,
            movement_hand.MoveForward,
            movement_hand.MoveBackwards);

        Rotate = new(rotation_hand.Rotate,
            rotation_hand.TurnLeft,
            rotation_hand.TurnRight);

        Teleport = new(movement_hand.MoveForward);

        SnapTurnLeft = new(rotation_hand.TurnLeft);

        SnapTurnRight = new(rotation_hand.TurnRight);

        Jump = new(rotation_hand.Jump);
    
        Crouch = new(non_dominant_hand.Crouch);
    
        Recenter = new(_default.Recenter);
    }
}