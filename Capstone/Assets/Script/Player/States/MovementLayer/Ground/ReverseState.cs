using UnityEngine;
using SkateGame;

public class ReverseState : GroundMovementState
{

    public ReverseState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    protected override void EnterGroundMovement()
    {
        Vector2 velocity = rb.linearVelocity;
        rb.linearVelocity = new Vector2(-velocity.x, velocity.y);
        player.stateMachine.SwitchState<MoveState>(StateLayer.Movement);


        if (player.ReverseEffect != null)
        {
            player.ReverseEffect.PlayFeedbacks();

        }
        playReverse();
    }

    protected override void UpdateGroundMovement()
    {

    }

    protected override void ExitGroundMovement()
    {

    }

    public void playReverse()
    {
        AudioManager.Instance.fmodPlayReverse();
    }
}
