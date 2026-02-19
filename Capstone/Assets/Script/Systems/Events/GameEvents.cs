using QFramework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SkateGame
{
    // 切换暂停/继续游戏事件
    public struct TogglePauseEvent
    {
    }
    public struct SceneChangeEvent
    {
    }
    // 玩家落地事件
    public struct PlayerLandedEvent
    {
    }
    
    // 状态切换事件
    public struct StateChangedEvent
    {
        public StateLayer Layer;
        public string FromState;
        public string ToState;
    }
    
    // 分数更新事件
    public struct ScoreUpdatedEvent
    {
        public int NewScore;
    }
    
    
    // 轨道输入事件
    public struct GrindInputEvent
    {
        public bool IsGrinding;
    }
    
    #region Tricks
    // TrickA - 360
    public struct TrickAInputEvent
    {
    }

    // 奖励跳跃事件
    public struct TrickARewardEvent
    {
        public float RewardDirection;
    }   
    
    // TrickB - Dash
    public struct TrickBInputEvent
    {
        public float Direction;
        public bool IsTrickingB;
    }

    public struct TrickBResetSpeedEvent
    {
    }
    
    // TrickC - Slam
    public struct TrickCInputEvent
    {
        public bool IsTrickingC;
    }

    public struct TrickCLandEvent
    {
    }
    public struct GrappleEvent
    {
        public Vector2 pullDirection;
        public bool IsGrappling;
    }
    #endregion
    // 强力轨道输入事件
    public struct PowerGrindInputEvent
    {
        public bool IsPowerGrinding;
    }
    
    // 反向输入事件
    public struct ReverseInputEvent
    {
    }
    
    // 跳跃执行事件
    public struct JumpExecuteEvent
    {
        public bool IsJumping;
    }
    public struct WallJumpExecuteEvent
    {
    }
    // 移动输入事件
    public struct MoveInputEvent
    {
        public float HorizontalInput;
    }

    // Push输入事件
    public struct PushInputEvent
    {
        public bool IsPushing;
        public bool IsPushingRight;
        public bool IsReversing;
    }
    
    // Trick列表变化事件
    public struct TrickListChangedEvent
    {
        public TrickState LatestTrick;
    }
    // 通过重生检查点事件
    public struct PassRespawnPointEvent
    {
        public Vector2 CheckpointPosition;
    }
    
    // 撞墙事件
    public struct WallHitEvent
    {
        public Vector2 wallNormal;
    }
    
    // 受击事件
    public struct HitEvent
    {
        public bool IsHitting;
    }
}
