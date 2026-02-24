using UnityEngine;
using System.Collections.Generic;
using BaseUtility;
using QFramework;
using Hitbox;
using SkateGame;

namespace SkateGame
{
    public class ReportBox : ViewerControllerBase, IHitBox
    {
        protected List<string> targetTags = new List<string>();

        protected List<GameObject> reported = new List<GameObject>();

        protected EffectPackage recordPackage;

        public GameObject GetGameObject() => gameObject;

        protected override void InitializeController()
        {
            base.InitializeController();
        }

        protected override void OnRealTimeUpdate()
        { 
            
        }

        public void OpenBox(List<string> tags, EffectPackage p, Vector3 scale)
        {
            targetTags = tags;
            reported.Clear();
            recordPackage = p;
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
                
                    DamageSystem.ProcessDamage(recordPackage, null);
                    if (obj.GetComponent<PlayerController>() != null)
                    {
                        Debug.Log("PlayerDie!");
                        var respawnSystem  = this.GetSystem<IRespawnSystem>();
                        if(respawnSystem!=null)
                        {
                            respawnSystem.RespawnPlayer();
                        }
                    }

                }
            }
        }
    }
}
