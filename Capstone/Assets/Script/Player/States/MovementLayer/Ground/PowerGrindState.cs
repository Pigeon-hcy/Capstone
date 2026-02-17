using UnityEngine;
using SkateGame;
using QFramework;

public class PowerGrindState : GroundMovementState
{
    private float grindTimer;

    public PowerGrindState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    protected override void EnterGroundMovement()
    {
        grindTimer = 0f;
        playerModel.PowerGrindStopped.Value = false;
        player.SendEvent<PowerGrindInputEvent>(new PowerGrindInputEvent { IsPowerGrinding = true });
        // 开始检查反向输入窗口
        StartCheckReverseWindow();
        
        // 播放MMF效果
        if (player.powerGrindEffect != null)
            player.powerGrindEffect.PlayFeedbacks();
        playPowerGrind();
    }

    protected override void UpdateGroundMovement()
    {
        grindTimer += Time.deltaTime;
        if (playerModel.PowerGrindStopped.Value || grindTimer >= playerModel.Config.Value.powerGrindMaxDuration)
        {
            player.stateMachine.SwitchState<MoveState>(StateLayer.Movement);
        }

        // CheckReverse();
    }

    protected override void ExitGroundMovement()
    {   
        // 停止MMF效果
        if (player.powerGrindEffect != null)
        {
            player.powerGrindEffect.StopFeedbacks();

        }
        player.SendEvent<PowerGrindInputEvent>(new PowerGrindInputEvent { IsPowerGrinding = false });
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
