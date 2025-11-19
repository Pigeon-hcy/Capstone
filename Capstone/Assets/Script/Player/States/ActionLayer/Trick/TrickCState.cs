using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickCState : TrickState, ICanGetSystem, IBelongToArchitecture
{
    public TrickCState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopTrickC;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerTrickC;
        this.trickName = "TrickC";
        this.scoreValue = 10; 
    }

    protected override void EnterTrickState()
    {
        player.SendEvent<TrickCInputEvent>(new TrickCInputEvent { IsTrickingC = true });
    }
    protected override void UpdateActionState()
    {
        if(!inputModel.TrickC.Value)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "Recovery");
        }
        if(playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "Recovery");
        }
    }

    protected override void ExitActionState()
    {
        player.SendEvent<TrickCInputEvent>(new TrickCInputEvent { IsTrickingC = false });
        player.SendEvent<TrickCResetSpeedEvent>();
    } 
}