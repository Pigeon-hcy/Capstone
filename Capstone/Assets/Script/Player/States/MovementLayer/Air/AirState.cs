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
        if (playerModel.CanDoubleJump.Value)
        {
            // player.animator.Play("oPlayer@OllieAirborne", 0);
            player.animator.SetBool("CanDoubleJump", true);
        }
        else // player.animator.Play("oPlayer@KickFlipAirborne", 0);
        {
            player.animator.SetBool("CanDoubleJump", false);
        }
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
        if (inputModel.JumpStart.Value && (playerModel.IsNearFgWall.Value || playerModel.WallJumpGraceTimer.Value > 0f))
        {
            player.stateMachine.SwitchState<WallJumpState>(StateLayer.Movement);
        }
    }
} 