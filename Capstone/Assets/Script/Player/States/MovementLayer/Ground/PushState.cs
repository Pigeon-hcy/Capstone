using SkateGame;
using UnityEngine;
using QFramework;

public class PushState : GroundMovementState
{
    private float pushTimer;
    public PushState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    protected override void EnterGroundMovement()
    {
        pushTimer = 0f;
        playPush();
        player.animator.SetTrigger("Push");
        bool isPushingRight = inputModel.Move.Value.x > 0f || (inputModel.Move.Value.x == 0f && playerModel.IsFacingRight.Value);
        player.SendEvent<PushInputEvent>(new PushInputEvent { IsPushing = true, IsPushingRight = isPushingRight });
    }

    protected override void UpdateGroundMovement()
    {
        pushTimer += Time.deltaTime;
        if(pushTimer > 1f)
        {
            pushTimer = 0f;
            // TODO: Add push sliding animation
            player.animator.SetTrigger("Push");
        }
        if (!inputModel.Push.Value)
        {
            player.stateMachine.SwitchState<PowerGrindState>(StateLayer.Movement);
        }
    }

    protected override void ExitGroundMovement()
    {
        player.SendEvent<PushInputEvent>(new PushInputEvent { IsPushing = false });
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
