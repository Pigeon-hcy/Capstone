using QFramework;

namespace SkateGame
{
    public interface IInputGateSystem : ISystem
    {
        bool PlayerInputBlocked { get; }
        bool UiInputEnabled { get; }
        void SetPlayerInputBlocked(bool blocked);
        void SetUiInputEnabled(bool enabled);
    }
    
    public class InputGateSystem : AbstractSystem, IInputGateSystem
    {
        public bool PlayerInputBlocked { get; private set; }
        public bool UiInputEnabled { get; private set; }

        protected override void OnInit()
        {
            PlayerInputBlocked = false;
            UiInputEnabled = false;
        }

        public void SetPlayerInputBlocked(bool blocked)
        {
            PlayerInputBlocked = blocked;
        }

        public void SetUiInputEnabled(bool enabled)
        {
            UiInputEnabled = enabled;
        }
    }
}


