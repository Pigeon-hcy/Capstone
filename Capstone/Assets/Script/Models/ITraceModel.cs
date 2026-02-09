using QFramework;
using UnityEngine;
using System.Collections.Generic;

namespace SkateGame
{
    /// <summary>
    /// 死亡路径点数据模型 - 存储玩家上次生命的轨迹点
    /// </summary>
    public interface ITraceModel : IModel
    {
        /// <summary>
        /// 本局记录下的路径点列表
        /// </summary>
        List<Vector2> DrawnPoints { get; }
    }

    public class TraceModel : AbstractModel, ITraceModel
    {
        public List<Vector2> DrawnPoints { get; } = new List<Vector2>();

        protected override void OnInit() { }
    }
}
