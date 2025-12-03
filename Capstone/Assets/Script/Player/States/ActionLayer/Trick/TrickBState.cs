using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickBState : TrickState, ICanGetSystem, IBelongToArchitecture
{
    public TrickBState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopTrickB;
        stateDuration = playerModel.Config.Value.durationTrickB;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerTrickB;
        this.trickName = "TrickB";
        this.scoreValue = 10; 
    }

    protected override void EnterTrickState()
    {
        playerModel.VelocityBeforeTrick.Value = rb.linearVelocity.x;
        player.SendEvent<TrickBInputEvent>(new TrickBInputEvent { IsTrickingB = true, Direction = inputModel.Move.Value.x > 0 ? 1f : -1f });
        
    }
    protected override void UpdateActionState()
    {
        if(DetectInteractiveObjects(out Collider2D[] colliders)){
            //尝试对第一个进行交互
            IInteractable interact = colliders[0].GetComponent<IInteractable>();
            interact?.DoInteraction();
            player.TrickABoostEffect.PlayFeedbacks();
            player.stateMachine.SwitchState(StateLayer.Action, "TrickBBoost");
        }
    }
    protected override void ExitActionState()
    {
        player.SendEvent<TrickBInputEvent>(new TrickBInputEvent { IsTrickingB = false });
        player.SendEvent<TrickBResetSpeedEvent>();
    } 
}