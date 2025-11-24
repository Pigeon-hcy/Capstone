using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickCBoostState : TrickState, ICanGetSystem, IBelongToArchitecture
{
    public TrickCBoostState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopTrickCBoost;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerTrickCBoost;
        this.trickName = "TrickCBoost";
        this.scoreValue = 10; 
    }

    protected override void UpdateActionState()
    {
        if (playerModel.IsGrounded.Value)
        {
            rb.linearVelocity = playerModel.Config.Value.TrickCBoostspeed * (Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.right).normalized;
            if(Mathf.Abs(playerModel.CurrentRotationDeg.Value) > playerModel.Config.Value.stopTrickCAngle)
            {
                player.stateMachine.SwitchState(StateLayer.Action, "None");
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -playerModel.Config.Value.TrickCBoostspeed);
        }
        
        if(!inputModel.TrickC.Value && stateTimer > playerModel.Config.Value.minDurationTrickCBoost)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "None");
        }
    }
    protected override void ExitActionState()
    {
        player.SendEvent<TrickARewardEvent>();
    }
}