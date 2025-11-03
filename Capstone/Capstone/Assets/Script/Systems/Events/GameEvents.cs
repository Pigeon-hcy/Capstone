using QFramework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SkateGame
{
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
    }
    
    // 技巧A输入事件
    public struct TrickAInputEvent
    {
    }
    
    // 技巧B输入事件
    public struct TrickBInputEvent
    {
    }
    
    // 强力轨道输入事件
    public struct PowerGrindInputEvent
    {
    }
    
    // 反向输入事件
    public struct ReverseInputEvent
    {
    }
    
    // 跳跃执行事件
    public struct JumpExecuteEvent
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
}
