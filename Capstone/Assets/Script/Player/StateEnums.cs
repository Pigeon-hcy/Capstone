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
        ReverseState = 6,
        WallJumpState = 7,
        WallSlideState = 8,
    }

    public enum ActionStates
    {
        NoActionState = 0,
        GrindState = 1,
        GrabbingState = 2,
        WallRideState = 3,
        TrickAState = 4,
        PushState = 5,
        PowerGrindState = 6,
        HitState = 7,
        CruiseState = 8,
    }
}


