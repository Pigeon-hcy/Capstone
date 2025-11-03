using UnityEngine;
using SkateGame;
using QFramework;

public class IdleState : GroundMovementState
{
    public IdleState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    public override string GetStateName() => "Idle";

    protected override void EnterGroundMovement()
    {
        // player.animator.Play("oPlayer@Idle", 0);
        //MMF
        if (player.IdleEffect != null)
        {
            player.IdleEffect.PlayFeedbacks();
        }
    }

    protected override void UpdateGroundMovement()
    {
        float moveInput = inputModel.Move.Value.x;

        /* 状态切换 */
        // 移动 
        if (Mathf.Abs(rb.linearVelocity.x) > 0.01f)
        {
            player.stateMachine.SwitchState(StateLayer.Movement, "Move");
        }
    }

    protected override void ExitGroundMovement()
    {
        if (player.IdleEffect != null)
        {
            player.IdleEffect.StopFeedbacks();
        }
    }
} 