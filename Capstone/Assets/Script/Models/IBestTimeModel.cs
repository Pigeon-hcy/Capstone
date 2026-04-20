using QFramework;
using System.Collections.Generic;

namespace SkateGame
{
    public interface IBestTimeModel : IModel
    {
        /// <summary>
        /// 每个关卡索引对应的最佳通关时间（秒）
        /// </summary>
        Dictionary<int, float> BestTimes { get; }
    }

    public class BestTimeModel : AbstractModel, IBestTimeModel
    {
        public Dictionary<int, float> BestTimes { get; } = new Dictionary<int, float>();

        protected override void OnInit() { }
    }
}
