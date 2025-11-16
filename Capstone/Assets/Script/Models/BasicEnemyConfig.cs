using System.Collections.Generic;
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

        [Header("警戒速度")]
        public float guardIncreaseSpeed = 1;
        public float guardDecreaseSpeed = 0.5f;
        [Header("警戒举例")]
        public float detectRadius = 3;

        [Header("扑的角度修改")]
        public float jumpAngleModifier = 0.7f;

        [Header("扑的力度")]
         public float jumpForce = 10f;

         [Header("扑之后攻击box启动时间")]
         public float JumpAtkBoxActiveTime = 3;

         [Header("攻击的tag")]
         public List<string> AtkTags;
    }
}
