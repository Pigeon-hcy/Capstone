using QFramework;
using UnityEngine;

namespace SkateGame
{
    public interface IEnemyAssetSystem : ISystem
    {
        void SetEnemyConfig(EnemyConfig config);
    }

    public class EnemyAssetSystem : AbstractSystem, IEnemyAssetSystem
    {
        private EnemyConfig enemyConfig;
        private IEnemyModel enemyModel;

        protected override void OnInit()
        {
            enemyModel = this.GetModel<IEnemyModel>();
        }

        public void SetEnemyConfig(EnemyConfig config)
        {
            enemyConfig = config;
            enemyModel.Config.Value = enemyConfig;
        }
    }
}
