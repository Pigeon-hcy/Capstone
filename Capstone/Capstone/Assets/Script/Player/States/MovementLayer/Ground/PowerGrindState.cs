using UnityEngine;
using SkateGame;
using QFramework;

public class PowerGrindState : GroundMovementState
{
    private float deceleration;
    private float direction;


    public PowerGrindState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
        deceleration = playerModel.Config.Value.powerGrindDeceleration;
        direction = playerModel.PowerGrindDirection.Value;
    }

    public override string GetStateName() => "PowerGrind";

    protected override void EnterGroundMovement()
    {
        // 开始检查反向输入窗口
        StartCheckReverseWindow();
        // 设置方向
        direction = Mathf.Sign(rb.linearVelocity.x);
        if (direction == 0) direction = 1f;
        
        // 播放MMF效果
        if (player.powerGrindEffect != null)
        {
            player.powerGrindEffect.PlayFeedbacks();
        }
    }

    protected override void UpdateGroundMovement()
    {
        float vx = rb.linearVelocity.x;

        // 逐渐减少的速度，保持方向不变
        float newVx = vx - direction * deceleration * Time.deltaTime;

        // 防止越过零点
        if (Mathf.Sign(newVx) != direction || Mathf.Abs(newVx) < 0.01f)
        {
            newVx = 0;
        }

        rb.linearVelocity = new Vector2(newVx, rb.linearVelocity.y);


        if (!inputModel.Trick.Value || Mathf.Abs(rb.linearVelocity.x) <= 0.5f)
        {
            player.stateMachine.SwitchState(StateLayer.Movement, "Idle");
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
}
