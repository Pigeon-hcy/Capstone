using BaseUtility;
using BulletToolKit;
using UnityEngine;
using Hitbox;

namespace SkateGame
{
    public class ShootEnemy : BasicEnemyController
    {
        private float bulletSpeed = 7f;
        private float bulletLifeTime = 5f;
        public GameObject lineObject;
        public GameObject headObject;
        public SpriteRenderer headRenderer;
        public Sprite shootSprite;
        public Sprite closeSprite;
        public Transform shootPos;

        public ParticleSystem effect;

        private LineRenderer lRenderer;
        protected override void Start()
        {
            base.Start();
            bulletSpeed = (config as ShootEnemyConfig).bSpeed;
            bulletLifeTime = (config  as ShootEnemyConfig).bLifeTime;
            lRenderer = lineObject.GetComponent<LineRenderer>();
        }

        public BulletFactory bulletFactory;
        protected override void AtkTowardsPlayer(Transform pTrans)
        {
            //Debug.LogError("发出子弹");
            //base.AtkTowardsPlayer(pTrans);
            Vector2 dir = ((Vector2)pTrans.position - (Vector2)shootPos.position).normalized;
            DirectionMoveCompoInitData initData = new DirectionMoveCompoInitData(dir, bulletSpeed, bulletLifeTime);
            BaseBullet bullet = bulletFactory.CreateBulletWithDir<DirectionMoveCompo>(shootPos.position, initData);
            bullet.StartShoot(new HitBoxInitValue(AtkTags, new EffectPackage(0), new Vector2(0.5f, 0.5f)));
            directFirstAtk = false;
        }

        protected override void Guard(Transform pTrans, float guard)
        {
            base.Guard(pTrans, guard);
            if (GuardProcess >= 1f && Vector2.Distance(pTrans.position, transform.position) < 5)
            {
                GuardProcess = 0.9f;
            }

            lineObject.SetActive(true);
            var start = lRenderer.startColor;
            var end   = lRenderer.endColor;

            float a = Mathf.Clamp01(guard);

            start.a = a;
            end.a   = a;

            lRenderer.startColor = start;
            lRenderer.endColor   = end;
            
            Vector2 dir = ((Vector2)pTrans.position - (Vector2)shootPos.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            
            headObject.transform.rotation = Quaternion.Euler(0, 0, angle+((displayTrans.localScale.x>0)?180:0));
            headRenderer.sprite = shootSprite;
            if(!effect.isPlaying)effect.Play();

        }

        protected override void UnGuard()
        {
            base.UnGuard();
            lineObject.SetActive(false);
            headRenderer.sprite = closeSprite;
            headObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            effect.Stop();
        }

        protected override void Flip(FlipInfoRecorder recorder)
        {
            if(!recorder.inGuard)
                base.Flip(recorder);
            else
            {
                Vector3 scale = displayTrans.localScale;

                bool playerIsOnLeft = recorder.playerPos.x < transform.position.x;

                if (playerIsOnLeft)
                    scale.x = Mathf.Abs(scale.x);       // 玩家在左 → 正
                else
                    scale.x = -Mathf.Abs(scale.x);      // 玩家在右 → 负

                displayTrans.localScale = scale;
            }
        }
    }

}
