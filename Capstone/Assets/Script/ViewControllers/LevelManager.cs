using UnityEngine;
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
        private bool isLoadingLevel = false; // 防止重复加载

        protected override void InitializeController()
        {
            Debug.Log($"LevelManager.InitializeController: 开始初始化，当前场景 = {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            
            // 重置加载标志
            isLoadingLevel = false;
            
            levelModel = this.GetModel<ILevelModel>();
            levelSystem = this.GetSystem<ILevelSystem>();

            // 保存当前场景名称，用于后续同步索引
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"LevelManager.InitializeController: 当前场景名称 = {currentSceneName}, 当前关卡索引 = {levelModel.CurrentLevelIndex}");

            // 清空旧的 LevelList
            levelModel.LevelList.Clear();

            // 添加到系统
            foreach (Level lvl in levelList)
            {
                levelSystem.AddLevel(lvl);
            }
            Debug.Log($"LevelManager.InitializeController: 已添加 {levelModel.LevelList.Count} 个关卡到列表");

            // 根据当前场景名称同步 CurrentLevelIndex
            SyncCurrentLevelIndex(currentSceneName);

            InitializeButtons();
            
            Debug.Log($"LevelManager.InitializeController: 初始化完成，当前关卡索引 = {levelModel.CurrentLevelIndex}");
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
                    lvl.button.onClick.AddListener(() => OnLevelButtonClick(index));
                }
            }
        }

        private void OnLevelButtonClick(int index)
        {
            LoadLevel(index);
        }

        public void LoadLevel(int index)
        {
            if (isLoadingLevel)
            {
                Debug.LogWarning($"LevelManager.LoadLevel: 正在加载关卡中，忽略重复调用 (索引: {index})");
                return;
            }
            
            Debug.Log($"LevelManager.LoadLevel: 被调用，索引 = {index}, 当前场景 = {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            isLoadingLevel = true;
            levelSystem.LoadLevel(index);
        }

        public void LoadNextLevel()
        {
            Debug.Log($"LoadNextLevel: 当前关卡索引 = {levelModel.CurrentLevelIndex}, 总关卡数 = {levelModel.LevelList.Count}");
            
            int next = levelModel.CurrentLevelIndex + 1;
            if (next < levelModel.LevelList.Count)
            {
                Debug.Log($"LoadNextLevel: 准备加载下一关，索引 = {next}");
                LoadLevel(next);
            }
            else
            {
                Debug.Log($"LoadNextLevel: 已经是最后一关 (当前索引: {levelModel.CurrentLevelIndex}, 总关卡数: {levelModel.LevelList.Count})");
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

        [ContextMenu("刷新按钮")]
        public void RefreshButtons()
        {
            InitializeButtons();
        }
    }
}

