using BulletToolKit;
using UnityEngine;
using Hitbox;
using BaseUtility;

namespace SkateGame
{
    public class ThrowEnemy : BasicEnemyController
    {
        private float bulletSpeed = 7f;
        private float bulletLifeTime = 5f;
        private Vector2 bulletDirection;
        
        [Header("投掷offset")]
        public float throwOffset;
        protected override void Start()
        {
            base.Start();
            bulletSpeed =  (config as ThrowEnemyConfig).bSpeed;
            bulletLifeTime = (config  as ThrowEnemyConfig).bLifeTime;
            bulletDirection = (config as ThrowEnemyConfig).bDirection;
            //Debug.LogError(bulletDirection);
        }
        
       public BulletFactory bulletFactory;


       protected override void AtkTowardsPlayer(Transform pTrans)
       {
           if (bulletFactory == null || pTrans == null)
               return;

           // 1️⃣ 判断左右
           bool playerOnRight = pTrans.position.x >= transform.position.x;

           // 2️⃣ 根据左右计算方向
           Vector3 dir = bulletDirection.normalized;
           //Debug.LogError(dir.x +" , "+ dir.y);
           if (!playerOnRight)
           {
               dir.x = -dir.x;
           }

           // 3️⃣ 组初始化数据
           DirectionMoveCompoInitData initData =
               new DirectionMoveCompoInitData(
                   dir,
                   bulletSpeed,
                   bulletLifeTime
               );

           // 4️⃣ 创建子弹
           BaseBullet bullet =
               bulletFactory.CreateBulletWithDir<GravityDirectionMoveComp>(transform.position+Vector3.up*throwOffset, initData);

           if (bullet == null)
               return;

           // 5️⃣ 发射
           bullet.StartShoot(
               new HitBoxInitValue(
                   enemyModel.AtkTags.Value,
                   new EffectPackage(0),
                   new Vector2(1f, 1f)
               )
           );
           directFirstAtk = false;
       }
    }
}

