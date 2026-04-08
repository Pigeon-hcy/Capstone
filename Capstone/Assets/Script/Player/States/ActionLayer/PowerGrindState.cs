using UnityEngine;
using SkateGame;
using QFramework;

public class PowerGrindState : ActionStateBase
{
    private bool isInterrupted;
    private bool brakingRight; // direction at entry, used for reverse-hold check
    public PowerGrindState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        this.player = player;
        this.rb = rb;
        isLoop = true;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerPowerGrind;
    }

    protected override void EnterActionState()
    {
        brakingRight = playerModel.PushSpeed.Value > 0;
        isInterrupted = true;
        player.SendEvent<PowerGrindInputEvent>(new PowerGrindInputEvent { IsPowerGrinding = true, IsInterrupted = isInterrupted });
        // 播放MMF效果
        if (player.powerGrindEffect != null)
            player.powerGrindEffect.PlayFeedbacks();
        playPowerGrind();
    }

    protected override void UpdateActionState()
    {
        
        CheckSwitchAction();
        if (!playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
            return;
        }
        bool isReversing = inputModel.Move.Value.x != 0f && (playerModel.IsFacingRight.Value != (inputModel.Move.Value.x > 0));
        if (inputModel.PushStart.Value && playerModel.IsGrounded.Value && !isReversing)
        {
            playerModel.PowergrindInterrupted.Value = true;
            player.stateMachine.SwitchState<PushState>(StateLayer.Action);
            return;
        }
        float moveX = inputModel.Move.Value.x;
        bool holdingReverse = Mathf.Abs(moveX) > 0.01f && (moveX > 0) != brakingRight;
        if (!holdingReverse)
        {
            isInterrupted = true; // interrupted by user, no walk delay
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
            return;
        }
        if (Mathf.Abs(playerModel.PushSpeed.Value) < playerModel.Config.Value.powerGrindStopSpeedThreshold)
        {
            isInterrupted = false;
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
            return;
        }
    }

    protected override void ExitActionState()
    {   
        // 停止MMF效果
        if (player.powerGrindEffect != null)
        {
            player.powerGrindEffect.StopFeedbacks();

        }
        player.SendEvent<PowerGrindInputEvent>(new PowerGrindInputEvent { IsPowerGrinding = false, IsInterrupted = isInterrupted });
        pausePowerGrind();
    }


    public void playPowerGrind()
    {
        AudioManager.Instance.fmodPlayPowerGrind();
    }
    public void pausePowerGrind()
    {
        AudioManager.Instance.fmodPausePowerGrind();
    }
}
