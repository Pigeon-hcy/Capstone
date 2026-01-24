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
        /* 状态切换 */
        // 停止移动
        if (rb.linearVelocity.x == 0)
        {
            player.stateMachine.SwitchState<IdleState>(StateLayer.Movement);
            pauseMove();
        }
        // PowerGrind
        if (inputModel.BrakeStart.Value)
        {
            player.stateMachine.SwitchState<PowerGrindState>(StateLayer.Movement);
        }
    }

    protected override void ExitGroundMovement()
    {
        if (player.moveEffect != null)
        {
            player.moveEffect.StopFeedbacks();
        }
    }

    public void playMoving()
    {
        AudioManager.Instance.fmodPlayMove();
    }

    public void pauseMove()
    {
        AudioManager.Instance.fmodPauseMove();
    }
} 