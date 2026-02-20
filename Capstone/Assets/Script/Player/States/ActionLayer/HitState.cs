using SkateGame;
using UnityEngine;
using QFramework;

public class HitState : ActionStateBase
{
    public HitState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        this.player = player;
        this.rb = rb;
        isLoop = playerModel.Config.Value.isLoopHit;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerHit;
        stateDuration = playerModel.Config.Value.hitDuration;
    }

    protected override void EnterActionState()
    {
        playerModel.PushSpeedBeforeHit.Value = playerModel.PushSpeed.Value;
        player.SendEvent<HitEvent>(new HitEvent { IsHitting = true });
    }

    protected override void ExitActionState()
    {
        player.SendEvent<HitEvent>(new HitEvent { IsHitting = false });
        playerModel.HitKnockbackDirection.Value = Vector2.zero;
    }
}
