using UnityEngine;
using SkateGame;
using QFramework;

public class WallRideState : ActionStateBase
{
    private float normalGravity;
    private float onWallGravity = 0.1f;
    private Wall lastWall;  // 记录上一个墙体
    
    public WallRideState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopWallRide;
        ignoreMovementLayerDuration = playerModel.Config.Value.ignoreMovementLayerDurationWallRide;
    }

    public override string GetStateName() => "WallRide";

    protected override void EnterActionState()
    {

        normalGravity = rb.gravityScale;
        rb.gravityScale = onWallGravity;
        lastWall = playerModel.CurrentWall.Value;  // 记录当前墙体

        if (player.WallRideEffect != null)
        {
            player.WallRideEffect.PlayFeedbacks();
        }
    }

    protected override void UpdateActionState()
    {
        // 检测是否切换到了另一个墙体
        if (playerModel.CurrentWall.Value != null && playerModel.CurrentWall.Value != lastWall)
        {
            Debug.Log($"WallRideState: 切换到新墙体 {playerModel.CurrentWall.Value.name}，刷新冷却时间");
            stateTimer = 0f;  // 重置墙骑计时器
            lastWall = playerModel.CurrentWall.Value;  // 更新记录的墙体
            rb.gravityScale = onWallGravity;  // 重置重力
        }
        
        rb.gravityScale = Mathf.Lerp(onWallGravity, normalGravity, stateTimer / playerModel.Config.Value.wallrideDuration);
        if (playerModel.CurrentWall.Value == null || !inputModel.Grind.Value)
        {   
            rb.gravityScale = normalGravity;
            stateTimer = 0f;
            player.stateMachine.SwitchState(StateLayer.Action, "None");
            player.stateMachine.SwitchState(StateLayer.Movement, "Jump");
        }
    }

    protected override void ExitActionState()
    {   
        rb.gravityScale = normalGravity;
        playerModel.WallRideCooldownTimer.Value = playerModel.Config.Value.wallRideCooldown;
        if (player.WallRideEffect != null)
        {
            player.WallRideEffect.StopFeedbacks();
        }
    }
}