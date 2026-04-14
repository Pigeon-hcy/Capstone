using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using QFramework;
using System.Collections.Generic;

namespace SkateGame
{
    public class LevelManager : ViewerControllerBase
    {
        [Header("关卡列表（顺序 = 关卡顺序）")]
        public List<Level> levelList;

        private ILevelModel levelModel;
        private ILevelSystem levelSystem;
        private ILevelProgressModel levelProgressModel;
        private bool isLoadingLevel = false; // 防止重复加载

        public GameObject allLevelCanvas;
        public GameObject basicCanvas;
        private void Awake()
        {
            // Canvas canvas = GetComponent<Canvas>();
            // GameObject camObj = GameObject.Find("Main Camera");
            // Debug.Log("LevelManager Awake: canvas=" + canvas + ", camObj=" + camObj);
            // if (canvas != null && camObj != null)
            //     canvas.worldCamera = camObj.GetComponent<Camera>();
        }

        protected override void InitializeController()
        {
            isLoadingLevel = false;

            levelModel = this.GetModel<ILevelModel>();
            levelSystem = this.GetSystem<ILevelSystem>();
            levelProgressModel = this.GetModel<ILevelProgressModel>();
            this.GetSystem<ILevelProgressSystem>(); // 确保进度已加载

            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            levelModel.LevelList.Clear();
            foreach (Level lvl in levelList)
            {
                levelSystem.AddLevel(lvl);
            }

            // 根据当前场景名称同步 CurrentLevelIndex
            SyncCurrentLevelIndex(currentSceneName);

            InitializeButtons();
            RefreshButtonStatesOnShow();
        }

        /// <summary>
        /// 根据场景名称同步当前关卡索引
        /// </summary>
        private void SyncCurrentLevelIndex(string sceneName)
        {
            for (int i = 0; i < levelModel.LevelList.Count; i++)
            {
                if (levelModel.LevelList[i].SceneName == sceneName)
                {
                    levelModel.CurrentLevelIndex = i;
                    levelModel.CurrentLevelName = levelModel.LevelList[i].Name;
                    Debug.Log($"LevelManager: 同步当前关卡索引为 {i} (场景: {sceneName})");
                    return;
                }
            }
            
            // 如果找不到匹配的场景，默认设置为 0
            if (levelModel.LevelList.Count > 0)
            {
                levelModel.CurrentLevelIndex = 0;
                levelModel.CurrentLevelName = levelModel.LevelList[0].Name;
                Debug.LogWarning($"LevelManager: 未找到场景 '{sceneName}' 对应的关卡，默认设置为索引 0");
            }
        }

        protected override void OnRealTimeUpdate()
        {
            // 可以在这里添加实时更新逻辑
        }

        private void InitializeButtons()
        {
            for (int i = 0; i < levelList.Count; i++)
            {
                int index = i;
                Level lvl = levelList[i];

                if (lvl.button != null)
                {
                    lvl.button.onClick.RemoveAllListeners();
                    // FOR JERRY'S AUDIO - UI CLICK
                    lvl.button.onClick.AddListener(() => OnLevelButtonClick(index));
                }
            }
            UpdateButtonStates();
        }

        /// <summary>
        /// 根据 PassedLevelIndex 更新按钮状态：已通关及之前的关卡可点击，否则不可点击并变暗
        /// </summary>
        private void UpdateButtonStates()
        {
            int passedIndex = levelProgressModel?.PassedLevelIndex ?? -1;
            for (int i = 0; i < levelList.Count; i++)
            {
                Level lvl = levelList[i];
                if (lvl.button != null)
                {
                    bool canClick = i <= passedIndex;
                    lvl.button.interactable = canClick;
                   
                }
            }
        }

        public void ShowAllLevelCanvas()
        {
            basicCanvas.SetActive(false);
            if (allLevelCanvas != null)
            {
                allLevelCanvas.gameObject.SetActive(true);
            }
            RefreshButtonStatesOnShow();
        }
        
        public void HideAllLevelCanvas()
        {
            if (allLevelCanvas != null)
            {
                allLevelCanvas.gameObject.SetActive(false);
            }
            basicCanvas.SetActive(true);
        }

       

        private void OnLevelButtonClick(int index)
        {
            LoadLevel(index);
        }

        public void LoadLevel(int index)
        {
            if (isLoadingLevel) return;
            if (index < 0 || index >= levelModel.LevelList.Count) return;

            // Keep consistent with MainMenu flow: leave menu state before transition,
            // otherwise Time.timeScale may stay at 0 and block fade completion.
            if (GameStateController.Instance != null)
                GameStateController.Instance.EnterInGame();

            isLoadingLevel = true;
            ActionKit.Coroutine(() => LoadLevelWithFade(index)).StartGlobal();
        }

        private static IEnumerator LoadLevelWithFade(int index)
        {
            var levelModel = GameApp.Interface.GetModel<ILevelModel>();
            if (levelModel == null || index < 0 || index >= levelModel.LevelList.Count)
                yield break;

            levelModel.CurrentLevelIndex = index;
            var current = levelModel.LevelList[index];
            levelModel.CurrentLevelName = current.Name;

            bool blackDone = false;
            ActionKit.ScreenTransition.FadeIn()
                .Duration(0.25f)
                .StartGlobal(() => blackDone = true);
            yield return new WaitUntil(() => blackDone);

            var op = SceneManager.LoadSceneAsync(current.SceneName);
            op.allowSceneActivation = false;
            while (op.progress < 0.9f)
                yield return null;

            op.allowSceneActivation = true;
            while (!op.isDone)
                yield return null;
            yield return null;

            ActionKit.ScreenTransition.FadeOut()
                .Duration(0.25f)
                .StartGlobal();

            GameApp.Interface.SendEvent(new SceneChangeEvent());
        }

        public void LoadNextLevel()
        {
            int next = levelModel.CurrentLevelIndex + 1;
            if (next < levelModel.LevelList.Count)
            {
                LoadLevel(next);
            }
            else
            {
                Debug.Log("LoadNextLevel: 已经是最后一关");
            }
        }

        public void LoadPreviousLevel()
        {
            int prev = levelModel.CurrentLevelIndex - 1;
            if (prev >= 0)
            {
                LoadLevel(prev);
            }
            else
            {
                Debug.Log("LevelManager: 已经是第一关");
            }
        }

        public void ReloadCurrentLevel()
        {
            LoadLevel(levelModel.CurrentLevelIndex);
        }

        
        public void RefreshButtons()
        {
            this.GetSystem<ILevelProgressSystem>(); // 确保进度已加载
            InitializeButtons();
        }

        /// <summary>
        /// 打开关卡选择时刷新按钮状态（进度可能已更新）
        /// </summary>
        public void RefreshButtonStatesOnShow()
        {
            UpdateButtonStates();
        }

    }
}

