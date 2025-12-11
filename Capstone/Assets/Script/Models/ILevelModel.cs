using QFramework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SkateGame
{
    [System.Serializable]
    public class Level
    {
        public string SceneName;  // 场景名
        public string Name;       // 关卡名（显示用）
        public Button button;     // 对应 UI 按钮
    }

    public interface ILevelModel : IModel
    {
        List<Level> LevelList { get; set; }
        int CurrentLevelIndex { get; set; }
        string CurrentLevelName { get; set; }
    }

    public class LevelModel : AbstractModel, ILevelModel
    {
        public List<Level> LevelList { get; set; } = new List<Level>();
        public int CurrentLevelIndex { get; set; } = 0;
        public string CurrentLevelName { get; set; } = "";

        protected override void OnInit() { }
    }
}

