using QFramework;
using System.Collections.Generic;

namespace SkateGame
{
    public interface ILevelProgressModel : IModel
    {
        /// <summary>
        /// 已通关的关卡索引（最高通关关卡）
        /// </summary>
        int PassedLevelIndex { get; set; }

        /// <summary>
        /// 最近一次通关的关卡索引（用于主菜单等“最近进度表现”）
        /// </summary>
        int LastClearedLevelIndex { get; set; }
        List<int> WatchedCutscenes { get; set; }
    }

    public class LevelProgressModel : AbstractModel, ILevelProgressModel
    {
        public int PassedLevelIndex { get; set; } = -1; // -1 表示没有通关任何关卡
        public int LastClearedLevelIndex { get; set; } = -1;
        public List<int> WatchedCutscenes { get; set; } = new List<int>();
        
        protected override void OnInit()
        {
            // 初始化逻辑
        }
    }
}
