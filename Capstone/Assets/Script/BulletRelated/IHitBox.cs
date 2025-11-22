using UnityEngine;
using System.Collections.Generic;
using BaseUtility;


namespace Hitbox
{
    public interface IHitBox
    {
        void OpenBox(List<string> tags,EffectPackage package, Vector3 scaleRef);
        void CloseBox();
        GameObject GetGameObject();
    }

    public abstract class HitBoxFactory : ScriptableObject
    {
        
        public abstract IHitBox CreateHitbox(Transform trans);
        public abstract IHitBox CreateAndOpenHitbox(Transform trans, HitBoxInitValue initV);
    }

    public struct HitBoxInitValue
        {
            public List<string> tags;
            public EffectPackage package;
            public Vector3 scaleRef;
            public HitBoxInitValue(List<string>t,EffectPackage p, Vector3 f)
            {
                tags = t;
               package = p;
                scaleRef = f;
            }
        }
}

