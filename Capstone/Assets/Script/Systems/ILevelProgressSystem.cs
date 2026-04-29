using QFramework;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace SkateGame
{
    public interface ILevelProgressSystem : ISystem
    {
        void SaveLevel();
        void LoadLevelProgress();
        bool HasWatchedCutscene(int levelIndex);
        void MarkCutsceneWatched(int levelIndex);
    }

    [System.Serializable]
    public class LevelProgressData
    {
        public int passedLevelIndex = -1;
        public int lastClearedLevelIndex = -1;
        public List<int> watchedCutscenes = new List<int>();
    }

    public class LevelProgressSystem : AbstractSystem, ILevelProgressSystem
    {
        private ILevelProgressModel levelProgressModel;
        private ILevelModel levelModel;
        private const string SAVE_FILE_NAME = "LevelProgress.json";
        
        protected override void OnInit()
        {
            levelProgressModel = this.GetModel<ILevelProgressModel>();
            levelModel = this.GetModel<ILevelModel>();
            
            // 游戏开始时自动加载进度
            LoadLevelProgress();
        }
        
        public bool HasWatchedCutscene(int levelIndex)
        {
            return levelProgressModel.WatchedCutscenes.Contains(levelIndex);
        }

        public void MarkCutsceneWatched(int levelIndex)
        {
            if (levelProgressModel.WatchedCutscenes.Contains(levelIndex)) return;
            levelProgressModel.WatchedCutscenes.Add(levelIndex);
            SaveToDisk();
        }

        public void SaveLevel()
        {
            // 获取当前关卡索引
            int currentLevelIndex = levelModel.CurrentLevelIndex;

            // 记录“最近一次通关”的关卡（每次通关都刷新）
            levelProgressModel.LastClearedLevelIndex = currentLevelIndex;
            
            // 如果当前关卡索引大于已通关的关卡索引，则更新
            if (currentLevelIndex > levelProgressModel.PassedLevelIndex)
            {
                levelProgressModel.PassedLevelIndex = currentLevelIndex;
            }
            
            // 保存到磁盘
            SaveToDisk();
            
            Debug.Log($"LevelProgressSystem: 已保存关卡进度，最高通关={levelProgressModel.PassedLevelIndex}, 最近通关={levelProgressModel.LastClearedLevelIndex}");
        }
        
        public void LoadLevelProgress()
        {
            string filePath = GetSaveFilePath();
            
            if (File.Exists(filePath))
            {
                try
                {
                    string jsonData = File.ReadAllText(filePath);
                    LevelProgressData data = JsonUtility.FromJson<LevelProgressData>(jsonData);
                    
                    if (data != null)
                    {
                        levelProgressModel.PassedLevelIndex = data.passedLevelIndex;

                        if (jsonData.Contains("\"lastClearedLevelIndex\""))
                            levelProgressModel.LastClearedLevelIndex = data.lastClearedLevelIndex;
                        else
                            levelProgressModel.LastClearedLevelIndex = data.passedLevelIndex;

                        levelProgressModel.WatchedCutscenes = data.watchedCutscenes ?? new List<int>();

                        Debug.Log($"LevelProgressSystem: 已加载关卡进度，最高通关={levelProgressModel.PassedLevelIndex}, 最近通关={levelProgressModel.LastClearedLevelIndex}");
                    }
                    else
                    {
                        Debug.LogWarning("LevelProgressSystem: 读取的存档数据为空，使用默认值");
                        levelProgressModel.PassedLevelIndex = -1;
                        levelProgressModel.LastClearedLevelIndex = -1;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"LevelProgressSystem: 加载存档失败: {e.Message}");
                    levelProgressModel.PassedLevelIndex = -1;
                    levelProgressModel.LastClearedLevelIndex = -1;
                }
            }
            else
            {
                Debug.Log("LevelProgressSystem: 存档文件不存在，使用默认值");
                levelProgressModel.PassedLevelIndex = -1;
                levelProgressModel.LastClearedLevelIndex = -1;
            }
        }
        
        private void SaveToDisk()
        {
            try
            {
                LevelProgressData data = new LevelProgressData
                {
                    passedLevelIndex = levelProgressModel.PassedLevelIndex,
                    lastClearedLevelIndex = levelProgressModel.LastClearedLevelIndex,
                    watchedCutscenes = levelProgressModel.WatchedCutscenes
                };
                
                string jsonData = JsonUtility.ToJson(data, true);
                string filePath = GetSaveFilePath();
                
                // 确保目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(filePath, jsonData);
                Debug.Log($"LevelProgressSystem: 存档已保存到 {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LevelProgressSystem: 保存存档失败: {e.Message}");
            }
        }
        
        private string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }
    }
}
