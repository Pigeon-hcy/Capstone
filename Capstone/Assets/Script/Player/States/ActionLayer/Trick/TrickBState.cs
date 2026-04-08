using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;
using Lofelt.NiceVibrations;

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
        // FOR JERRY'S AUDIO - DASH
        //Debug.LogError("触发了");
        HapticPatterns.PlayEmphasis(0.1f, 0.3f);
        player.TrickBEffect.PlayFeedbacks();
        playerModel.VelocityBeforeTrick.Value = rb.linearVelocity.x;
        // TODO: fix direction calculation
        player.SendEvent<TrickBInputEvent>(new TrickBInputEvent 
            { 
                IsTrickingB = true, 
                Direction = inputModel.Move.Value.x != 0f ? (inputModel.Move.Value.x < 0f ? -1f : 1f) : playerModel.IsFacingRight.Value ? 1f : -1f
            });
        
    }
    protected override void UpdateActionState()
    {
        //Debug.LogError("触发了");
        if(DetectInteractiveObjects(out Collider2D[] colliders)){
            //尝试对第一个进行交互
            IInteractable interact = colliders[0].GetComponentInParent<IInteractable>();
            interact?.DoInteraction();
            player.TrickBBoostEffect.PlayFeedbacks();
            player.stateMachine.SwitchState<TrickBBoostState>(StateLayer.Action);
        }
    }
    protected override void ExitActionState()
    {
        player.SendEvent<TrickBInputEvent>(new TrickBInputEvent { IsTrickingB = false });
        player.SendEvent<TrickBResetSpeedEvent>();
    } 
}