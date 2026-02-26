using BaseUtility;
using QFramework;
using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;

namespace SkateGame
{
    public interface IRespawnSystem : ISystem
    {
        void AddCheckpoint(Vector2 checkpointPosition);
        void RespawnPlayer();
        void ClearCheckpoints();
    }

    public class RespawnSystem : AbstractSystem, IRespawnSystem, ICanSendEvent, ICanGetSystem
    {
        private IRespawnModel respawnModel;
        private PlayerController playerController;
        private IEnergySystem energySystem;
        private static MonoBehaviour coroutineRunner;

        public IArchitecture GetArchitecture() => GameApp.Interface;

        protected override void OnInit()
        {
            respawnModel = this.GetModel<IRespawnModel>();
            UpdatePlayerController();
            InitializeCoroutineRunner();
            energySystem = this.GetSystem<IEnergySystem>();
            this.RegisterEvent<PassRespawnPointEvent>(OnPassRespawnPoint);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

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
            UpdatePlayerController();
            ClearCheckpoints();
            this.GetModel<ITraceModel>().DrawnPoints.Clear();
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
            if (coroutineRunner == null)
                InitializeCoroutineRunner();
            coroutineRunner.StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            playDeath();
            if (playerController == null)
                UpdatePlayerController();
            if (playerController == null)
            {
                Debug.LogError("[RespawnSystem] PlayerController not found, cannot respawn.");
                yield break;
            }
            Transform player = playerController.transform;
            Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
            Vector2 deathPos = player.position;

            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;

            // 绘制死亡路径点（玩家禁用前，死亡点用专用 prefab）
            this.GetSystem<ITraceSystem>().OnPlayerDeath(deathPos);

            PlayDeathParticleAt(deathPos);

            playerController.gameObject.SetActive(false);

            yield return new WaitForSeconds(1f);

            PlayDeathMMFEffects();

            yield return new WaitForSeconds(1f);

            player.position = respawnModel.LatestCheckpoint.Value;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;

            playerController.gameObject.SetActive(true);
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            playerController.SendEvent<PlayerRespawnEvent>();
            playerController.UpdatePlayerDirection(true);

            
            var triggers = Object.FindObjectsByType<AttackTrigger>(FindObjectsSortMode.None);
            foreach (var t in triggers)
                t.isResetting = false;

            energySystem.ResetEnergy();
            playRespawnParticleAt(player.position);

            
            

            MessageSystem.Instance.Send(GameStateEnum.PlayerRespawn, null);
        }

        private void PlayDeathMMFEffects()
        {
            GameObject deathTrans = GameObject.Find("death trans");
            MMF_Player startPlayer = deathTrans.transform.Find("MMF_StartTrans").GetComponent<MMF_Player>();
            MMF_Player endPlayer = deathTrans.transform.Find("MMF_EndTrans").GetComponent<MMF_Player>();

            startPlayer.DurationMultiplier = 2f;
            endPlayer.DurationMultiplier = 2f;

            startPlayer.PlayFeedbacks();
            endPlayer.PlayFeedbacks();
        }

        private void PlayDeathParticleAt(Vector2 pos)
        {
            
            var particlePrefab = playerController.transform
                .Find("Particle Holder")
                .Find("DeathP")
                .GetComponent<ParticleSystem>();

            ParticleSystem ps = Object.Instantiate(particlePrefab, pos, Quaternion.identity);
            ps.transform.parent = null;
            ps.Play();

            Object.Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        private void playRespawnParticleAt(Vector2 pos)
        {
            var holder = playerController?.transform?.Find("Particle Holder");
            if (holder == null) return;
            var spawnObj = holder.Find("Spawn");
            if (spawnObj == null) return;
            var particlePrefab = spawnObj.GetComponent<ParticleSystem>();
            if (particlePrefab == null) return;
            particlePrefab.Play();
        }


        public void ClearCheckpoints()
        {
            respawnModel.CheckpointList.Value.Clear();
            respawnModel.LatestCheckpoint.Value = Vector2.zero;
            respawnModel.HasCheckpoint.Value = false;
        }
        public void playDeath()
        {
            AudioManager.Instance.fmodPlayDeath();
        }
    }

    public class CoroutineRunner : MonoBehaviour { }
}
