using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickAState : TrickState, ICanGetSystem, IBelongToArchitecture
{
    
    public TrickAState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopTrickA;
        stateTotalDuration = playerModel.Config.Value.durationTrickA;
        ignoreMovementLayerDuration = playerModel.Config.Value.ignoreMovementLayerDurationTrickA;
        recoveryDuration = playerModel.Config.Value.recoveryDurationTrickA;
        this.trickName = "TrickA";
        this.scoreValue = 10; 
    }

    public override string GetStateName() => "TrickA";
    protected override void EnterActionState()
    {
       
        
        player.TrickAEffect.PlayFeedbacks();

        DetectInteractiveObjectsWithRaycast();
        
        var trickSystem = this.GetSystem<ITrickSystem>();
        
        if (trickSystem != null)
        {
            trickSystem.AddTrick(this);
            trickSystem.printTrickList();
        }
        
    }
    private void DetectInteractiveObjectsWithRaycast()
        {
            if (player == null) return;
            Vector2 playerPosition = player.transform.position;
            float detectionRadius = 2f; // 检测半径
            
            // 方法1: 使用 Physics2D.OverlapCircle 检测圆形区域
            Collider2D[] colliders = Physics2D.OverlapCircleAll(playerPosition, detectionRadius, LayerMask.GetMask("InteractiveLayer"));
            if(colliders.Length > 0)
            {
                this.GetModel<IPlayerModel>().IsInPower.Value = true;
                if(this.GetModel<IPlayerModel>().IsInPower.Value){
                    player.TrickABoostEffect.PlayFeedbacks();
                    player.RewardJump();
                }
            }
        }
}