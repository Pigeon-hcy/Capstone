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
        player.SendEvent<TrickAInputEvent>();
        player.TrickAEffect.PlayFeedbacks();
        
    }
    protected override void UpdateActionState()
    {
        //Debug.LogError("TrickC");
        if(DetectInteractiveObjects(out Collider2D[] colliders)){
            //尝试对第一个进行交互
            IInteractable interact = colliders[0].GetComponentInParent<IInteractable>();
            interact?.DoInteraction();
            player.TrickABoostEffect.PlayFeedbacks();
            player.SendEvent<TrickARewardEvent>();
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
        }
    }
}