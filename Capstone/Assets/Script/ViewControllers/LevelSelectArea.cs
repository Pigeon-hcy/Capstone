using UnityEngine;
using UnityEngine.SceneManagement;
using QFramework;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using MoreMountains.Feedbacks;

namespace SkateGame
{
    public class LevelSelectArea : ViewerControllerBase
    {
        private Canvas levelSelectCanvas;
        public string playerTag = "Player";
        private PlayerController playerController;   // 玩家控制器引用
        private ILevelProgressSystem levelProgressSystem;  // 关卡进度系统
        private ILevelProgressModel levelProgressModel;
        private ILevelModel levelModel;
        private ITimerSystem timerSystem;
        
        EndUIController endUIController;
        
        protected override void OnEnable()
        {
            Debug.Log("LevelSelectArea: OnEnable");
            base.OnEnable();
            this.RegisterEvent<GameStateChangedEvent>(OnGameStateChanged);
        }
        protected override void OnDisable()
        {
            this.UnRegisterEvent<GameStateChangedEvent>(OnGameStateChanged);
            base.OnDisable();
        }
        void OnGameStateChanged(GameStateChangedEvent e)
        {
            if (e.NewState == GameState.InGame) HideCanvas();
        }

        protected override void InitializeController()
        {
            // 获取关卡进度系统
            levelProgressSystem = this.GetSystem<ILevelProgressSystem>();
            levelProgressModel = this.GetModel<ILevelProgressModel>();
            levelModel = this.GetModel<ILevelModel>();
            timerSystem = this.GetSystem<ITimerSystem>();
            
            // 查找关卡 UI 画布
            GameObject canvasObject = GameObject.Find("LevelCanvas");
            if (canvasObject != null)
            {
                levelSelectCanvas = canvasObject.GetComponent<Canvas>();
                if (levelSelectCanvas != null)
                {
                    levelSelectCanvas.gameObject.SetActive(false);
                    endUIController = levelSelectCanvas.GetComponentInChildren<EndUIController>();
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
                // 停止计时
                timerSystem?.StopTimer();
                // FOR JERRY'S AUDIO - LEVEL FINISH
                // ----------------------------
                // 保存当前关卡进度（到达终点时保存）
                // ----------------------------
                if (levelProgressSystem != null)
                {
                    string sceneName = SceneManager.GetActiveScene().name;
                    levelProgressSystem.SaveLevel();
                    
                    // Debug: 输出所有已通关的关卡（save 过的）
                    int passed = levelProgressModel.PassedLevelIndex;
                    var passedLevels = new List<string>();
                    for (int i = 0; i <= passed && i < levelModel.LevelList.Count; i++)
                    {
                        var lvl = levelModel.LevelList[i];
                        passedLevels.Add($"[{i}] {lvl.Name}({lvl.SceneName})");
                    }
                    Debug.Log($"LevelSelectArea: 已保存 | 当前场景={sceneName} | PassedLevelIndex={passed} | 已通关关卡: {string.Join(", ", passedLevels)}");
                }


                // 显示 UI
                ShowCanvas();

                // endUIController.transform.parent.gameObject.SetActive(true);
                // endUIController.gameObject.SetActive(true);
                // endUIController.EndSequence_A(other);

                /*
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
                    playerController.rb.linearVelocity = Vector2.zero;
                }
                GameStateController.Instance.EnterUIPause();
                */
                GameStateController.Instance?.EnterTutorial();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
            {
                Debug.Log("玩家离开 Level Trigger");

                // 隐藏 UI
                HideCanvas();
                GameStateController.Instance?.EnterInGame();

                // ----------------------------
                // 恢复射击
                // ----------------------------
                if (playerController != null)
                {
                    playerController.disableInput = false;
                }

                playerController = null;
                GameStateController.Instance?.EnterInGame();
            }
        }

        private void ShowCanvas()
        {
            if (levelSelectCanvas != null)
            {
                levelSelectCanvas.gameObject.SetActive(true);
                Transform mmfEndFeedback = levelSelectCanvas.transform.Find("MMF_EndFeedback");
                if (mmfEndFeedback != null)
                {
                    MMF_Player mmfPlayer = mmfEndFeedback.GetComponent<MMF_Player>();
                    if (mmfPlayer != null)
                        mmfPlayer.PlayFeedbacks();
                }
                // 找到Canvas下第一个可被选中的Selectable 
                // TODO: 指定Selectable
                var selectables = levelSelectCanvas.GetComponentsInChildren<UnityEngine.UI.Selectable>();
                if (selectables != null && selectables.Length > 0)
                    EventSystem.current?.SetSelectedGameObject(selectables[0].gameObject);
                Debug.Log("LevelCanvas 显示");
            }
        }

        private void HideCanvas()
        {
            if (levelSelectCanvas != null)
            {
                levelSelectCanvas.gameObject.SetActive(false);
                EventSystem.current?.SetSelectedGameObject(null);
                Debug.Log("LevelCanvas 隐藏");
            }
        }
    }
}
