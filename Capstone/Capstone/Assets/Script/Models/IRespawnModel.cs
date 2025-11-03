using QFramework;
using UnityEngine;
using System.Collections.Generic;

namespace SkateGame
{
    public interface IRespawnModel : IModel
    {
        /// <summary>
        /// 所有经过的检查点列表
        /// </summary>
        BindableProperty<List<Vector2>> CheckpointList { get; }
        
        /// <summary>
        /// 最新的检查点位置
        /// </summary>
        BindableProperty<Vector2> LatestCheckpoint { get; }
        
        /// <summary>
        /// 是否有可用的检查点
        /// </summary>
        BindableProperty<bool> HasCheckpoint { get; }
    }

    public class RespawnModel : AbstractModel, IRespawnModel
    {
        public BindableProperty<List<Vector2>> CheckpointList { get; } = new BindableProperty<List<Vector2>>(new List<Vector2>());
        public BindableProperty<Vector2> LatestCheckpoint { get; } = new BindableProperty<Vector2>(Vector2.zero);
        public BindableProperty<bool> HasCheckpoint { get; } = new BindableProperty<bool>(false);
        
        protected override void OnInit()
        {
            // 初始化逻辑
        }
    }
}

