using QFramework;

namespace SkateGame
{
    public interface ITimerModel : IModel
    {
        BindableProperty<float> ElapsedTime { get; }
        BindableProperty<bool> IsRunning { get; }
    }

    public class TimerModel : AbstractModel, ITimerModel
    {
        public BindableProperty<float> ElapsedTime { get; } = new BindableProperty<float>(0f);
        public BindableProperty<bool> IsRunning { get; } = new BindableProperty<bool>(false);

        protected override void OnInit()
        {
        }
    }
}
