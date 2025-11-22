using UnityEngine;
using Hitbox;
using BaseUtility;

namespace BulletToolKit
{
    public class BaseBullet : MonoBehaviour
    {
        public HitBoxFactory hitBoxFactory;
        protected IHitBox hitBox;
        protected IMoveCompo moveCompo;

        public virtual void StartShoot(HitBoxInitValue initV)
        {
            if(hitBox != null)
            {
                Destroy(hitBox.GetGameObject());
            }
            initV.package.AddEffect(new DestroyEffectDeco(gameObject));
            hitBox = hitBoxFactory.CreateAndOpenHitbox(transform, initV);
            moveCompo?.AllowMove();
        }

        public void RecordMoveCompo(IMoveCompo moveCompo)
        {
            this.moveCompo = moveCompo;
        }
    }

    public abstract class BulletFactory : ScriptableObject
    {
        public abstract BaseBullet CreateBulletWithTarget(Vector3 pos, TargetMoveCompInitData initData);
        public abstract BaseBullet CreateBulletWithDir(Vector3 pos, DirectionMoveCompoInitData initData);
    }

    public class DestroyEffectDeco : IEPDecoration
    {
        private GameObject self;
        public DestroyEffectDeco(GameObject gmo)
        {
            self = gmo;
        }

        public void Decorate(ITarget target)
        {
            GameObject.Destroy(self);
        }
    }
}

