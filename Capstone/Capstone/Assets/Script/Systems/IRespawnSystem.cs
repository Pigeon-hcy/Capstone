using QFramework;
using UnityEngine;

namespace SkateGame
{
    public interface IRespawnSystem : ISystem
    {
        /// <summary>
        /// 添加检查点
        /// </summary>
        void AddCheckpoint(Vector2 checkpointPosition);
        
        /// <summary>
        /// 重生玩家
        /// </summary>
        void RespawnPlayer();
        
        /// <summary>
        /// 清除所有检查点
        /// </summary>
        void ClearCheckpoints();
    }

    public class RespawnSystem : AbstractSystem, IRespawnSystem, ICanSendEvent
    {
        private IRespawnModel respawnModel;
        private PlayerController playerController;
        
        protected override void OnInit()
        {
            // 获取模型
            respawnModel = this.GetModel<IRespawnModel>();
            
            // 更新 PlayerController 引用
            UpdatePlayerController();
            
            // 监听检查点经过事件
            this.RegisterEvent<PassRespawnPointEvent>(OnPassRespawnPoint);
            
            // 监听场景加载
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // 场景加载后重新查找 PlayerController 并清空检查点
            UpdatePlayerController();
            ClearCheckpoints();
        }
        
        private void UpdatePlayerController()
        {
            playerController = Object.FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                Debug.Log("RespawnSystem: 找到 PlayerController");
            }
            else
            {
                Debug.LogWarning("RespawnSystem: 场景中没有找到 PlayerController");
            }
        }
        
        private void OnPassRespawnPoint(PassRespawnPointEvent evt)
        {
            AddCheckpoint(evt.CheckpointPosition);
        }
        
        public void AddCheckpoint(Vector2 checkpointPosition)
        {
            // 添加到列表
            respawnModel.CheckpointList.Value.Add(checkpointPosition);
            
            // 更新最新检查点
            respawnModel.LatestCheckpoint.Value = checkpointPosition;
            
            // 标记有检查点
            respawnModel.HasCheckpoint.Value = true;
            
            Debug.Log($"RespawnSystem: 添加检查点 {checkpointPosition}, 总数: {respawnModel.CheckpointList.Value.Count}");
        }
        
        public void RespawnPlayer()
        {
            if (!respawnModel.HasCheckpoint.Value)
            {
                Debug.LogWarning("RespawnSystem: 没有可用的检查点");
                return;
            }
            
            if (playerController == null)
            {
                Debug.LogWarning("RespawnSystem: PlayerController 未找到");
                UpdatePlayerController();
                return;
            }
            
            // 获取最新检查点
            Vector2 latestCheckpoint = respawnModel.LatestCheckpoint.Value;
            
            // 重生玩家到检查点
            playerController.transform.position = latestCheckpoint;
            
            // 重置玩家速度
            Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            
            Debug.Log($"RespawnSystem: 玩家重生到 {latestCheckpoint}");
        }
        
        public void ClearCheckpoints()
        {
            respawnModel.CheckpointList.Value.Clear();
            respawnModel.LatestCheckpoint.Value = Vector2.zero;
            respawnModel.HasCheckpoint.Value = false;
            
            Debug.Log("RespawnSystem: 清除所有检查点");
        }
    }
}

