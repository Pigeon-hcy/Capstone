using UnityEngine;
using QFramework;
using SkateGame;

namespace SkateGame
{
    /// <summary>
    /// 死亡区域控制器
    /// 玩家接触到此区域会触发重生
    /// </summary>
    public class DeathPoint : ViewerControllerBase
    {
        [Header("死亡区域设置")]
        public string playerTag = "Player";

        protected override void InitializeController()
        {
      
        }
        
        protected override void OnRealTimeUpdate()
        {
     
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 检测玩家进入死亡区域
            if (other.CompareTag(playerTag))
            {
                Debug.Log($"[DeathPoint] 玩家进入死亡区域: {gameObject.name}");
                
                // 调用重生系统
                var respawnSystem = this.GetSystem<IRespawnSystem>();
                if (respawnSystem != null)
                {
                    respawnSystem.RespawnPlayer();
                    Debug.Log($"[DeathPoint] 已触发重生");
                }
                else
                {
                    Debug.LogWarning("[DeathPoint] RespawnSystem 未找到，无法重生玩家");
                }
            }
        }
    }
}
