using SkateGame;
using UnityEngine;

public class CruiseState : ActionStateBase
{
    public CruiseState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = true;
        ignoringMovementLayer = false;
    }

    protected override void EnterActionState()
    {
    }

    protected override void UpdateActionState()
    {
        CheckSwitchAction();
        CheckPushAndPowerGrind();
        if (!playerModel.IsGrounded.Value
            || Mathf.Abs(playerModel.PushSpeed.Value) <= playerModel.Config.Value.powerGrindStopSpeedThreshold)
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
    }

    protected override void ExitActionState()
    {
    }
}
