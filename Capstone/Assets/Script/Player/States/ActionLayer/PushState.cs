using SkateGame;
using UnityEngine;
using QFramework;

public class PushState : ActionStateBase
{
    private bool pushEventSent;
    public PushState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopPush;
        stateDuration = playerModel.PushDuration.Value;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerPush;
    }

    public override string GetStateName() => "Push";

    protected override void EnterActionState()
    {   
        pushEventSent = false;
    }

    protected override void UpdateActionState()
    {
        if (stateTimer >= 0.3f)
        {
            if(!pushEventSent){
                player.SendEvent<PushInputEvent>(new PushInputEvent { IsPushing = true });
                pushEventSent = true;
            }
        }
    }

    protected override void ExitActionState()
    {
        player.SendEvent<PushInputEvent>(new PushInputEvent { IsPushing = false });
    }
}
