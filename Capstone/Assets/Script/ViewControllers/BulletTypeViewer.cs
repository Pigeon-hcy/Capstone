using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace SkateGame
{
    /// <summary>
    /// 子弹类型显示 UI
    /// 展示玩家可切换的子弹类型，并高亮当前选中的类型
    /// </summary>
    public class BulletTypeViewer : ViewerControllerBase
    {
        [Header("子弹槽位设置")]
        [Tooltip("按顺序配置三个用于显示子弹图标的 UI Image")]
        public Image[] bulletSlots = new Image[3];

        private IPlayerModel playerModel;
        private PlayerConfig cachedConfig;
        private int lastHighlightedIndex = -1;
        private Image highlightCursor;

        protected override void InitializeController()
        {
            base.InitializeController();

            playerModel = this.GetModel<IPlayerModel>();
            cachedConfig = playerModel?.Config.Value;

            SetupSlots();
            EnsureHighlightCursor();
            UpdateHighlight(force: true);
        }

        protected override void OnRealTimeUpdate()
        {
            if (playerModel == null) return;

            if (cachedConfig != playerModel.Config.Value)
            {
                cachedConfig = playerModel.Config.Value;
                SetupSlots();
                EnsureHighlightCursor();
                UpdateHighlight(force: true);
                return;
            }

            UpdateHighlight();
        }

        private void SetupSlots()
        {
            int availableCount = cachedConfig != null && cachedConfig.bulletPrefabs != null
                ? cachedConfig.bulletPrefabs.Length
                : 0;

            for (int i = 0; i < bulletSlots.Length; i++)
            {
                bool hasSlot = i < availableCount;
                var slot = bulletSlots[i];
                if (slot == null) continue;

                slot.gameObject.SetActive(hasSlot);
                if (!hasSlot)
                {
                    slot.sprite = null;
                    continue;
                }

                var prefab = cachedConfig.bulletPrefabs[i];
                slot.enabled = prefab != null;

                if (prefab == null)
                {
                    slot.sprite = null;
                    continue;
                }

                Sprite iconSprite = ExtractSpriteFromPrefab(prefab);
                slot.sprite = iconSprite;
                slot.SetNativeSize();
            }

            lastHighlightedIndex = Mathf.Clamp(playerModel?.CurrentBulletIndex.Value ?? -1, -1, availableCount - 1);
        }

        private void UpdateHighlight(bool force = false)
        {
            if (playerModel == null) return;

            int bulletCount = cachedConfig?.bulletPrefabs?.Length ?? 0;
            int availableSlots = Mathf.Min(bulletCount, bulletSlots.Length);

            if (availableSlots <= 0)
            {
                lastHighlightedIndex = -1;
                return;
            }

            int currentIndex = Mathf.Clamp(playerModel.CurrentBulletIndex.Value, 0, availableSlots - 1);
            if (!force && currentIndex == lastHighlightedIndex) return;

            if (highlightCursor != null && bulletSlots[currentIndex] != null)
            {
                var targetRect = bulletSlots[currentIndex].rectTransform;
                var cursorRect = highlightCursor.rectTransform;

                cursorRect.SetParent(targetRect, false);
                cursorRect.anchorMin = Vector2.zero;
                cursorRect.anchorMax = Vector2.one;
                cursorRect.offsetMin = Vector2.zero;
                cursorRect.offsetMax = Vector2.zero;
                cursorRect.SetAsFirstSibling();
                highlightCursor.gameObject.SetActive(true);
            }

            lastHighlightedIndex = currentIndex;
        }

        private void EnsureHighlightCursor()
        {
            if (highlightCursor != null) return;

            foreach (Transform child in transform)
            {
                bool isSlot = false;
                foreach (var slot in bulletSlots)
                {
                    if (slot != null && slot.transform == child)
                    {
                        isSlot = true;
                        break;
                    }
                }

                if (isSlot) continue;

                var image = child.GetComponent<Image>();
                if (image != null)
                {
                    highlightCursor = image;
                    break;
                }
            }

            if (highlightCursor == null)
            {
                var cursorObject = new GameObject("HighlightCursor", typeof(RectTransform), typeof(Image));
                cursorObject.transform.SetParent(transform, false);
                highlightCursor = cursorObject.GetComponent<Image>();
                highlightCursor.raycastTarget = false;
                highlightCursor.color = new Color(1f, 1f, 1f, 0.25f);
            }

            highlightCursor.gameObject.SetActive(true);
        }

        private static Sprite ExtractSpriteFromPrefab(GameObject prefab)
        {
            if (prefab == null) return null;

            var spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null) return spriteRenderer.sprite;

            var image = prefab.GetComponentInChildren<Image>();
            if (image != null) return image.sprite;

            return null;
        }
    }
}

