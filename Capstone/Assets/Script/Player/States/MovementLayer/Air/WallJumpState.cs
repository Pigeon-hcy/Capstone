using UnityEngine;
using SkateGame;
using QFramework;

public class WallJumpState : AirborneMovementState
{
    private float jumpTimer;
    public WallJumpState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    public override void Enter()
    {
        if (playerModel.CurrentActionState.Value == ActionStates.HitState)
        {
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
        }
        if (playerModel.WallJumpGraceTimer.Value > 0f)
        {
            playerModel.WallJumpGraceTimer.Value = 0f;
            player.SendEvent<WallJumpExecuteEvent>(new WallJumpExecuteEvent { IsGraceWallJump = true });
        }
        else
        {
            player.SendEvent<WallJumpExecuteEvent>(new WallJumpExecuteEvent { IsGraceWallJump = false });
        }
        playerModel.GrindJumpTimer.Value = playerModel.Config.Value.grindJumpIgnoreTime;
        jumpTimer = 0f;
        playerModel.JumpStarted.Value = false;

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
        if (inputModel.JumpStart.Value && (playerModel.IsNearFgWall.Value || playerModel.WallJumpGraceTimer.Value > 0f))
        {
            // 重新walljump
            player.stateMachine.ReenterCurrentState(StateLayer.Movement);
        }
    }

    public void playOllie()
    {
        AudioManager.Instance.fmodPlayOllie();
        AudioManager.Instance.fmodPauseMove();
    }
} 