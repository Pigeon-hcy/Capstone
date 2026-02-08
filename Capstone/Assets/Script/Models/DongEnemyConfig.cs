using UnityEngine;

namespace SkateGame
{
    [CreateAssetMenu(fileName = "DongEnemyConfig", menuName = "Game/Dong Enemy Config")]
    public class DongEnemyConfig : EnemyConfig
    {
        [Header("下落相关")] public float checkWidth;
        public AnimationCurve fallCurve;
        public float fallTime;
        public AnimationCurve resetCurve;
        public float resetTime;
        public float resetDelay;
    }

}

