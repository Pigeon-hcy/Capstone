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
        Pause
    }

	public class GameStateController : MonoBehaviour, IBelongToArchitecture, ICanGetSystem, ICanRegisterEvent
    {
        public IArchitecture GetArchitecture() => GameApp.Interface;
        private static GameStateController _instance;
        public static GameStateController Instance {get => _instance;}

        [SerializeField] private GameState _current = GameState.Menu;
        public GameState Current => _current;
		[SerializeField] private GameObject pauseUI;
		[SerializeField] private GameObject firstSelected;

        void OnEnable()
        {
            this.RegisterEvent<TogglePauseEvent>(OnTogglePause);
        }

        void OnDisable()
        {
            this.UnRegisterEvent<TogglePauseEvent>(OnTogglePause);
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
					Debug.Log("Enter Menu");
					break;
                case GameState.InGame:
                    gate.SetPlayerInputBlocked(false);
                    gate.SetUiInputEnabled(false);
					Time.timeScale = 1f;
					Time.fixedDeltaTime = 0.02f * Time.timeScale;
					SetPauseUI(false);
                    Debug.Log("Enter InGame");
                    break;
                case GameState.Dialogue:
                    gate.SetPlayerInputBlocked(true);
                    gate.SetUiInputEnabled(true);
					Time.timeScale = 1f;
					Time.fixedDeltaTime = 0.02f * Time.timeScale;
					SetPauseUI(false);
                    Debug.Log("Enter Dialogue");
                    break;
                case GameState.Pause:
                    gate.SetPlayerInputBlocked(true);
                    gate.SetUiInputEnabled(true);
					Time.timeScale = 0f;
					Time.fixedDeltaTime = 0.02f * Time.timeScale;
					SetPauseUI(true);
                    Debug.Log("Enter Pause");
                    break;
            }
        }
        public void OnTogglePause(TogglePauseEvent evt)
        {
            if (_current == GameState.Menu) return;
            if (_current == GameState.InGame) EnterPause();
            else EnterInGame();
        }

		private void SetPauseUI(bool show)
		{
			if (pauseUI != null) pauseUI.SetActive(show);
			if (show) EventSystem.current?.SetSelectedGameObject(firstSelected);
			else if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
		}
    }
}


