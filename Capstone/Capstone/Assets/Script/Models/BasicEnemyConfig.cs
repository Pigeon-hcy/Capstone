using UnityEngine;

namespace SkateGame
{
    [CreateAssetMenu(fileName = "BasicEnemyConfig", menuName = "Game/Basic Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("巡逻")]
       public float moveDuration   = 2f;   
        public float moveSpeed       = 2f;
        public float waitTime        = 1.0f; // 到边界停顿

        [Header("初始")]
        public int   maxHealth       = 30;
        public bool  startFacingRight = true;

        [Header("物理")]
        public float gravityScale    = 2f;   // 地面型用得到；飞行型可设为 0
    }
}
