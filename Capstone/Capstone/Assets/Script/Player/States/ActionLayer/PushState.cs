using SkateGame;
using UnityEngine;
using QFramework;

public class PushState : ActionStateBase
{
    public PushState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopPush;
        stateTotalDuration = playerModel.PushDuration.Value;
        ignoreMovementLayerDuration = playerModel.Config.Value.ignoreMovementLayerDurationPush;
        recoveryDuration = playerModel.Config.Value.recoveryDurationPush;
    }

    public override string GetStateName() => "Push";

    protected override void EnterActionState()
    {
    }

    protected override void UpdateActionState()
    {
        player.SendEvent<PushInputEvent>();
    }

    protected override void ExitActionState()
    {
    }
}
