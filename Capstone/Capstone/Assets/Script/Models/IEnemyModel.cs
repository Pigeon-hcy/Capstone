using UnityEngine;
using QFramework;

namespace SkateGame
{
    public interface IEnemyModel : IModel
    {
        BindableProperty<EnemyConfig> Config { get; }

        BindableProperty<int> Health { get; }
        BindableProperty<int> MaxHealth { get; }
        BindableProperty<bool> IsAlive { get; }

        BindableProperty<bool> MovingRight { get; }
        BindableProperty<float> MoveSpeed { get; }
        BindableProperty<float> WaitTime { get; }
        BindableProperty<float> PatrolLeftX { get; }
        BindableProperty<float> PatrolRightX { get; }

        BindableProperty<Vector2> Position { get; }

        void ApplyDamage(int amount, DamageType type);
    }

    public class EnemyModel : AbstractModel, IEnemyModel
    {
        public BindableProperty<EnemyConfig> Config { get; } = new BindableProperty<EnemyConfig>(null);

        public BindableProperty<int> Health { get; } = new BindableProperty<int>(100);
        public BindableProperty<int> MaxHealth { get; } = new BindableProperty<int>(100);
        public BindableProperty<bool> IsAlive { get; } = new BindableProperty<bool>(true);

        public BindableProperty<bool> MovingRight { get; } = new BindableProperty<bool>(true);
        public BindableProperty<float> MoveSpeed { get; } = new BindableProperty<float>(2.0f);
        public BindableProperty<float> WaitTime { get; } = new BindableProperty<float>(1.0f);
        public BindableProperty<float> PatrolLeftX { get; } = new BindableProperty<float>(-2.0f);
        public BindableProperty<float> PatrolRightX { get; } = new BindableProperty<float>(2.0f);

        public BindableProperty<Vector2> Position { get; } = new BindableProperty<Vector2>(Vector2.zero);

        protected override void OnInit()
        {
            // 初始化逻辑
        }

        public void ApplyDamage(int amount, DamageType type)
        {
            if (!IsAlive.Value) return;

            Health.Value -= amount;
            if (Health.Value <= 0)
            {
                Health.Value = 0;
                IsAlive.Value = false;
            }
        }
    }
}
