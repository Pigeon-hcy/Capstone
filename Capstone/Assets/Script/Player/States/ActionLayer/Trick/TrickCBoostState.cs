using UnityEngine;
using SkateGame;
using QFramework;

public class TrickCBoostState : TrickState, ICanGetSystem, IBelongToArchitecture
{
    private bool hasLanded = false;
    public TrickCBoostState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopTrickCBoost;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerTrickCBoost;
        this.trickName = "TrickCBoost";
        this.scoreValue = 10; 
    }
    protected override void EnterTrickState()
    {
    }

    protected override void UpdateActionState()
    {

    }
    protected override void ExitActionState()
    {
    }
}