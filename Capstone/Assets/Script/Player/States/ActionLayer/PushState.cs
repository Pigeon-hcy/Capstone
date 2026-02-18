using SkateGame;
using UnityEngine;
using QFramework;

public class PushState : ActionStateBase
{
    private float pushTimer;
    private bool pushingRight; 
    private bool reversing = false;
    public PushState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        this.player = player;
        this.rb = rb;
        isLoop = playerModel.Config.Value.isLoopPush;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerPush;
    }

    protected override void EnterActionState()
    {
        pushTimer = 0f;
        pushingRight = inputModel.Move.Value.x > 0f || (inputModel.Move.Value.x == 0f && playerModel.IsFacingRight.Value);
        playPush();
        player.animator.SetTrigger("Push");
        player.SendEvent<PushInputEvent>(new PushInputEvent { IsPushing = true, IsPushingRight = pushingRight, IsReversing = reversing });
        reversing = false;
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
                reversing = true;
                playerModel.PushSpeedBeforeReverse.Value = Mathf.Abs(playerModel.PushSpeed.Value);
                player.stateMachine.SwitchState<PowerGrindState>(StateLayer.Action);
                return;
            }
        }
        pushTimer += Time.deltaTime;
        if (pushTimer > 1f)
        {
            pushTimer = 0f;
            // TODO: Add push sliding animation
            player.animator.SetTrigger("Push");
        }
        if (!inputModel.Push.Value || !playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
        }
    }

    protected override void ExitActionState()
    {
        player.SendEvent<PushInputEvent>(new PushInputEvent { IsPushing = false, IsReversing = false });
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
