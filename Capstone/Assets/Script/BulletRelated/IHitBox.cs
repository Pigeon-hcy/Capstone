using UnityEngine;
using System.Collections.Generic;


namespace Hitbox
{
    public interface IHitBox
    {
        void OpenBox(List<string> tags, HitboxHandler handler, Vector3 scaleRef);
        void CloseBox();
        GameObject GetGameObject();
    }
    public delegate void HitboxHandler(GameObject obj);

    public abstract class HitBoxFactory : ScriptableObject
    {
        
        public abstract IHitBox CreateHitbox(Transform trans);
        public abstract IHitBox CreateAndOpenHitbox(Transform trans, HitBoxInitValue initV);
    }

    public struct HitBoxInitValue
        {
            public List<string> tags;
            public HitboxHandler Handler;
            public Vector3 scaleRef;
            public HitBoxInitValue(List<string>t, HitboxHandler h, Vector3 f)
            {
                tags = t;
                Handler = h;
                scaleRef = f;
            }
        }
}

