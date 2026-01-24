using SkateGame;
using QFramework;
using UnityEngine;
public abstract class AirborneMovementState : StateBase
{
    protected virtual void UpdateAirMovement(){}
    
    public sealed override void Update()
    {
        float moveInput = inputModel.Move.Value.x;
        player.SendEvent<MoveInputEvent>(new MoveInputEvent { HorizontalInput = moveInput });
        UpdateAirMovement();
        switchGroundMovement();
    }
    private void switchGroundMovement()
    {
        if (!playerModel.WasGrounded.Value && playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState<LandState>(StateLayer.Movement);
        }
    }
}


