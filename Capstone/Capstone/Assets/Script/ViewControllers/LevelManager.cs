using UnityEngine;
using UnityEngine.UI;
using QFramework;
using System.Collections.Generic;

namespace SkateGame
{
    /// <summary>
    /// 关卡管理器
    /// 负责关卡的切换、管理和逻辑控制
    /// </summary>
    public class LevelManager : ViewerControllerBase
    {
        [Header("关卡列表（包含按钮）")]
        public List<Level> levelList;
        
        private ILevelModel levelModel;
        private ILevelSystem levelSystem;
        
        
        protected override void InitializeController()
        {
            levelModel = this.GetModel<ILevelModel>();
            levelSystem = this.GetSystem<ILevelSystem>();
            
            // 将 Inspector 中配置的关卡添加到系统
            if (levelList != null && levelList.Count > 0)
            {
                foreach (Level level in levelList)
                {
                    levelSystem.AddLevel(level);
                }
                Debug.Log($"LevelManager: 已添加 {levelList.Count} 个关卡");
            }
            
            // 初始化按钮
            InitializeButtons();
        }
        
        protected override void OnRealTimeUpdate()
        {
            // 可以在这里添加实时更新逻辑
        }
        
        
        public void LoadLevel(int index)
        {
            levelModel.CurrentLevelIndex = index;
            Debug.Log($"LevelManager: 加载关卡 {levelModel.CurrentLevelIndex}");
            levelSystem.LoadLevel(index);

        }
        
        /// <summary>
        /// 加载下一关
        /// </summary>
        public void LoadNextLevel()
        {
            int nextIndex = levelModel.CurrentLevelIndex + 1;
            
            if (nextIndex < levelModel.LevelList.Count)
            {
                levelModel.CurrentLevelIndex = nextIndex;
                levelSystem.LoadLevel(nextIndex);
            }
            else
            {
                Debug.LogWarning("LevelManager: 已经是最后一关了！");
            }
        }
        
        /// <summary>
        /// 重新加载当前关卡
        /// </summary>
        public void ReloadCurrentLevel()
        {
            levelSystem.LoadLevel(levelModel.CurrentLevelIndex);
        }
        
        /// <summary>
        /// 返回上一关
        /// </summary>
        public void LoadPreviousLevel()
        {
            int prevIndex = levelModel.CurrentLevelIndex - 1;
            
            if (prevIndex >= 0)
            {
                levelSystem.LoadLevel(prevIndex);
            }
            else
            {
                Debug.LogWarning("LevelManager: 已经是第一关了！");
            }
        }
        
        #region Button Management
        
        /// <summary>
        /// 初始化所有按钮
        /// </summary>
        private void InitializeButtons()
        {
            
            
            foreach (var level in levelList)
            {
                if (level.button != null)
                {
                    // 为每个按钮添加点击事件
                    int levelIndex = level.Index; // 创建局部变量，避免闭包问题
                    level.button.onClick.RemoveAllListeners(); // 清除旧的监听器
                    level.button.onClick.AddListener(() => OnLevelButtonClick(levelIndex));
                    
                  
                }
            }
            
            //Debug.Log($"LevelManager: 已初始化 {levelList.Count} 个关卡按钮");
        }
        
        /// <summary>
        /// 按钮点击事件
        /// </summary>
        private void OnLevelButtonClick(int levelIndex)
        {
            //Debug.Log($"LevelManager: 点击了关卡按钮，Index: {levelIndex}");
            LoadLevel(levelIndex);
        }
        
        /// <summary>
        /// 手动刷新按钮（可在Inspector中调用）
        /// </summary>
        [ContextMenu("刷新按钮")]
        public void RefreshButtons()
        {
            InitializeButtons();
        }
        
       
        
        #endregion
    }
    
}

