using QFramework;
using UnityEngine;
using System.IO;

namespace SkateGame
{
    public interface ILevelProgressSystem : ISystem
    {
        /// <summary>
        /// 保存当前已通过的关卡到磁盘
        /// </summary>
        void SaveLevel();
        
        /// <summary>
        /// 从磁盘读取已解锁的关卡
        /// </summary>
        void LoadLevelProgress();
    }

    [System.Serializable]
    public class LevelProgressData
    {
        public int passedLevelIndex = -1;
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
        
        public void SaveLevel()
        {
            // 获取当前关卡索引
            int currentLevelIndex = levelModel.CurrentLevelIndex;
            
            // 如果当前关卡索引大于已通关的关卡索引，则更新
            if (currentLevelIndex > levelProgressModel.PassedLevelIndex)
            {
                levelProgressModel.PassedLevelIndex = currentLevelIndex;
            }
            
            // 保存到磁盘
            SaveToDisk();
            
            Debug.Log($"LevelProgressSystem: 已保存关卡进度，当前最高通关关卡索引 = {levelProgressModel.PassedLevelIndex}");
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
                        Debug.Log($"LevelProgressSystem: 已加载关卡进度，最高通关关卡索引 = {levelProgressModel.PassedLevelIndex}");
                    }
                    else
                    {
                        Debug.LogWarning("LevelProgressSystem: 读取的存档数据为空，使用默认值");
                        levelProgressModel.PassedLevelIndex = -1;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"LevelProgressSystem: 加载存档失败: {e.Message}");
                    levelProgressModel.PassedLevelIndex = -1;
                }
            }
            else
            {
                Debug.Log("LevelProgressSystem: 存档文件不存在，使用默认值");
                levelProgressModel.PassedLevelIndex = -1;
            }
        }
        
        private void SaveToDisk()
        {
            try
            {
                LevelProgressData data = new LevelProgressData
                {
                    passedLevelIndex = levelProgressModel.PassedLevelIndex
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
