using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkateGame
{
    /// <summary>
    /// 关卡系统接口
    /// </summary>
    public interface ILevelSystem : ISystem
    {
        void LoadLevel(int index);
        Level GetCurrent();
        void AddLevel(Level newLevel);
    }

    /// <summary>
    /// 关卡系统实现
    /// </summary>
    public class LevelSystem : AbstractSystem, ILevelSystem
    {
        private ILevelModel levelModel;
        
        protected override void OnInit()
        {
            levelModel = this.GetModel<ILevelModel>();
        }

        /// <summary>
        /// 加载指定索引的关卡
        /// </summary>
        public void LoadLevel(int index)
        {
            if (index < 0 || index >= levelModel.LevelList.Count)
            {
                Debug.LogError($"LevelSystem: 关卡索引 {index} 超出范围！");
                return;
            }

            levelModel.CurrentLevelIndex = index;
            Level currentLevel = GetCurrent();
            levelModel.CurrentLevelName = currentLevel.Name;
            
            ChangeScene(currentLevel.SceneName);
        }
        
        /// <summary>
        /// 获取当前关卡
        /// </summary>
        public Level GetCurrent()
        {
            return levelModel.LevelList[levelModel.CurrentLevelIndex];
        }
        
        /// <summary>
        /// 添加新关卡
        /// </summary>
        public void AddLevel(Level newLevel)
        {
            levelModel.LevelList.Add(newLevel);
            Debug.Log($"LevelSystem: 添加关卡 {newLevel.Name}");
        }
        
        /// <summary>
        /// 切换场景
        /// </summary>
        private void ChangeScene(string sceneName)
        {
            Debug.Log($"LevelSystem: 尝试加载场景 '{sceneName}'");
            
            // 检查场景是否在 Build Settings 中
            int sceneIndex = -1;
            bool sceneFound = false;
            
            // 遍历 Build Settings 中的所有场景来查找目标场景
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                if (sceneNameFromPath == sceneName)
                {
                    sceneIndex = i;
                    sceneFound = true;
                    Debug.Log($"LevelSystem: 在 Build Settings 中找到场景 '{sceneName}' (索引: {sceneIndex})");
                    break;
                }
            }
            
            if (!sceneFound)
            {
                Debug.LogError($"LevelSystem: 场景 '{sceneName}' 未添加到 Build Settings！");
                Debug.LogError("请在 File → Build Settings 中添加该场景");
                
                // 列出所有可用的场景
                Debug.Log("当前 Build Settings 中的场景列表：");
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    Debug.Log($"  [{i}] {sceneNameFromPath} (路径: {scenePath})");
                }
                return;
            }
            
            Debug.Log($"LevelSystem: 开始加载场景 {sceneName} (索引: {sceneIndex})");
            SceneManager.LoadScene(sceneName);
        }
    }
}

