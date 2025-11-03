using QFramework;
using UnityEngine;

namespace SkateGame
{
    public interface IPlayerAssetSystem : ISystem
    {
        void SetPlayerConfig(PlayerConfig config);
    }

    public class PlayerAssetSystem : AbstractSystem, IPlayerAssetSystem
    {
        private PlayerConfig playerConfig;
        private IPlayerModel playerModel;
        
        protected override void OnInit()
        {
            // 获取玩家模型
            playerModel = this.GetModel<IPlayerModel>();
        }

        public void SetPlayerConfig(PlayerConfig config)
        {   
            playerConfig = config;
            playerModel.Config.Value = playerConfig;
        }
    }
}
