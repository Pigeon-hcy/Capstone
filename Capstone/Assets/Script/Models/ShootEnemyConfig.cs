using UnityEngine;

namespace SkateGame
{
    [CreateAssetMenu(fileName = "ShootEnemyConfig", menuName = "Game/Shoot Enemy Config")]
    public class ShootEnemyConfig : EnemyConfig
    {
        [Header("子弹相关")]
        public float bSpeed;

        public float bLifeTime;
    }
}

