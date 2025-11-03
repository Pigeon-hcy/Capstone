using UnityEngine;
using SkateGame;
using QFramework;

public class LandState : GroundMovementState
{
    private float landTimer;
    private bool isDoubleJumpLand;

    public LandState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    public override string GetStateName() => "Land";

    protected override void EnterGroundMovement()
    {
        playerModel.CurrentBulletCount.Value = playerModel.Config.Value.bulletMaxCount;
        isDoubleJumpLand = playerModel.CanDoubleJump.Value;
        playerModel.CanDoubleJump.Value = true;
        landTimer = 0f;
    }

    protected override void UpdateGroundMovement()
    {
        UpdateLandTimer();
    }

    protected override void ExitGroundMovement()
    {
    }

    private void UpdateLandTimer()
    {
        if(isDoubleJumpLand && landTimer < playerModel.LandDuration.Value ||
            !isDoubleJumpLand && landTimer < playerModel.DoubleJumpLandDuration.Value)
        {
            landTimer += Time.deltaTime;
        }
        else
        {
            if(rb.linearVelocity.x == 0)
            {
                player.stateMachine.SwitchState(StateLayer.Movement, "Idle");
            }
            else
            {
                player.stateMachine.SwitchState(StateLayer.Movement, "Move");
            }
        }
    }
} 