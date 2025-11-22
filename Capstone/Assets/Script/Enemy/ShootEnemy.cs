using BulletToolKit;
using UnityEngine;
using Hitbox;

namespace SkateGame
{
    public class ShootEnemy : BasicEnemyController
    {
        public BulletFactory bulletFactory;
        protected override void AtkTowardsPlayer(Transform pTrans)
        {
            //TODO: 让so包含子弹生存时间，速度，还有大小
            //base.AtkTowardsPlayer(pTrans);
            Vector2 dir = ((Vector2)pTrans.position - (Vector2)transform.position).normalized;
            DirectionMoveCompoInitData initData = new DirectionMoveCompoInitData(dir, 7f, 5f);
            BaseBullet bullet = bulletFactory.CreateBulletWithDir(transform.position, initData);
            bullet.StartShoot(new HitBoxInitValue(enemyModel.AtkTags.Value, AtkHandler, new Vector2(1f, 1f)));
        }
    }

}
