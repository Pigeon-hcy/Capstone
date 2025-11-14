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

        BindableProperty<float> GuardProcess { get; }
        public BindableProperty<float> DetectRadius {get;}
         public BindableProperty<float> GuardIncreaseSpeed {get;}
         public BindableProperty<float> GuardDecreaseSpeed {get;}
          public BindableProperty<float> JumpAngleModifier {get;}

           public BindableProperty<float> JumpForce {get;}



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

        public BindableProperty<float> GuardProcess {get;} = new BindableProperty<float>(0);
        public BindableProperty<float> DetectRadius {get;} = new BindableProperty<float>(3);
         public BindableProperty<float> GuardIncreaseSpeed {get;} = new BindableProperty<float>(1);
         public BindableProperty<float> GuardDecreaseSpeed {get;} = new BindableProperty<float>(0.5f);

        public BindableProperty<Vector2> Position { get; } = new BindableProperty<Vector2>(Vector2.zero);

        public BindableProperty<float> JumpAngleModifier {get;} = new BindableProperty<float>(0.7f);

           public BindableProperty<float> JumpForce {get;} = new BindableProperty<float>(10f);

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
