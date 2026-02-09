
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkateGame
{
    public interface ILevelSystem : ISystem
    {
        void LoadLevel(int index);
        Level GetCurrent();
        void AddLevel(Level newLevel);
    }

    public class LevelSystem : AbstractSystem, ILevelSystem
    {
        private ILevelModel levelModel;

        protected override void OnInit()
        {
            levelModel = this.GetModel<ILevelModel>();
        }

        public void LoadLevel(int index)
        {
            Debug.Log($"LevelSystem.LoadLevel: 被调用，索引 = {index}, 当前场景 = {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, LevelList.Count = {levelModel.LevelList.Count}");
            
            if (index < 0 || index >= levelModel.LevelList.Count)
            {
                Debug.LogError($"LevelSystem: 关卡索引 {index} 超出范围！(范围: 0-{levelModel.LevelList.Count - 1})");
                return;
            }

            levelModel.CurrentLevelIndex = index;
            Level current = GetCurrent();
            levelModel.CurrentLevelName = current.Name;
            
            Debug.Log($"LevelSystem.LoadLevel: 设置当前关卡索引 = {index}, 关卡名称 = {current.Name}, 场景名称 = {current.SceneName}");
            
            ChangeScene(current.SceneName);
        }

        public Level GetCurrent()
        {
            return levelModel.LevelList[levelModel.CurrentLevelIndex];
        }

        public void AddLevel(Level newLevel)
        {
            levelModel.LevelList.Add(newLevel);
        }

        private void ChangeScene(string sceneName)
        {
            Debug.Log($"LevelSystem: 加载场景 {sceneName}");
            SceneManager.LoadScene(sceneName);
            this.SendEvent(new SceneChangeEvent());
        }
    }
}
