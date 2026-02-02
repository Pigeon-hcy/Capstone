namespace SkateGame
{

    public enum MovementStates
    {
        IdleState = 0    ,
        LandState = 1,
        MoveState = 2,
        AirState = 3,
        JumpState = 4,
        DoubleJumpState = 5,
        PowerGrindState = 6,
        ReverseState = 7,
        PushState = 8,
    }

    public enum ActionStates
    {
        NoActionState = 0,
        GrindState = 1,
        GrabbingState = 2,
        WallRideState = 3,
        TrickAState = 4,
    }
}


