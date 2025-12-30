using UnityEngine;
using QFramework;

namespace SkateGame
{
    public enum GameState
    {
        InGame,
        Dialogue,
        Pause
    }

    public class GameStateController : MonoBehaviour, IBelongToArchitecture, ICanGetSystem, ICanRegisterEvent
    {
        public IArchitecture GetArchitecture() => GameApp.Interface;

        [SerializeField] private GameState _current = GameState.InGame;
        public GameState Current => _current;

        void OnEnable()
        {
            this.RegisterEvent<TogglePauseEvent>(OnTogglePause);
        }

        void OnDisable()
        {
            this.UnRegisterEvent<TogglePauseEvent>(OnTogglePause);
        }
        void Awake()
        {
            ApplyState(_current);
        }

        public void EnterInGame() => Switch(GameState.InGame);
        public void EnterDialogue() => Switch(GameState.Dialogue);
        public void EnterPause() => Switch(GameState.Pause);

        public void Switch(GameState next)
        {
            if (_current == next) return;
            _current = next;
            ApplyState(_current);
        }

        private void ApplyState(GameState s)
        {
            var gate = this.GetSystem<IInputGateSystem>();
            switch (s)
            {
                case GameState.InGame:
                    gate.SetPlayerInputBlocked(false);
                    gate.SetUiInputEnabled(false);
                    Debug.Log("Enter InGame");
                    break;
                case GameState.Dialogue:
                    gate.SetPlayerInputBlocked(true);
                    gate.SetUiInputEnabled(true);
                    Debug.Log("Enter Dialogue");
                    break;
                case GameState.Pause:
                    gate.SetPlayerInputBlocked(true);
                    gate.SetUiInputEnabled(true);
                    Debug.Log("Enter Pause");
                    break;
            }
        }
        private void OnTogglePause(TogglePauseEvent evt)
        {
            if (_current == GameState.InGame) EnterPause();
            else EnterInGame();
        }
    }
}


