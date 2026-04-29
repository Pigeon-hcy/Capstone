using UnityEngine;
using UnityEngine.EventSystems;
using QFramework;
using MoreMountains.Feedbacks;

namespace SkateGame
{
    public enum GameState
    {
        Menu,
        Pause,
        InGame,
        Dialogue,
        Tutorial,
        UIPause
    }

	public class GameStateController : MonoBehaviour, IBelongToArchitecture, ICanGetSystem, ICanRegisterEvent, ICanSendEvent
    {
        public IArchitecture GetArchitecture() => GameApp.Interface;
        private static GameStateController _instance;
        public static GameStateController Instance {get => _instance;}

        [SerializeField] private GameState _current = GameState.Menu;
        public GameState Current => _current;
        private GameState _stateBeforePause = GameState.InGame;
        [SerializeField] private GameObject playerUI;
        private IInputGateSystem gate;

        /// <summary> GameState 使用 timeScale=0 暂停时为 true；此时禁止 MMTimeManager 写入 Time.timeScale，避免与 MMF 冲突。 </summary>
        bool _mmfTimescaleLocked;
        public bool MmfTimescaleLocked => _mmfTimescaleLocked;

        void OnEnable()
        {
            this.RegisterEvent<TogglePauseEvent>(OnTogglePause);
            this.RegisterEvent<SceneChangeEvent>(OnSceneChange);
        }

        void OnDisable()
        {
            this.UnRegisterEvent<TogglePauseEvent>(OnTogglePause);
            this.UnRegisterEvent<SceneChangeEvent>(OnSceneChange);
        }
        private void Awake()
        {
            gate = this.GetSystem<IInputGateSystem>();
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            ApplyState(_current);
        }

        void Start()
        {
            if (_mmfTimescaleLocked)
                SetMmfTimescaleWriteLocked(true);
        }

		public void EnterMenu() => Switch(GameState.Menu);
        public void EnterInGame() => Switch(GameState.InGame);
        public void EnterDialogue() => Switch(GameState.Dialogue);
        public void EnterPause() => Switch(GameState.Pause);
        public void EnterTutorial() => Switch(GameState.Tutorial);
        public void EnterUIPause() => Switch(GameState.UIPause);
        public void Switch(GameState next)
        {
			if (_current == GameState.Menu && next != GameState.InGame) return;
            if (_current == next) return;
            var old = _current;
            _current = next;
            ApplyState(_current);
            this.SendEvent(new GameStateChangedEvent { NewState = next, OldState = old });
        }

        private void ApplyState(GameState s)
        {
            var gate = this.GetSystem<IInputGateSystem>();
            switch (s)
            {
				case GameState.Menu:
                    UseUiInput(true);
					TimeStop(true);
                    SetPlayerUI(false);
					Debug.Log("Enter Menu");
					break;
                case GameState.InGame:
                    UseUiInput(false);
                    TimeStop(false);
                    SetPlayerUI(true);
                    Debug.Log("Enter InGame");
                    break;
                case GameState.Dialogue:
                    UseUiInput(true);
                    TimeStop(false);
                    SetPlayerUI(false);
                    Debug.Log("Enter Dialogue");
                    break;
                case GameState.Pause:
                    UseUiInput(true);
                    TimeStop(true);
                    SetPlayerUI(true);
                    Debug.Log("Enter Pause");
                    break;
                case GameState.Tutorial:
                    UseUiInput(false);
                    // Timescale由TeachTimeStop控制
                    SetPlayerUI(true);
                    Debug.Log("Enter Tutorial");
                    break;
                case GameState.UIPause:
                    UseUiInput(true);
                    TimeStop(true);
                    SetPlayerUI(false);
                    Debug.Log("Enter UIPause");
                    break;
            }
        }
        private void OnTogglePause(TogglePauseEvent evt)
        {
            if (_current == GameState.Menu) return;
            if (_current == GameState.InGame || _current == GameState.Dialogue)
            {
                _stateBeforePause = _current;
                EnterPause();
            }
            else if (_current == GameState.Pause)
            {
                Switch(_stateBeforePause);
            }
        }
        private void OnSceneChange(SceneChangeEvent evt)
        {
            EnterInGame();
        }
        private void SetPlayerUI(bool show)
        {
            if (playerUI != null) playerUI.GetComponent<CanvasGroup>().alpha = show ? 1 : 0;
        }
        
        private void UseUiInput(bool use)
        {
            if (use)
            {
                gate.SetUiInputEnabled(true);
                gate.SetPlayerInputBlocked(true);
            }
            else
            {
                gate.SetUiInputEnabled(false);
                gate.SetPlayerInputBlocked(false);
            }
        }

        private void TimeStop(bool stop)
        {
            if (stop)
            {
                SetMmfTimescaleWriteLocked(true);
                Time.timeScale = 0f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }
            else
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                SetMmfTimescaleWriteLocked(false);
            }
        }

        void SetMmfTimescaleWriteLocked(bool locked)
        {
            _mmfTimescaleLocked = locked;
            MMTimeManager mm = MMTimeManager.TryGetInstance();
            if (mm == null)
                mm = Object.FindAnyObjectByType<MMTimeManager>(FindObjectsInactive.Include);
            if (mm != null)
                mm.UpdateTimescale = !locked;
        }
    }
}


