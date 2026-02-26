using UnityEngine;
using SkateGame;
using QFramework;

public class PowerGrindState : ActionStateBase
{
    private bool isInterrupted;
    public PowerGrindState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        this.player = player;
        this.rb = rb;
        isLoop = playerModel.Config.Value.isLoopPowerGrind;
        stateDuration = playerModel.Config.Value.powerGrindDuration;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerPowerGrind;
    }

    protected override void EnterActionState()
    {
        isInterrupted = true;
        player.SendEvent<PowerGrindInputEvent>(new PowerGrindInputEvent { IsPowerGrinding = true, IsInterrupted = isInterrupted });
        // 开始检查反向输入窗口
        StartCheckReverseWindow();
        
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
            player.stateMachine.SwitchState<PushState>(StateLayer.Action);
            return;
        }
        bool hasStopped = Mathf.Abs(playerModel.PushSpeed.Value) < playerModel.Config.Value.powerGrindStopSpeedThreshold;
        bool durationReached = stateTimer >= stateDuration;
        if (hasStopped || durationReached)
        {
            isInterrupted = false;
            if (playerModel.PendingReversePush.Value)
                player.stateMachine.SwitchState<PushState>(StateLayer.Action);
            else
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

    // private void CheckReverse()
    // {
        
    //     if (playerModel.IsCheckingReverseWindow.Value)
    //     {
    //         // 计时
    //         playerModel.ReverseTimer.Value += Time.deltaTime;
    //         if (playerModel.ReverseTimer.Value >= playerModel.Config.Value.reverseInputWindow)
    //         {
    //             playerModel.IsCheckingReverseWindow.Value = false;
    //             return;
    //         }
    //         float currentVelocityX = rb.linearVelocity.x;

    //         // 如果当前有水平速度且输入方向与速度方向相反
    //         if (Mathf.Abs(currentVelocityX) > 1f && Mathf.Abs(inputModel.Move.Value.x) > 0.01f)
    //         {
    //             if (Mathf.Sign(inputModel.Move.Value.x) != Mathf.Sign(currentVelocityX))
    //             {
    //                 player.stateMachine.SwitchState<ReverseState>(StateLayer.Movement);
    //                 playerModel.IsCheckingReverseWindow.Value = false;
    //                 return; // 进入反向状态后直接返回，不处理其他逻辑
    //             }
    //         }
    //     }
    // }
    private void StartCheckReverseWindow()
    {
        playerModel.IsCheckingReverseWindow.Value = true;
        playerModel.ReverseTimer.Value = 0f;
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
