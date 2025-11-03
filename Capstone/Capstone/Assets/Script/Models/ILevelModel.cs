using QFramework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SkateGame
{
    /// <summary>
    /// 关卡信息类
    /// </summary>
    [System.Serializable]
    public class Level
    {
        public string SceneName;    // 场景名称
        public string Name;         // 关卡名称
        public int Index;           // 关卡索引
        public Button button;       // 关卡按钮
    }

    /// <summary>
    /// 关卡模型接口
    /// </summary>
    public interface ILevelModel : IModel
    {
        List<Level> LevelList { get; set; }
        int CurrentLevelIndex { get; set; }
        string CurrentLevelName { get; set; }
    }

    /// <summary>
    /// 关卡模型实现
    /// </summary>
    public class LevelModel : AbstractModel, ILevelModel
    {
        public List<Level> LevelList { get; set; } = new List<Level>();
        public int CurrentLevelIndex { get; set; } = 0;
        public string CurrentLevelName { get; set; } = "";

        protected override void OnInit()
        {
            // 初始化空列表，关卡将通过 LevelManager 配置
        }
    }
}

