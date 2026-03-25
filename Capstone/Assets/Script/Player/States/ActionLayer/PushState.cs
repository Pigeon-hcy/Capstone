using SkateGame;
using UnityEngine;
using QFramework;

public class PushState : ActionStateBase
{
    private float pushTimer;
    private bool pushingRight;
    private bool reversePush = false;
    private float kickDelayTimer = -1f;
    public PushState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        this.player = player;
        this.rb = rb;
        isLoop = playerModel.Config.Value.isLoopPush;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerPush;
    }

    protected override void EnterActionState()
    {
        reversePush = false;
        pushTimer = playerModel.Config.Value.pushKickInterval - playerModel.Config.Value.firstPushKickInterval;
        if(Mathf.Abs(playerModel.PushSpeed.Value) > playerModel.Config.Value.powerGrindStopSpeedThreshold)
        {
            pushingRight = Mathf.Sign(playerModel.PushSpeed.Value) > 0f;
        }
        else
        {
            pushingRight = inputModel.Move.Value.x > 0f || (inputModel.Move.Value.x == 0f && playerModel.IsFacingRight.Value);
        }

        // 是否需要播放push动画和声音
        // 如果刚起步或者刹车过程中尝试起步，就播放
        bool forceKick = playerModel.PowergrindInterrupted.Value;
        playerModel.PowergrindInterrupted.Value = false;
        bool landingHold = inputModel.Push.Value && !inputModel.PushStart.Value
            && !playerModel.WasGrounded.Value && playerModel.IsGrounded.Value;
        if (!landingHold || forceKick || reversePush)
        {
            pushTimer = 0f;
            player.animator.SetTrigger("Push");
            StartKickDelay();
            reversePush = false;
        }
    }

    protected override void UpdateActionState()
    {
        CheckSwitchAction();
        float moveX = inputModel.Move.Value.x;
        if (Mathf.Abs(moveX) > 0.01f)
        {
            bool inputRight = moveX > 0f;
            if (inputRight != pushingRight)
            {
                player.stateMachine.SwitchState<PowerGrindState>(StateLayer.Action);
                return;
            }
        }

        // 如果速度不满则阶梯式push
        pushTimer += Time.deltaTime;
        if (pushTimer >= playerModel.Config.Value.pushKickInterval)
        {
            pushTimer = 0f;
            player.animator.SetTrigger("Push");
            StartKickDelay();
        }

        // Kick delay countdown
        if (kickDelayTimer > 0f)
        {
            kickDelayTimer -= Time.deltaTime;
            if (kickDelayTimer <= 0f)
            {
                kickDelayTimer = -1f;
                playPush();
                player.SendEvent<PushInputEvent>(new PushInputEvent { IsPushing = true, IsPushingRight = pushingRight});
            }
        }
        if (!inputModel.Push.Value || !playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
        }
    }

    protected override void ExitActionState()
    {
        kickDelayTimer = -1f;
        player.SendEvent<PushInputEvent>(new PushInputEvent { IsPushing = false, IsReversing = false });
    }

    private void StartKickDelay()
    {
        kickDelayTimer = playerModel.Config.Value.pushKickDelay;
    }

    // TODO: 让push特效和声音与动画同步
    public void playPush()
    {
        AudioManager.Instance.fmodPlayPush();
        if (player.pushEffectPlayer != null)
        {
            player.pushEffectPlayer.PlayFeedbacks();
        }
    }
}
