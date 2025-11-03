using UnityEngine;
using QFramework;

namespace SkateGame
{
    /// <summary>
    /// 重生检查点控制器
    /// 检测玩家通过并发送事件
    /// </summary>
    public class RespawnPoint : ViewerControllerBase
    {
        [Header("检查点设置")]
        public string playerTag = "Player";
        
        
        private bool isActivated = false;
        
        protected override void InitializeController()
        {
            Debug.Log($"RespawnPoint 初始化: {gameObject.name}");
        }
        
        protected override void OnRealTimeUpdate()
        {
            // 检查点不需要实时更新逻辑
            // 主要依靠触发器事件
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 检测玩家进入
            Debug.Log($"检测到玩家进入: {other.name}");
            if (other.CompareTag(playerTag) && !isActivated)
            {
                Debug.Log($"检测到玩家通过检查点: {gameObject.name}");
                
                // 标记为已激活
                isActivated = true;
                
                // 发送事件，传递检查点位置
                this.SendEvent(new PassRespawnPointEvent
                {
                    CheckpointPosition = transform.position
                });
                
                
            }
        }
        
       
        
        
    }
}

