using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickState : ActionStateBase, ICanGetSystem, IBelongToArchitecture
{   protected int scoreValue;
    protected string trickName;

    // Public read-only accessors for external systems
    public int ScoreValue => scoreValue;
    protected virtual void EnterTrickState(){}
    public TrickState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
    }

    protected sealed override void EnterActionState()
    {
        var trickSystem = this.GetSystem<ITrickSystem>();
        if (trickSystem != null)
        {
            trickSystem.AddTrick(this);
            trickSystem.printTrickList();
        }
        EnterTrickState();
        // PerformTrick();
    }

    private void PerformTrick()
    {
        
        PerformTrick(player);
        
        // 标记已执行trick，用于落地奖励
        player.MarkTrickPerformed();
    }

    protected override void UpdateActionState()
    {
    }

    private void PerformTrick(PlayerController player)
    {   
        // 检测是否在能量状态，如果是则给予奖励
        CheckIfInPower(player);
        
    }
    private void CheckIfInPower(PlayerController player)
    {
        if (playerModel.IsInPower.Value)
            {
                playerModel.IsInPower.Value = false; // 消耗能量状态
            }
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