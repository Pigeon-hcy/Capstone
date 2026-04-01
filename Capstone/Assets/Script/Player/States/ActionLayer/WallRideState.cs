using UnityEngine;
using SkateGame;
using QFramework;

public class WallRideState : ActionStateBase
{
    private float normalG;
    private float onWallGravity = 0.1f;
    private Wall lastWall;  // 记录上一个墙体
    /// <summary>进入滑墙时选中的 CM 偏转方向；退出时用同一套 Exit，避免 CurrentBgWall 已清空。</summary>
    private bool wallRideCameraIsLeftToRight;

    public WallRideState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopBgWallRide;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerBgWallRide;
        normalG = playerModel.Config.Value.normalG;
    }

    protected override void EnterActionState()
    {
        playerModel.CurrentGravityScale.Value = onWallGravity;
        lastWall = playerModel.CurrentBgWall.Value;  // 记录当前墙体

        if (player.WallRideEffect != null)
        {
            player.WallRideEffect.PlayFeedbacks();
        }
        if (player.cmControls != null && player.cmControls.Length > 0)
        {
            wallRideCameraIsLeftToRight = playerModel.IsFacingRight.Value;
            foreach (var cmControl in player.cmControls)
            {
                if (wallRideCameraIsLeftToRight)
                    cmControl.EnterSpecialCameraPositionWhenWallRideLeftToRight();
                else
                    cmControl.EnterSpecialCameraPositionWhenWallRideRightToLeft();
            }
        }
        // FOR JERRY'S AUDIO - WALL RIDE
        playWallRide();
    }

    protected override void UpdateActionState()
    {
        // 检测是否切换到了另一个墙体
        if (playerModel.CurrentBgWall.Value != null && playerModel.CurrentBgWall.Value != lastWall)
        {
            Debug.Log($"WallRideState: 切换到新墙体 {playerModel.CurrentBgWall.Value.name}，刷新冷却时间");
            stateTimer = 0f;  // 重置墙骑计时器
            lastWall = playerModel.CurrentBgWall.Value;  // 更新记录的墙体
            playerModel.CurrentGravityScale.Value = onWallGravity;  // 重置重力
        }
        playerModel.CurrentGravityScale.Value = Mathf.Lerp(onWallGravity, normalG, stateTimer / playerModel.Config.Value.BgWallrideDuration);
        if (playerModel.CurrentBgWall.Value == null || !inputModel.Grind.Value || inputModel.JumpStart.Value)
        {
            playerModel.CurrentGravityScale.Value = normalG;
            stateTimer = 0f;
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
            player.stateMachine.SwitchState<JumpState>(StateLayer.Movement);
        }
    }

    protected override void ExitActionState()
    {
        playerModel.CurrentGravityScale.Value = normalG;
        playerModel.WallRideCooldownTimer.Value = playerModel.Config.Value.BgWallRideCooldown;
        if (player.WallRideEffect != null)
        {
            player.WallRideEffect.StopFeedbacks();
        }
        if (player.cmControls != null && player.cmControls.Length > 0)
        {
            foreach (var cmControl in player.cmControls)
            {
                if (wallRideCameraIsLeftToRight)
                    cmControl.ExitSpecialCameraPositionWhenWallRideLeftToRight();
                else
                    cmControl.ExitSpecialCameraPositionWhenWallRideRightToLeft();
            }
        }
        // FOR JERRY'S AUDIO - WALL RIDE STOP
        pauseWallRide();
    }

    public void playWallRide()
    {
        AudioManager.Instance.fmodPlayWallRide();
        // FOR JERRY'S AUDIO - LANDING
        AudioManager.Instance.fmodPlayLanding();
    }
    public void pauseWallRide()
    {
        AudioManager.Instance.fmodPauseWallRide();
    }
}