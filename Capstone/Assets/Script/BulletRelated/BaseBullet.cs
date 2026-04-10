using UnityEngine;
using Hitbox;
using BaseUtility;

namespace BulletToolKit
{
    public class BaseBullet : MonoBehaviour, IInteractable
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
        
        public virtual void DoInteraction()
        {
            Debug.Log("触发了");
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Destroy(gameObject);
            }

        }
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

