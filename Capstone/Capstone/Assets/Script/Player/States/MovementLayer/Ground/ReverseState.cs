using UnityEngine;
using SkateGame;

public class ReverseState : GroundMovementState
{

    public ReverseState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    public override string GetStateName() => "Reverse";

    protected override void EnterGroundMovement()
    {
        Vector2 velocity = rb.linearVelocity;
        rb.linearVelocity = new Vector2(-velocity.x, velocity.y);
        player.stateMachine.SwitchState(StateLayer.Movement, "Move");


        if (player.ReverseEffect != null)
        {
            player.ReverseEffect.PlayFeedbacks();

        }
    }

    protected override void UpdateGroundMovement()
    {

    }

    protected override void ExitGroundMovement()
    {

    }
}
