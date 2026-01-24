using SkateGame;
using UnityEngine;

public class RecoveryState : ActionStateBase
{
    public RecoveryState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        this.player = player;
        this.rb = rb;
        isLoop = playerModel.Config.Value.isLoopRecovery;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerRecovery;
    }
    protected override void EnterActionState()
    {
        var prev = playerModel.LastActionStateName.Value;
        stateDuration = 0f;
        if (prev == "TrickAState") stateDuration = playerModel.Config.Value.recoveryDurationTrickA;
        else if (prev == "TrickBState") stateDuration = playerModel.Config.Value.recoveryDurationTrickB;
        else if (prev == "TrickCState") stateDuration = playerModel.Config.Value.recoveryDurationTrickC;
        else if (prev == "PushState") stateDuration = playerModel.Config.Value.recoveryDurationPush;
    }
    protected override void UpdateActionState()
    {
        if(stateTimer > stateDuration)
        {
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
        }

        if (inputModel.Grind.Value)
        {
            GrindInput();   
        }
    }
}
