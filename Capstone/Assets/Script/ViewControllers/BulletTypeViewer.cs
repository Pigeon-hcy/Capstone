using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace SkateGame
{
    [System.Serializable]
    public class BulletSlotUI
    {
        public Image unselectedImage;
        public Image selectedImage;
    }

    public class BulletTypeViewer : ViewerControllerBase
    {
        [Header("四个子弹 UI 槽位，每个槽位有 未选中/选中 两个图片")]
        public BulletSlotUI[] bulletSlots = new BulletSlotUI[4];

        private IPlayerModel playerModel;
        private PlayerConfig lastConfig;
        private int lastIndex = -1;

        protected override void InitializeController()
        {
            base.InitializeController();

            playerModel = this.GetModel<IPlayerModel>();
            lastConfig = playerModel?.Config.Value;

            RefreshUI(force: true);
        }

        protected override void OnRealTimeUpdate()
        {
            if (playerModel == null) return;

            // config 变化了 → 刷新 UI
            if (playerModel.Config.Value != lastConfig)
            {
                lastConfig = playerModel.Config.Value;
                RefreshUI(force: true);
                return;
            }

            RefreshUI();
        }

        private void RefreshUI(bool force = false)
        {
            if (playerModel == null || playerModel.Config.Value == null) return;

            int bulletCount = playerModel.Config.Value.bulletPrefabs.Length;
            int current = Mathf.Clamp(playerModel.CurrentBulletIndex.Value, 0, bulletCount - 1);

            if (!force && current == lastIndex) return;

            for (int i = 0; i < bulletSlots.Length; i++)
            {
                bool slotActive = i < bulletCount;
                var slot = bulletSlots[i];

                if (slot.unselectedImage != null)
                    slot.unselectedImage.gameObject.SetActive(slotActive);

                if (slot.selectedImage != null)
                    slot.selectedImage.gameObject.SetActive(slotActive && i == current);
                
                if (slot.unselectedImage != null && slot.selectedImage != null)
                {
                    // 未选中 = 除了 current 外
                    slot.unselectedImage.enabled = slotActive && i != current;
                    
                    // 选中 = 只有 current
                    slot.selectedImage.enabled = slotActive && i == current;
                }
            }

            lastIndex = current;
        }
    }
}

