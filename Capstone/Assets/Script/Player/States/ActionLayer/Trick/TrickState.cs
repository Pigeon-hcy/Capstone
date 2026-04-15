using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickState : ActionStateBase, ICanGetSystem, IBelongToArchitecture
{
    protected virtual void EnterTrickState(){}
    public TrickState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
    }

    protected sealed override void EnterActionState()
    {
        EnterTrickState();
    }

    protected override void UpdateActionState()
    {
    }

    protected bool DetectInteractiveObjects(out Collider2D[] detectColliders)
    {
        detectColliders = null;
        if (player == null || player.trickDetectHitbox == null) return false;

        var filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("InteractiveLayer"));
        filter.useTriggers = true;

        var results = new System.Collections.Generic.List<Collider2D>();
        if (Physics2D.OverlapCollider(player.trickDetectHitbox, filter, results) > 0)
        {
            this.GetModel<IPlayerModel>().IsInPower.Value = true;
            detectColliders = results.ToArray();
            return true;
        }
        return false;
    }
} 