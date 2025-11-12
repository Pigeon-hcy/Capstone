using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickBState : TrickState, ICanGetSystem, IBelongToArchitecture
{
    private bool resetSpeedSent = false;
    public TrickBState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopTrickB;
        stateDuration = playerModel.Config.Value.durationTrickB;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerTrickB;
        recoveryDuration = playerModel.Config.Value.recoveryDurationTrickB;
        this.trickName = "TrickB";
        this.scoreValue = 10; 
    }

    protected override void EnterTrickState()
    {
        resetSpeedSent = false;
        playerModel.VelocityBeforeTrick.Value = rb.linearVelocity.x;
        player.SendEvent<TrickBInputEvent>(new TrickBInputEvent { IsTrickingB = true, Direction = playerModel.IsFacingRight.Value ? 1f : -1f });
        
    }
    protected override void UpdateActionState()
    {
        if (stateTimer > stateDuration && !resetSpeedSent)
        {
            player.SendEvent<TrickBInputEvent>(new TrickBInputEvent { IsTrickingB = false});
            resetSpeedSent = true;
        }
    }
}