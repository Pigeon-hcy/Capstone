using UnityEngine;

namespace BulletToolKit
{
    [CreateAssetMenu(fileName = "BaseBulletFact", menuName = "Factory/BaseBulletFact")]
    public class BaseBulletFactory : BulletFactory
    {
        public GameObject prefab;

        public override BaseBullet CreateBulletWithTarget(Vector3 pos, TargetMoveCompInitData initData)
        {
            GameObject go = Instantiate(prefab, pos, Quaternion.identity);
            TargetMoveComp comp = go.AddComponent<TargetMoveComp>();
            comp.Init(initData.target, initData.speed, initData.lifeTime, initData.ifTwoD);
            comp.BanMove();
            go.GetComponent<BaseBullet>().RecordMoveCompo(comp);
            return go.GetComponent<BaseBullet>();
        }

        public override BaseBullet CreateBulletWithDir(Vector3 pos, DirectionMoveCompoInitData initData)
        {
            GameObject go = Instantiate(prefab, pos, Quaternion.identity);
            DirectionMoveCompo comp = go.AddComponent<DirectionMoveCompo>();
            comp.Init(initData.dir, initData.speed, initData.lifeTime);
            comp.BanMove();
            go.GetComponent<BaseBullet>().RecordMoveCompo(comp);
            return go.GetComponent<BaseBullet>();
        }
    }
}