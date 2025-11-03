using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickState : ActionStateBase
{   protected int scoreValue;
    protected string trickName;

    // Public read-only accessors for external systems
    public int ScoreValue => scoreValue;
    public string TrickName => trickName;
    public float StateTotalDuration => stateTotalDuration;

    public TrickState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
    }

    public override string GetStateName() => "Trick";

    protected override void EnterActionState()
    {
        PerformTrick();
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
                player.RewardJump();
                playerModel.IsInPower.Value = false; // 消耗能量状态
            }
    }
} 