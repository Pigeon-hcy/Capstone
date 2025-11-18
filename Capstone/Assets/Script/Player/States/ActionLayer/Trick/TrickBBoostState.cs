using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickBBoostState : TrickState, ICanGetSystem, IBelongToArchitecture
{
    public TrickBBoostState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopTrickBBoost;
        stateDuration = playerModel.Config.Value.durationTrickBBoost;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerTrickBBoost;
        this.trickName = "TrickBBoost";
        this.scoreValue = 10; 
    }

    protected override void EnterTrickState()
    {
        playerModel.VelocityBeforeTrick.Value = rb.linearVelocity.x;
        player.SendEvent<TrickBInputEvent>(new TrickBInputEvent { IsTrickingB = true, Direction = playerModel.IsFacingRight.Value ? 1f : -1f });
    }
    protected override void UpdateActionState()
    {
        CheckSwitchAction();
    }
    protected override void ExitActionState()
    {
        player.SendEvent<TrickBInputEvent>(new TrickBInputEvent { IsTrickingB = false });
        player.SendEvent<TrickBResetSpeedEvent>();
    } 
}