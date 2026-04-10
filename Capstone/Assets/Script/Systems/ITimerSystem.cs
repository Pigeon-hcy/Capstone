using QFramework;
using UnityEngine;

namespace SkateGame
{
    public interface ITimerSystem : ISystem
    {
        void StartTimer();
        void StopTimer();
        void ResetTimer();
    }

    public class TimerSystem : AbstractSystem, ITimerSystem
    {
        private ITimerModel timerModel;
        private TimerRunner runner;

        protected override void OnInit()
        {
            timerModel = this.GetModel<ITimerModel>();

            var go = new GameObject("[TimerRunner]");
            Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<TimerRunner>();
            runner.Init(this);

            // 场景加载时重置并启动计时
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // 跳过菜单和 Bootstrap 场景
            string name = scene.name;
            if (name == "Bootstrap" || name == "MainMenu")
            {
                StopTimer();
                return;
            }

            ResetTimer();
            StartTimer();
        }

        public void StartTimer()
        {
            timerModel.IsRunning.Value = true;
            this.SendEvent(new TimerStateChangedEvent { IsRunning = true });
        }

        public void StopTimer()
        {
            timerModel.IsRunning.Value = false;
            this.SendEvent(new TimerStateChangedEvent { IsRunning = false });
        }

        public void ResetTimer()
        {
            timerModel.ElapsedTime.Value = 0f;
            this.SendEvent(new TimerUpdatedEvent { ElapsedTime = 0f });
        }

        internal void Tick(float deltaTime)
        {
            if (!timerModel.IsRunning.Value) return;

            timerModel.ElapsedTime.Value += deltaTime;
            this.SendEvent(new TimerUpdatedEvent { ElapsedTime = timerModel.ElapsedTime.Value });
        }
    }

    public class TimerRunner : MonoBehaviour
    {
        private TimerSystem system;

        public void Init(TimerSystem timerSystem)
        {
            system = timerSystem;
        }

        private void Update()
        {
            if (system != null)
            {
                system.Tick(Time.deltaTime);
            }
        }
    }
}
