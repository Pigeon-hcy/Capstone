using SkateGame;
using UnityEngine;

public class NoActionState : ActionStateBase
{
    public override string GetStateName() => "None";
    public NoActionState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        this.player = player;
        this.rb = rb;
        isLoop = playerModel.Config.Value.isLoopNoAction;
        ignoreMovementLayerDuration = playerModel.Config.Value.ignoreMovementLayerDurationNoAction;
    }
}
