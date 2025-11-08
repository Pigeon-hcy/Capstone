using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickBState : TrickState, ICanGetSystem, IBelongToArchitecture
{
    
    public TrickBState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopTrickB;
        stateTotalDuration = playerModel.Config.Value.durationTrickB;
        ignoreMovementLayerDuration = playerModel.Config.Value.ignoreMovementLayerDurationTrickB;
        recoveryDuration = playerModel.Config.Value.recoveryDurationTrickB;
        this.trickName = "TrickB";
        this.scoreValue = 10; 
    }

    public override string GetStateName() => "TrickB";
    protected override void EnterActionState()
    {
        base.EnterActionState();
        player.SendEvent<TrickBInputEvent>();
    }
}