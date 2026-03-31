using SkateGame;
using UnityEngine;
using QFramework;

/*
    具体加速逻辑迁移至animationEvent以适配动画节奏
*/
public class PushState : ActionStateBase
{
    private bool pushingRight;
    private float pushTimer;
    public PushState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        this.player = player;
        this.rb = rb;
        isLoop = playerModel.Config.Value.isLoopPush;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerPush;
    }

    protected override void EnterActionState()
    {
        pushTimer = playerModel.Config.Value.pushKickInterval - playerModel.Config.Value.firstPushKickInterval;
        bool isMounting = Mathf.Abs(playerModel.PushSpeed.Value) <= playerModel.Config.Value.pushMountSpeed;
        if (!isMounting)
            pushingRight = playerModel.PushSpeed.Value > 0f;
        else
        {
            pushingRight = inputModel.Move.Value.x > 0f || (inputModel.Move.Value.x == 0f && playerModel.IsFacingRight.Value);
            player.SendEvent<MountEvent>(new MountEvent { IsMountingRight = pushingRight });
        }

        bool forceKick = playerModel.PowergrindInterrupted.Value;
        playerModel.PowergrindInterrupted.Value = false;
        bool landingHold = inputModel.Push.Value && !inputModel.PushStart.Value
            && !playerModel.WasGrounded.Value && playerModel.IsGrounded.Value;
        if (!landingHold || forceKick)
        {
            pushTimer = 0f;
            player.animator.SetTrigger("Push");
        }
    }

    protected override void UpdateActionState()
    {
        CheckSwitchAction();

        // 阶梯式push
        pushTimer += Time.deltaTime;
        if (pushTimer >= playerModel.Config.Value.pushKickInterval)
        {
            pushTimer = 0f;
            player.animator.SetTrigger("Push");
        }
        float moveX = inputModel.Move.Value.x;
        if (Mathf.Abs(moveX) > 0.01f && (moveX > 0f) != pushingRight)
        {
            player.stateMachine.SwitchState<PowerGrindState>(StateLayer.Action);
            return;
        }
        if (!playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
            return;
        }
        if (!inputModel.Push.Value)
        {
            if (Mathf.Abs(playerModel.PushSpeed.Value) > playerModel.Config.Value.powerGrindStopSpeedThreshold)
                player.stateMachine.SwitchState<CruiseState>(StateLayer.Action);
            else
                player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
        }
    }

    protected override void ExitActionState()
    {
        player.SendEvent<PushKickEvent>(new PushKickEvent { IsPushing = false });
    }
}
