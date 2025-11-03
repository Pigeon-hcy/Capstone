using SkateGame;
using UnityEngine;

public class GrabbingState : ActionStateBase
{

    public GrabbingState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopGrab;
        ignoreMovementLayerDuration = playerModel.Config.Value.ignoreMovementLayerDurationGrab;
    }

    public override string GetStateName() => "Grab";

    protected override void EnterActionState()
    {
        //MMF
        if (player.GrabEffect != null)
        {
            player.GrabEffect.PlayFeedbacks();
        }
    }

    protected override void UpdateActionState()
    {
    }

    protected override void ExitActionState()
    {
        //MMF
        if (player.GrabEffect != null)
        {
            player.GrabEffect.StopFeedbacks();
        }
    }
}
