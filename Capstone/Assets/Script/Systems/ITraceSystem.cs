using QFramework;
using UnityEngine;

namespace SkateGame
{
    /// <summary>
    /// 死亡路径点系统 - 记录轨迹并在死亡时触发绘制
    /// </summary>
    public interface ITraceSystem : ISystem
    {
        /// <summary>
        /// 添加一个路径点
        /// </summary>
        void AddPoint(Vector2 pos);

        /// <summary>
        /// 玩家死亡时调用 - 绘制上次路径并清空记录
        /// </summary>
        /// <param name="deathPosition">死亡瞬间位置（用死亡点专用 prefab 画）</param>
        void OnPlayerDeath(Vector2 deathPosition);
    }

    public class TraceSystem : AbstractSystem, ITraceSystem
    {
        private ITraceModel traceModel;

        protected override void OnInit()
        {
            traceModel = this.GetModel<ITraceModel>();
        }

        public void AddPoint(Vector2 pos)
        {
            traceModel.DrawnPoints.Add(pos);
        }

        public void OnPlayerDeath(Vector2 deathPosition)
        {
            var playerController = Object.FindFirstObjectByType<PlayerController>();
            if (playerController == null) return;

            var points = new System.Collections.Generic.List<Vector2>(traceModel.DrawnPoints);
            playerController.DrawDeathTracePoints(points, deathPosition);
            traceModel.DrawnPoints.Clear();
        }
    }
}
