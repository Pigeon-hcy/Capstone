using UnityEngine;
using SkateGame;
using QFramework;

public class AirState : AirborneMovementState
{ 

    public AirState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    public override void Enter()
    {
        player.animator.SetBool("CanDoubleJump", true);
    }

    protected override void UpdateAirMovement()
    {
        StateChange();
    }

    public override void Exit()
    {
    }
    
    // state change
    private void StateChange()
    {
        // Case 1: hit wall from ground - require along-wall speed threshold
        if (playerModel.IsSlidingWall.Value)
        {
            // Vector2 wallTangent = (Vector2)(Quaternion.Euler(0f, 0f, playerModel.SlidingWallAngle.Value) * Vector2.right);
            // if (Mathf.Abs(Vector2.Dot(rb.linearVelocity, wallTangent)) > playerModel.Config.Value.wallSlideEntrySpeed)
            // {
            player.stateMachine.SwitchState<WallSlideState>(StateLayer.Movement);
            return;
            // }
        }
        // Case 2: airborne touching wall - enter immediately, no threshold
        if (playerModel.touchingWall.Value)
        {
            player.stateMachine.SwitchState<WallSlideState>(StateLayer.Movement);
            return;
        }
        bool wantJump = inputModel.JumpStart.Value && !playerModel.IsIgnoringMovementLayer.Value;
        if (wantJump && playerModel.IsNearFgWall.Value)
        {
            playerModel.WallJumpWallNormal.Value = (Vector2)(Quaternion.Euler(0f, 0f, playerModel.FgWallAngle.Value) * Vector2.up);
            player.stateMachine.SwitchState<WallJumpState>(StateLayer.Movement);
            return;
        }
        if (inputModel.JumpStart.Value && !playerModel.IsIgnoringMovementLayer.Value && playerModel.CoyoteTimer.Value > 0f)
            player.stateMachine.SwitchState<JumpState>(StateLayer.Movement);
    }
} 