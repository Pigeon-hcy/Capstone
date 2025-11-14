using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickAState : TrickState
{
    
    public TrickAState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopTrickA;
        stateDuration = playerModel.Config.Value.durationTrickA;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerTrickA;
        this.trickName = "TrickA";
        this.scoreValue = 10; 
    }

    protected override void EnterTrickState()
    {
        player.TrickAEffect.PlayFeedbacks();
        
    }
    protected override void UpdateActionState()
    {
        if(DetectInteractiveObjects()){
            player.TrickABoostEffect.PlayFeedbacks();
            player.SendEvent<TrickARewardEvent>();
            player.stateMachine.SwitchState(StateLayer.Action, "None");
        }
    }
}