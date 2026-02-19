using UnityEngine;
using SkateGame;
using QFramework;

public class MoveState : GroundMovementState
{

    public MoveState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    protected override void EnterGroundMovement()
    {
        // player.animator.Play("oPlayer@Push", 0);
        if (player.moveEffect != null)
        {
            player.moveEffect.PlayFeedbacks();
        }
        //playMoving();
    }

    protected override void UpdateGroundMovement()
    {
        if (Mathf.Abs(rb.linearVelocity.x) < 0.01f)
        {
            player.stateMachine.SwitchState<IdleState>(StateLayer.Movement);
            pauseMove();
        }
    }

    protected override void ExitGroundMovement()
    {
        if (player.moveEffect != null)
        {
            player.moveEffect.StopFeedbacks();
        }
    }

    public void pauseMove()
    {
        AudioManager.Instance.fmodPauseMove();
    }
} 