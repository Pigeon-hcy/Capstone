using UnityEngine;
using UnityEngine.EventSystems;
using QFramework;

namespace SkateGame
{
    public enum GameState
    {
		Menu,
        InGame,
        Dialogue,
        Pause,
        Overlay  //level select, etc.
    }

	public class GameStateController : MonoBehaviour, IBelongToArchitecture, ICanGetSystem, ICanRegisterEvent
    {
        public IArchitecture GetArchitecture() => GameApp.Interface;
        private static GameStateController _instance;
        public static GameStateController Instance {get => _instance;}

        [SerializeField] private GameState _current = GameState.Menu;
        public GameState Current => _current;
		[SerializeField] private GameObject pauseUI;
		[SerializeField] private GameObject pauseFirstSelected;
		[SerializeField] private GameObject playerUI;

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
        public void EnterOverlay() => Switch(GameState.Overlay);

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
					gate.SetPlayerInputBlocked(true);
					gate.SetUiInputEnabled(true);
					Time.timeScale = 0f;
					Time.fixedDeltaTime = 0.02f * Time.timeScale;
					SetPauseUI(false);
                    SetPlayerUI(false);
					Debug.Log("Enter Menu");
					break;
                case GameState.InGame:
                    gate.SetPlayerInputBlocked(false);
                    gate.SetUiInputEnabled(false);
					Time.timeScale = 1f;
					Time.fixedDeltaTime = 0.02f * Time.timeScale;
					SetPauseUI(false);
                    SetPlayerUI(true);
                    Debug.Log("Enter InGame");
                    break;
                case GameState.Dialogue:
                    gate.SetPlayerInputBlocked(true);
                    gate.SetUiInputEnabled(true);
					Time.timeScale = 1f;
					Time.fixedDeltaTime = 0.02f * Time.timeScale;
					SetPauseUI(false);
                    SetPlayerUI(false);
                    Debug.Log("Enter Dialogue");
                    break;
                case GameState.Pause:
                    gate.SetPlayerInputBlocked(true);
                    gate.SetUiInputEnabled(true);
					Time.timeScale = 0f;
					Time.fixedDeltaTime = 0.02f * Time.timeScale;
					SetPauseUI(true);
                    SetPlayerUI(true);
                    Debug.Log("Enter Pause");
                    break;
                case GameState.Overlay:
                    gate.SetPlayerInputBlocked(true);
                    gate.SetUiInputEnabled(true);
                    Time.timeScale = 0f;
                    Time.fixedDeltaTime = 0.02f * Time.timeScale;
                    SetPauseUI(false);
                    SetPlayerUI(true);
                    Debug.Log("Enter Overlay");
                    break;
            }
        }
        private void OnTogglePause(TogglePauseEvent evt)
        {
            if (_current == GameState.Menu) return;
            if (_current == GameState.InGame) EnterPause();
            else if (_current == GameState.Pause || _current == GameState.Overlay) EnterInGame();
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
    }
}


