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
        recoveryDuration = playerModel.Config.Value.recoveryDurationTrickA;
        this.trickName = "TrickA";
        this.scoreValue = 10; 
    }

    public override string GetStateName() => "TrickA";
    protected override void EnterTrickState()
    {
        player.TrickAEffect.PlayFeedbacks();
        
    }
    protected override void UpdateActionState()
    {
        if(DetectInteractiveObjectsWithRaycast()){
            player.TrickABoostEffect.PlayFeedbacks();
            player.SendEvent<RewardJumpEvent>();
            player.stateMachine.SwitchState(StateLayer.Action, "None");
        }
    }
}