using UnityEngine;
using SkateGame;
using QFramework;

public class JumpState : AirborneMovementState
{
    private float jumpTimer;
    private bool jumpEnded;
    private bool isGrindJump;
    public JumpState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    public override void Enter()
    {
        // player.animator.Play("oPlayer@Ollie", 0);
        playerModel.GrindJumpTimer.Value = playerModel.Config.Value.grindJumpIgnoreTime;
        playerModel.CoyoteTimer.Value = 0f;
        jumpTimer = 0f;
        jumpEnded = false;
        playerModel.JumpStarted.Value = false;
        isGrindJump = playerModel.IsGrindJump.Value;
        playerModel.IsGrindJump.Value = false;
        // 立即发送跳跃执行事件
        player.SendEvent<JumpExecuteEvent>(new JumpExecuteEvent { IsJumping = true });

       // 播放MMF效果
        if (player.JumpEffect != null)
        {
            player.JumpEffect.PlayFeedbacks();
        }
        // FOR JERRY'S AUDIO - OLLIE
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
        // GrindJump时自动大跳
        if ((!isGrindJump && inputModel.JumpReleased.Value) || jumpTimer > playerModel.Config.Value.jumpHoldMaxTime)
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
            if (jumpTimer > 0f)
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
        if (playerModel.IsSlidingWall.Value)
        {
            player.stateMachine.SwitchState<WallSlideState>(StateLayer.Movement);
            return;
        }
        if (playerModel.touchingWall.Value)
        {
            // Vector2 wallTangent = (Vector2)(Quaternion.Euler(0f, 0f, playerModel.FgWallAngle.Value) * Vector2.right);
            // if (Mathf.Abs(Vector2.Dot(rb.linearVelocity, wallTangent)) > playerModel.Config.Value.wallSlideEntrySpeed)
            // {   
            player.stateMachine.SwitchState<WallSlideState>(StateLayer.Movement);
            return;
            // }
        }
        if (inputModel.JumpStart.Value && playerModel.IsNearFgWall.Value && playerModel.JumpStarted.Value)
        {
            playerModel.WallJumpWallNormal.Value = (Vector2)(Quaternion.Euler(0f, 0f, playerModel.FgWallAngle.Value) * Vector2.up);
            player.stateMachine.SwitchState<WallJumpState>(StateLayer.Movement);
            return;
        }
    }

    public void playOllie()
    {
        AudioManager.Instance.fmodPlayOllie();
    }
} 