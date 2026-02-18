using SkateGame;
using UnityEngine;

public class NoActionState : ActionStateBase
{
    public NoActionState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        this.player = player;
        this.rb = rb;
        isLoop = playerModel.Config.Value.isLoopNoAction;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerNoAction;
    }
    protected override void UpdateActionState()
    {
        CheckSwitchAction();
        CheckPushAndPowerGrind();
        if (!playerModel.IsGrounded.Value && playerModel.JumpStarted.Value && inputModel.JumpStart.Value)
        {
            player.stateMachine.SwitchState<TrickAState>(StateLayer.Action);
        }
    }
}   
