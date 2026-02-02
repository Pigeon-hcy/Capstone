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
        if(!inputModel.Brake.Value)
        {
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
        }
        if(playerModel.IsGrounded.Value)
        {
            player.OpenSlamHitbox(playerModel.Config.Value.slamHitboxDurationTrickC);
            player.stateMachine.SwitchState<RecoveryState>(StateLayer.Action);
        }
        if(DetectInteractiveObjects(out Collider2D[] colliders))
        {
            IInteractable interact = colliders[0].GetComponent<IInteractable>();
            interact?.DoInteraction();
            player.stateMachine.SwitchState<TrickCBoostState>(StateLayer.Action);
        }
    }

    protected override void ExitActionState()
    {
        player.SendEvent<TrickCInputEvent>(new TrickCInputEvent { IsTrickingC = false });
        player.SendEvent<TrickCResetSpeedEvent>();
    }
}