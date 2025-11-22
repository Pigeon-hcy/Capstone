using UnityEngine;
using System.Collections.Generic;
using QFramework;
using Hitbox;

public class ReportBox : MonoBehaviour, IHitBox
{
    protected List<string> targetTags = new List<string>();

    protected List<GameObject> reported = new List<GameObject>();

    protected HitboxHandler recordHandler;

    public GameObject GetGameObject() => gameObject;

    public void OpenBox(List<string> tags,HitboxHandler hander, Vector3 scale)
    {
        targetTags = tags;
        reported.Clear();
        recordHandler = hander;
        GetComponent<BoxCollider2D>().size = scale;
    }

    public void CloseBox()
    {
        Destroy(gameObject);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (targetTags == null || targetTags.Count == 0)
            return;

        string colTag = collision.tag;

        // 如果这个 tag 在目标列表里
        if (targetTags.Contains(colTag))
        {
            GameObject obj = collision.gameObject;

            // 防止重复添加
            if (!reported.Contains(obj))
            {
                reported.Add(obj);
                recordHandler?.Invoke(obj);
            }
        }
    }
}

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
