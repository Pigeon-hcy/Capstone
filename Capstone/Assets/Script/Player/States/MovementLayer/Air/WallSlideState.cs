using UnityEngine;
using SkateGame;
using QFramework;

public class WallSlideState : AirborneMovementState, ICanGetSystem
{
    private float _wallAngle;

    public WallSlideState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    public override void Enter()
    {
        _wallAngle = playerModel.IsSlidingWall.Value ? playerModel.SlidingWallAngle.Value : playerModel.FgWallAngle.Value;
        playerModel.TargetRotationDeg.Value = _wallAngle;
        playerModel.WallJumpWallNormal.Value = (Vector2)(Quaternion.Euler(0f, 0f, _wallAngle) * Vector2.up);
    }

    protected override void UpdateAirMovement()
    {
        // Refresh angle only while wall data is still valid
        if (playerModel.IsSlidingWall.Value)
            _wallAngle = playerModel.SlidingWallAngle.Value;
        else if (playerModel.IsNearFgWall.Value)
            _wallAngle = playerModel.FgWallAngle.Value;
        playerModel.TargetRotationDeg.Value = _wallAngle;

        if (inputModel.JumpStart.Value && !playerModel.IsIgnoringMovementLayer.Value){
            playerModel.WallJumpWallNormal.Value = (Vector2)(Quaternion.Euler(0f, 0f, _wallAngle) * Vector2.up);
            player.stateMachine.SwitchState<WallJumpState>(StateLayer.Movement);
        }
        else if (CheckFloorBelow() && !IsMovingAwayFromFloor())
            player.stateMachine.SwitchState<LandState>(StateLayer.Movement);
        else if (!playerModel.IsNearFgWall.Value && !playerModel.IsSlidingWall.Value)
            player.stateMachine.SwitchState<AirState>(StateLayer.Movement);
    }

    private bool IsMovingAwayFromFloor()
    {
        Vector2 wallTangent = (Vector2)(Quaternion.Euler(0f, 0f, _wallAngle) * Vector2.right);
        Vector2 wallUp = wallTangent.y >= 0f ? wallTangent : -wallTangent;
        return Vector2.Dot(rb.linearVelocity, wallUp) > playerModel.Config.Value.wallSlideEntrySpeed;
    }

    // World-space downward floor check from the lowest corner, independent of player rotation
    private bool CheckFloorBelow()
    {
        Vector2 origin = player.bottomLeft.position.y < player.bottomRight.position.y
            ? player.bottomLeft.position
            : player.bottomRight.position;
        return this.GetSystem<ICollisionSystem>().CheckFloorBelow(origin);
    }

    public override void Exit()
    {
    }
}
