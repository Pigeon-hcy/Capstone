using UnityEngine;
using UnityEngine.UI;
using QFramework;
using MoreMountains.Feedbacks;

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

        [Header("选中后要吸到的中心点（比如 BulletPanel）")]
        public Transform bulletSlotParent;

        [Header("动画参数")]
        public float moveSpeed = 6f;
        public float fadeSpeed = 4f;
        public float scaleSpeed = 6f;
        public float animationDelay = 1f;   // 切换完成后等待 1s 再开始动画
        public float selectedScale = 1.4f; // 放大倍数

        private IPlayerModel playerModel;
        private PlayerConfig lastConfig;
        private int lastIndex = -1;
        private int currentIndex = -1;

        // 记录每个槽位的初始位置和缩放（用 selected 的 transform，当它没有时用 unselected）
        private Vector3[] originalPos;
        private Vector3[] originalScale;

        // 动画状态
        private bool isAnimating = false;
        private float delayTimer = 0f;

        [Header("MMF效果")]
        public MMF_Player onFireSelectedEffect;
        public MMF_Player onFireActiveEffect;

        public MMF_Player onIceSelectedEffect;
        public MMF_Player onIceActiveEffect;

        public MMF_Player onVineSelectedEffect;
        public MMF_Player onVineActiveEffect;

        /// <summary>
        /// 获取当前选择的子弹索引（0-3）
        /// </summary>
        public int GetCurrentBulletIndex()
        {
            if (playerModel == null || playerModel.Config.Value == null) return -1;
            int bulletCount = playerModel.Config.Value.bulletPrefabs.Length;
            return Mathf.Clamp(playerModel.CurrentBulletIndex.Value, 0, bulletCount - 1);
        }

        /// <summary>
        /// 获取当前选择的子弹Prefab
        /// </summary>
        public GameObject GetCurrentBulletPrefab()
        {
            int index = GetCurrentBulletIndex();
            if (index < 0 || playerModel == null || playerModel.Config.Value == null) return null;
            if (index >= playerModel.Config.Value.bulletPrefabs.Length) return null;
            return playerModel.Config.Value.bulletPrefabs[index];
        }

        /// <summary>
        /// 获取当前选择的UI槽位
        /// </summary>
        public BulletSlotUI GetCurrentBulletSlot()
        {
            int index = GetCurrentBulletIndex();
            if (index < 0 || index >= bulletSlots.Length) return null;
            return bulletSlots[index];
        }

        protected override void InitializeController()
        {
            base.InitializeController();

            playerModel = this.GetModel<IPlayerModel>();
            lastConfig = playerModel?.Config.Value;

            CacheOriginalTransform();
            RefreshUI(true);
        }

        private void CacheOriginalTransform()
        {
            int len = bulletSlots.Length;
            originalPos = new Vector3[len];
            originalScale = new Vector3[len];

            for (int i = 0; i < len; i++)
            {
                var slot = bulletSlots[i];
                Transform t = null;

                if (slot.selectedImage != null)
                    t = slot.selectedImage.transform;
                else if (slot.unselectedImage != null)
                    t = slot.unselectedImage.transform;

                if (t != null)
                {
                    originalPos[i] = t.position;
                    originalScale[i] = t.localScale;
                }
                else
                {
                    originalPos[i] = Vector3.zero;
                    originalScale[i] = Vector3.one;
                }
            }
        }

        protected override void OnRealTimeUpdate()
        {
            if (playerModel == null) return;

            // 配置变了重新刷新
            if (playerModel.Config.Value != lastConfig)
            {
                lastConfig = playerModel.Config.Value;
                RefreshUI(true);
                return;
            }

            RefreshUI();

            // 延迟阶段：只等时间，不做动画
            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;
                if (delayTimer <= 0f)
                {
                    isAnimating = true;
                    
                    // 播放确认选择特效（延迟结束后，动画开始时）
                    if (onFireActiveEffect != null && GetCurrentBulletIndex() == 0 && playerModel.CurrentBulletIndex.Value == 0)
                    {
                        onFireActiveEffect.PlayFeedbacks();
                    }
                }
                return;
            }

            if (isAnimating)
                UpdateAnimation(Time.deltaTime);
        }

        private void RefreshUI(bool force = false)
        {
            if (playerModel == null || playerModel.Config.Value == null) return;

            int bulletCount = playerModel.Config.Value.bulletPrefabs.Length;
            int now = Mathf.Clamp(playerModel.CurrentBulletIndex.Value, 0, bulletCount - 1);

            if (!force && now == lastIndex) return;

            currentIndex = now;

            // ★ 每次切换时：先把所有框恢复到原始位置 & 缩放
            ResetAllTransforms(bulletCount, now);

            // ★ 刚切换完，先显示 4 个在原位置，等 1 秒后才开始动画
            delayTimer = animationDelay;
            isAnimating = false;

            // 播放选择特效（玩家刚选择到的时候）
            if (onFireSelectedEffect != null && GetCurrentBulletIndex() == 0 && playerModel.CurrentBulletIndex.Value == 0)
            {
                onFireSelectedEffect.PlayFeedbacks();
            }else if (onIceSelectedEffect != null && GetCurrentBulletIndex() == 1 && playerModel.CurrentBulletIndex.Value == 1)
            {
                onIceSelectedEffect.PlayFeedbacks();
            }else if (onVineSelectedEffect != null && GetCurrentBulletIndex() == 2 && playerModel.CurrentBulletIndex.Value == 2)
            {
                onVineSelectedEffect.PlayFeedbacks();
            }

            lastIndex = now;
        }

        private void ResetAllTransforms(int bulletCount, int current)
        {
            for (int i = 0; i < bulletSlots.Length; i++)
            {
                bool active = i < bulletCount;
                var slot = bulletSlots[i];

                if (slot.unselectedImage != null)
                {
                    slot.unselectedImage.gameObject.SetActive(active);
                    slot.unselectedImage.transform.position = originalPos[i];
                    slot.unselectedImage.transform.localScale = originalScale[i];
                    var c = slot.unselectedImage.color;
                    c.a = 1f;
                    slot.unselectedImage.color = c;
                    slot.unselectedImage.enabled = active && i != current;
                }

                if (slot.selectedImage != null)
                {
                    slot.selectedImage.gameObject.SetActive(active);
                    slot.selectedImage.transform.position = originalPos[i];
                    slot.selectedImage.transform.localScale = originalScale[i];
                    var c = slot.selectedImage.color;
                    c.a = (i == current) ? 1f : 0f; // 当前的亮，其他透明
                    slot.selectedImage.color = c;
                    slot.selectedImage.enabled = active && i == current;
                }
            }
        }

        private void UpdateAnimation(float dt)
        {
            if (currentIndex < 0 || currentIndex >= bulletSlots.Length) return;
            if (bulletSlotParent == null) return;

            for (int i = 0; i < bulletSlots.Length; i++)
            {
                var slot = bulletSlots[i];
                if (slot.unselectedImage == null && slot.selectedImage == null) continue;

                bool isCurrent = (i == currentIndex);

                // 选中那个：把 selectedImage 移到中心并放大
                if (slot.selectedImage != null)
                {
                    if (isCurrent)
                    {
                        Transform tSel = slot.selectedImage.transform;

                        tSel.position = Vector3.Lerp(
                            tSel.position,
                            bulletSlotParent.position,
                            dt * moveSpeed
                        );

                        tSel.localScale = Vector3.Lerp(
                            tSel.localScale,
                            Vector3.one * selectedScale,
                            dt * scaleSpeed
                        );

                        var cSel = slot.selectedImage.color;
                        cSel.a = Mathf.Lerp(cSel.a, 1f, dt * fadeSpeed);
                        slot.selectedImage.color = cSel;
                        slot.selectedImage.enabled = true;
                    }
                    else
                    {
                        // 非当前：selected 持续保持原位置，只做淡出
                        var cSel = slot.selectedImage.color;
                        cSel.a = Mathf.Lerp(cSel.a, 0f, dt * fadeSpeed);
                        slot.selectedImage.color = cSel;
                        if (cSel.a <= 0.02f)
                            slot.selectedImage.enabled = false;
                    }
                }

                // 所有未选中的 unselected 都只淡出，不移动位置
                if (slot.unselectedImage != null)
                {
                    var cUn = slot.unselectedImage.color;

                    if (isCurrent)
                    {
                        cUn.a = Mathf.Lerp(cUn.a, 0f, dt * fadeSpeed);
                        if (cUn.a <= 0.02f)
                            slot.unselectedImage.enabled = false;
                    }
                    else
                    {
                        cUn.a = Mathf.Lerp(cUn.a, 0f, dt * fadeSpeed);
                        if (cUn.a <= 0.02f)
                            slot.unselectedImage.enabled = false;
                    }

                    slot.unselectedImage.color = cUn;
                }
            }
        }
    }
}
