using UnityEngine;
using SkateGame;
using QFramework;

public class DoubleJumpState : AirborneMovementState
{
    private float jumpTimer;
    public DoubleJumpState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    public override string GetStateName() => "DoubleJump";

    public override void Enter()
    {
        // player.animator.Play("oPlayer@KickFlip", 0);
        playerModel.CanDoubleJump.Value = false;
        // 直接跳起来
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, playerModel.Config.Value.maxJumpForce);
        jumpTimer = 0f;
        // 发送技巧执行事件给系统，更新UIController
        player.SendEvent<JumpExecuteEvent>();
        //MMF
        if (player.DoubleJumpEffect != null)
        {
            player.DoubleJumpEffect.PlayFeedbacks();
        }
    }

    protected override void UpdateAirMovement()
    {   
        UpdateGrindJumpTimer();
        UpdateJumpTimer();
    }

    public override void Exit()
    {
        //MMF
        if (player.DoubleJumpEffect != null)
        {
            player.DoubleJumpEffect.StopFeedbacks();
        }
        playerModel.GrindJumpTimer.Value = 0f;
    }
        private void UpdateGrindJumpTimer()
    {
        // 更新轨道跳计时器
        if (playerModel.GrindJumpTimer.Value > 0f)
        {
            playerModel.GrindJumpTimer.Value -= Time.deltaTime;
        }
    }
    private void UpdateJumpTimer()
    {   
        if (jumpTimer < playerModel.DoubleJumpDuration.Value)
        {
            jumpTimer += Time.deltaTime;
        }
        else
        {
            player.stateMachine.SwitchState(StateLayer.Movement, "Air");
        }
    }
} 