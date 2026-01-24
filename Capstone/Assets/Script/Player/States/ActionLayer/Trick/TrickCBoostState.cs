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

    protected override void UpdateActionState()
    {
        if (playerModel.IsGrounded.Value)
        {
            rb.linearVelocity = playerModel.Config.Value.TrickCBoostspeed * (Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.right).normalized;
            hasLanded = true;
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -playerModel.Config.Value.TrickCBoostspeed);
            if(hasLanded)
            {
                player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
            }
        }
        
        if(!inputModel.Slam.Value && stateTimer > playerModel.Config.Value.minDurationTrickCBoost)
        {
            player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
        }
    }
    protected override void ExitActionState()
    {
        player.SendEvent<TrickARewardEvent>();
    }
}