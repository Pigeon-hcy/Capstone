using UnityEngine;
using SkateGame;
using QFramework;

public class JumpState : AirborneMovementState
{
    private float jumpTimer;
    private bool jumpEnded;
    public JumpState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    public override void Enter()
    {
        // player.animator.Play("oPlayer@Ollie", 0);
        playerModel.GrindJumpTimer.Value = playerModel.Config.Value.grindJumpIgnoreTime;
        jumpTimer = 0f;
        jumpEnded = false;
        playerModel.JumpStarted.Value = false;
        // 立即发送跳跃执行事件
        player.SendEvent<JumpExecuteEvent>(new JumpExecuteEvent { IsJumping = true });

       // 播放MMF效果
        if (player.JumpEffect != null)
        {
            player.JumpEffect.PlayFeedbacks();
        }
        playOllie();
    }

    protected override void UpdateAirMovement()
    {
        StateChange();
        UpdateGrindJumpTimer();
        CheckEndJump();
        UpdateJumpTimer();
    }

    public override void Exit()
    {
        // 播放MMF效果
        if (player.JumpEffect != null)
        {
            player.JumpEffect.StopFeedbacks();
        }
        playerModel.GrindJumpTimer.Value = 0f;
        // 保底：发送跳跃结束事件
        if (!jumpEnded) player.SendEvent<JumpExecuteEvent>(new JumpExecuteEvent { IsJumping = false });
    }

    private void CheckEndJump()
    {
        if (jumpEnded) return;
        if (inputModel.JumpReleased.Value || jumpTimer > playerModel.Config.Value.jumpHoldMaxTime)
        {
            player.SendEvent<JumpExecuteEvent>(new JumpExecuteEvent { IsJumping = false });
            jumpEnded = true;
        }
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
        if (jumpTimer < playerModel.JumpDuration.Value)
        {
            jumpTimer += Time.deltaTime;
            if (playerModel.CanDoubleJump.Value && jumpTimer > 0f)
            {
                playerModel.JumpStarted.Value = true;
            }
        }
        else
        {
            player.stateMachine.SwitchState<AirState>(StateLayer.Movement);
        }
    }
    // state change
    private void StateChange()
    {
        if (inputModel.JumpStart.Value && (playerModel.IsNearFgWall.Value || playerModel.WallJumpGraceTimer.Value > 0f))
        {
            player.stateMachine.SwitchState<WallJumpState>(StateLayer.Movement);
        }
    }

    public void playOllie()
    {
        AudioManager.Instance.fmodPlayOllie();
    }
} 