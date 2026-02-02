using UnityEngine;
using QFramework;

namespace SkateGame
{
    public class LevelSelectArea : ViewerControllerBase
    {
        private Canvas levelSelectCanvas;
        public string playerTag = "Player";

        private PlayerController playerController;   // 玩家控制器引用
        private ILevelProgressSystem levelProgressSystem;  // 关卡进度系统
        
        protected override void InitializeController()
        {
            // 获取关卡进度系统
            levelProgressSystem = this.GetSystem<ILevelProgressSystem>();
            
            // 查找关卡 UI 画布
            GameObject canvasObject = GameObject.Find("LevelCanvas");
            if (canvasObject != null)
            {
                levelSelectCanvas = canvasObject.GetComponent<Canvas>();
                if (levelSelectCanvas != null)
                {
                    levelSelectCanvas.gameObject.SetActive(false);
                    Debug.Log("LevelSelectArea: LevelCanvas 初始化成功");
                }
            }
        }

        protected override void OnRealTimeUpdate()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                ShowCanvas();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
            {
                Debug.Log("玩家进入 Level Trigger");

                // ----------------------------
                // 保存当前关卡进度（到达终点时保存）
                // ----------------------------
                if (levelProgressSystem != null)
                {
                    levelProgressSystem.SaveLevel();
                    Debug.Log("LevelSelectArea: 已保存关卡进度");
                }

                // 显示 UI
                ShowCanvas();

                // ----------------------------
                // ① 清零玩家速度，避免滑过去
                // ----------------------------
                Rigidbody2D rb = other.attachedRigidbody;
                if (rb == null) rb = other.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }

                // ----------------------------
                // ② 禁止射击（使用 disableInput）
                // ----------------------------
                playerController = other.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.disableInput = true;
                }
                /////////////////////////////////////////////////Pause/////////////////////////////////////////////////////////////
                playerController.rb.linearVelocity = Vector2.zero;
                Time.timeScale = 0f;
				Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
            {
                Debug.Log("玩家离开 Level Trigger");

                // 隐藏 UI
                HideCanvas();

                // ----------------------------
                // 恢复射击
                // ----------------------------
                if (playerController != null)
                {
                    playerController.disableInput = false;
                }

                playerController = null;
            }
        }

        private void ShowCanvas()
        {
            if (levelSelectCanvas != null)
            {
                levelSelectCanvas.gameObject.SetActive(true);
                Debug.Log("LevelCanvas 显示");
            }
        }

        private void HideCanvas()
        {
            if (levelSelectCanvas != null)
            {
                levelSelectCanvas.gameObject.SetActive(false);
                Debug.Log("LevelCanvas 隐藏");
            }
        }
    }
}
