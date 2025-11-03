using QFramework;

namespace SkateGame
{
    public interface IGameModel : IModel
    {
        BindableProperty<bool> IsPaused { get; }
        BindableProperty<bool> IsGameOver { get; }
    }

    public class GameModel : AbstractModel, IGameModel
    {
        public BindableProperty<bool> IsPaused { get; } = new BindableProperty<bool>(false);
        public BindableProperty<bool> IsGameOver { get; } = new BindableProperty<bool>(false);
        
        protected override void OnInit()
        {
            // 初始化逻辑
        }
    }
}
