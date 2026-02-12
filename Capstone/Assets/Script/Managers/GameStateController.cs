using UnityEngine;
using UnityEngine.EventSystems;
using QFramework;

namespace SkateGame
{
    public enum GameState
    {
        Menu,
        Pause,
        InGame,
        Dialogue,
        Tutorial
    }

	public class GameStateController : MonoBehaviour, IBelongToArchitecture, ICanGetSystem, ICanRegisterEvent
    {
        public IArchitecture GetArchitecture() => GameApp.Interface;
        private static GameStateController _instance;
        public static GameStateController Instance {get => _instance;}

        [SerializeField] private GameState _current = GameState.Menu;
        public GameState Current => _current;
        private GameState _stateBeforePause = GameState.InGame;
		[SerializeField] private GameObject pauseUI;
		[SerializeField] private GameObject pauseFirstSelected;
		[SerializeField] private GameObject playerUI;
        private IInputGateSystem gate;

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

		public void EnterMenu() => Switch(GameState.Menu);
        public void EnterInGame() => Switch(GameState.InGame);
        public void EnterDialogue() => Switch(GameState.Dialogue);
        public void EnterPause() => Switch(GameState.Pause);
        public void EnterTutorial() => Switch(GameState.Tutorial);
        public void Switch(GameState next)
        {
			if (_current == GameState.Menu && next != GameState.InGame) return;
            if (_current == next) return;
            _current = next;
            ApplyState(_current);
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
            }
        }
        private void OnTogglePause(TogglePauseEvent evt)
        {
            if (_current == GameState.Menu) return;
            if (_current == GameState.InGame || _current == GameState.Dialogue || _current == GameState.Tutorial)
            {
                _stateBeforePause = _current;
                EnterPause();
                SetPauseUI(true);
            }
            else if (_current == GameState.Pause)
            {
                Switch(_stateBeforePause);
                SetPauseUI(false);
            }
        }
        private void OnSceneChange(SceneChangeEvent evt)
        {
            EnterInGame();
        }
		private void SetPauseUI(bool show)
		{
			if (pauseUI != null) pauseUI.SetActive(show);
			if (show) EventSystem.current?.SetSelectedGameObject(pauseFirstSelected);
			else if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
		}
        private void SetPlayerUI(bool show)
        {
            if (playerUI != null) playerUI.SetActive(show);
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
                Time.timeScale = 0f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }
            else
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }
        }
    }
}


