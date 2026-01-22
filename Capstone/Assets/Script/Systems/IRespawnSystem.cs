using BaseUtility;
using QFramework;
using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;

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
        private static MonoBehaviour coroutineRunner;
        
        protected override void OnInit()
        {
            // 获取模型
            respawnModel = this.GetModel<IRespawnModel>();
            
            // 更新 PlayerController 引用
            UpdatePlayerController();
            
            // 初始化协程运行器
            InitializeCoroutineRunner();
            
            // 监听检查点经过事件
            this.RegisterEvent<PassRespawnPointEvent>(OnPassRespawnPoint);
            
            // 监听场景加载
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        /// <summary>
        /// 初始化协程运行器（用于在玩家被隐藏时也能运行协程）
        /// </summary>
        private void InitializeCoroutineRunner()
        {
            if (coroutineRunner == null)
            {
                GameObject runnerObj = new GameObject("RespawnCoroutineRunner");
                coroutineRunner = runnerObj.AddComponent<CoroutineRunner>();
                Object.DontDestroyOnLoad(runnerObj);
            }
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
        }
        
        private void OnPassRespawnPoint(PassRespawnPointEvent evt)
        {
            AddCheckpoint(evt.CheckpointPosition);
        }
        
        public void AddCheckpoint(Vector2 checkpointPosition)
        {
            respawnModel.CheckpointList.Value.Add(checkpointPosition);
            respawnModel.LatestCheckpoint.Value = checkpointPosition;
            respawnModel.HasCheckpoint.Value = true;
        }
        
        public void RespawnPlayer()
        {
            PlayDeathParticle();
           
            if (coroutineRunner == null)
                InitializeCoroutineRunner();
           
            PlayDeathMMFEffects();
            
            
            playerController.transform.position = respawnModel.LatestCheckpoint.Value;
            playerController.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            playerController.gameObject.SetActive(false);
            
            MessageSystem.Instance.Send(GameStateEnum.PlayerRespawn, null);
            coroutineRunner.StartCoroutine(ShowPlayerAfterDelay(0.5f));
        }
        
        private System.Collections.IEnumerator ShowPlayerAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            playerController.gameObject.SetActive(true);
        }
        
        private void PlayDeathMMFEffects()
        {
            GameObject deathTrans = GameObject.Find("death trans");
            deathTrans.transform.Find("MMF_StartTrans").GetComponent<MMF_Player>().PlayFeedbacks();
            deathTrans.transform.Find("MMF_EndTrans").GetComponent<MMF_Player>().PlayFeedbacks();
        }
        
        private void PlayDeathParticle()
        {
            playerController.transform.Find("Particle Holder").Find("DeathP").GetComponent<ParticleSystem>().Play();
        }
        
        public void ClearCheckpoints()
        {
            respawnModel.CheckpointList.Value.Clear();
            respawnModel.LatestCheckpoint.Value = Vector2.zero;
            respawnModel.HasCheckpoint.Value = false;
        }
    }
    
    /// <summary>
    /// 协程运行器辅助类
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        // 这个类只用于运行协程，不需要其他功能
    }
}
