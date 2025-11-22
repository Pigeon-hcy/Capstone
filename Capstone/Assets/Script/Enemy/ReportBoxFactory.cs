using Hitbox;
using UnityEngine;

[CreateAssetMenu(fileName = "ReportBoxFactory", menuName = "Factory/ReportBoxFactory")]
public class ReportBoxFactory : HitBoxFactory
{
    public GameObject prefab;
    public override IHitBox CreateHitbox(Transform trans)
    {
        GameObject go = Instantiate(prefab, trans);
        return go.GetComponent<IHitBox>();
    }

    public override IHitBox CreateAndOpenHitbox(Transform trans, HitBoxInitValue initV)
    {
        IHitBox box = CreateHitbox(trans);
        box.OpenBox(initV.tags, initV.Handler, initV.scaleRef);
        return box;
    }
}