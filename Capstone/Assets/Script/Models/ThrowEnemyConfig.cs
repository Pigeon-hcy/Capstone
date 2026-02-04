using UnityEngine;


namespace SkateGame
{
    [CreateAssetMenu(fileName = "ThrowEnemyConfig", menuName = "Game/Throw Enemy Config")]
    public class ThrowEnemyConfig : EnemyConfig
    {
        [Header("子弹相关")] public float bSpeed;
        public Vector2 bDirection;
        public float bLifeTime;
    }

}

