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

    public override string GetStateName() => trickName;

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
    protected bool DetectInteractiveObjects()
        {
            if (player == null) return false;
            Vector2 playerPosition = player.transform.position;
            float detectionRadius = 2f; // 检测半径
            
            // 方法1: 使用 Physics2D.OverlapCircle 检测圆形区域
            Collider2D[] colliders = Physics2D.OverlapCircleAll(playerPosition, detectionRadius, LayerMask.GetMask("InteractiveLayer"));
            if(colliders.Length > 0)
            {
                this.GetModel<IPlayerModel>().IsInPower.Value = true;
                return true;
            }
            return false;
        }
} 