using System.Linq;
using UnityEngine;

namespace SkateGame
{
    /// <summary>
    /// 进入触发范围时显示 DialogueViewer，离开时自动关闭
    /// </summary>
    public class NPCTrigger : ViewerControllerBase
    {
        [Header("触发配置")]
        public string playerTag = "Player";
        [Tooltip("可选：直接拖拽 DialogueViewer（建议挂 Canvas 上）。如果留空会自动搜索，包括未激活对象。")]
        public DialogueViewer dialogueViewer;
        private bool Deactived = false;
        private PlayerInputs playerInputs;
        protected override void InitializeController()
        {
            base.InitializeController();

           
        }

        protected override void OnRealTimeUpdate()
        {
          
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Deactived || !other.CompareTag(playerTag)) return;

            if (dialogueViewer != null)
            {
                dialogueViewer.gameObject.SetActive(true);
            }

            if (playerInputs == null)
            {
                playerInputs = other.GetComponent<PlayerInputs>() ?? other.GetComponentInParent<PlayerInputs>();
            }

            playerInputs?.SetShootLock(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;

            Deactived = true;
            playerInputs?.SetShootLock(false);
        }
    }
}