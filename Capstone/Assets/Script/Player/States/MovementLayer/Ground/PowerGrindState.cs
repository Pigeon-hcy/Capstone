using UnityEngine;
using SkateGame;
using QFramework;

public class PowerGrindState : GroundMovementState
{

    public PowerGrindState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    public override string GetStateName() => "PowerGrind";

    protected override void EnterGroundMovement()
    {
        player.SendEvent<PowerGrindInputEvent>(new PowerGrindInputEvent { IsPowerGrinding = true });
        // 开始检查反向输入窗口
        StartCheckReverseWindow();
        
        // 播放MMF效果
        if (player.powerGrindEffect != null)
        {
            player.powerGrindEffect.PlayFeedbacks();
        }
        playPowerGrind();
    }

    protected override void UpdateGroundMovement()
    {
        if (!inputModel.TrickA.Value)
        {
            if (Mathf.Abs(rb.linearVelocity.x) <= 0.5f)
            {
                player.stateMachine.SwitchState(StateLayer.Movement, "Idle");
            }
            else
            {
                player.stateMachine.SwitchState(StateLayer.Movement, "Move");
            }
        }
        // 检测反向输入
        CheckReverse();
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

    private void CheckReverse()
    {
        
        if (playerModel.IsCheckingReverseWindow.Value)
        {
            // 计时
            playerModel.ReverseTimer.Value += Time.deltaTime;
            if (playerModel.ReverseTimer.Value >= playerModel.Config.Value.reverseInputWindow)
            {
                playerModel.IsCheckingReverseWindow.Value = false;
                return;
            }
            float currentVelocityX = rb.linearVelocity.x;

            // 如果当前有水平速度且输入方向与速度方向相反
            if (Mathf.Abs(currentVelocityX) > 1f && Mathf.Abs(inputModel.Move.Value.x) > 0.01f)
            {
                if (Mathf.Sign(inputModel.Move.Value.x) != Mathf.Sign(currentVelocityX))
                {
                    player.stateMachine.SwitchState(StateLayer.Movement, "Reverse");
                    playerModel.IsCheckingReverseWindow.Value = false;
                    return; // 进入反向状态后直接返回，不处理其他逻辑
                }
            }
        }
    }
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
