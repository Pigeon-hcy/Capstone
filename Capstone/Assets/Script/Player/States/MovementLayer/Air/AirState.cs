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
        if (inputModel.JumpStart.Value && (playerModel.IsNearFgWall.Value || playerModel.WallJumpGraceTimer.Value > 0f))
        {
            player.stateMachine.SwitchState<WallJumpState>(StateLayer.Movement);
        }
    }
} 