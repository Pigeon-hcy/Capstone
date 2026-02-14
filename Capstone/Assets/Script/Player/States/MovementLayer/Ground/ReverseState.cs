using UnityEngine;
using SkateGame;
using QFramework;

public class ReverseState : GroundMovementState, ICanSendEvent
{

    public ReverseState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    protected override void EnterGroundMovement()
    {
        player.SendEvent<ReverseInputEvent>();
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
