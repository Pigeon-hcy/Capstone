using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickDState : TrickState, ICanGetSystem, IBelongToArchitecture
{
    public TrickDState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopTrickD;
        stateDuration = playerModel.Config.Value.durationTrickD;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerTrickD;
        this.trickName = "TrickD";
        this.scoreValue = 10; 
    }

    protected override void EnterTrickState()
    {
        /// TODO：自瞄敌人 
        player.vineGun.fireGrabbingHook(45f);
    }
    protected override void UpdateActionState()
    {
    }

    protected override void ExitActionState()
    {
    } 
}