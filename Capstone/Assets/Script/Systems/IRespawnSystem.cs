using BaseUtility;
using QFramework;
using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;
using Lofelt.NiceVibrations;

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
            // Debug: 死亡时检查点状态
            if (!respawnModel.HasCheckpoint.Value || respawnModel.CheckpointList.Value == null || respawnModel.CheckpointList.Value.Count == 0)
            {
                Debug.LogWarning("[RespawnSystem] 死亡时检查点为空！HasCheckpoint=" + respawnModel.HasCheckpoint.Value
                    + ", CheckpointList.Count=" + (respawnModel.CheckpointList.Value?.Count ?? 0)
                    + ", LatestCheckpoint=" + respawnModel.LatestCheckpoint.Value + "（将重生在原点或上次记录点）");
            }
            else
            {
                Debug.Log("[RespawnSystem] 死亡时检查点有效，重生位置: " + respawnModel.LatestCheckpoint.Value + ", 检查点数量: " + respawnModel.CheckpointList.Value.Count);
            }

            playDeath();
            stopTimeStopEffect();
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

            var triggers = Object.FindObjectsByType<AttackTrigger>(FindObjectsSortMode.None);
            foreach (var t in triggers)
                t.ResetTrapOnPlayerDeath();
            var tickToggles = Object.FindObjectsByType<TickToggle>(FindObjectsSortMode.None);
            foreach (var t in tickToggles)
                t.ResetOnPlayerDeath();

            yield return new WaitForSeconds(0.6f);

            PlayDeathMMFEffects();

            yield return new WaitForSeconds(0.6f);

            player.position = respawnModel.LatestCheckpoint.Value;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;

            playerController.gameObject.SetActive(true);
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            playerController.SendEvent<PlayerRespawnEvent>();
            playerController.UpdatePlayerDirection(true);
            
            foreach (var t in triggers)
                t.isResetting = false;

            energySystem.ResetEnergy();
            playRespawnParticleAt(player.position);

            foreach (var t in triggers)
                t.isResetting = true;

            MessageSystem.Instance.Send(GameStateEnum.PlayerRespawn, null);
        }

        private void PlayDeathMMFEffects()
        {
            GameObject deathTrans = GameObject.Find("death trans");
            MMF_Player startPlayer = deathTrans.transform.Find("MMF_StartTrans").GetComponent<MMF_Player>();
            MMF_Player endPlayer = deathTrans.transform.Find("MMF_EndTrans").GetComponent<MMF_Player>();

            startPlayer.DurationMultiplier = 0.9f;
            endPlayer.DurationMultiplier = 0.9f;

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
        // FOR JERRY'S AUDIO - PLAYER DEATH
        public void playDeath()
        {
            AudioManager.Instance.fmodPlayDeath();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
        }

        public void stopTimeStopEffect()
        {
            var TeachTimeStopBoxs = Object.FindObjectsByType<TeachTimeStop>(FindObjectsSortMode.None);
            if (TeachTimeStopBoxs == null) return;
            foreach (var box in TeachTimeStopBoxs)
            {
                box.timeStopEffect.StopFeedbacks();
                box.CompleteEffectBack.PlayFeedbacks();
            }
        }
    }

    public class CoroutineRunner : MonoBehaviour { }
}
