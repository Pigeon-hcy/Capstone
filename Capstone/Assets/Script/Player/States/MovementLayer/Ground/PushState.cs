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
        player.SendEvent<PushInputEvent>(new PushInputEvent { IsPushing = true });
    }

    protected override void UpdateGroundMovement()
    {
        pushTimer += Time.deltaTime;
        if(pushTimer > 1.5f)
        {
            pushTimer = 0f;
            // TODO: Add push sliding animation
            player.animator.SetTrigger("Push");
        }
        // Switch State
        // Brake
        if(inputModel.BrakeStart.Value)
        {
            player.stateMachine.SwitchState<PowerGrindState>(StateLayer.Movement);
        }
        else if(!inputModel.Push.Value)
        {
            player.stateMachine.SwitchState<MoveState>(StateLayer.Movement);
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
