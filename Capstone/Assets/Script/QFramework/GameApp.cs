using QFramework;

namespace SkateGame
{
    public class GameApp : Architecture<GameApp>
    {
        protected override void Init()
        {
            // 注册数据模型
            this.RegisterModel<IPlayerModel>(new PlayerModel());
            this.RegisterModel<IInputModel>(new InputModel());
            this.RegisterModel<IGameModel>(new GameModel());
            this.RegisterModel<ITrickListModel>(new TrickListModel());
            this.RegisterModel<ILevelModel>(new LevelModel());
            this.RegisterModel<ILevelProgressModel>(new LevelProgressModel());
            this.RegisterModel<IEnemyModel>(new EnemyModel());
            this.RegisterModel<IRespawnModel>(new RespawnModel());
            this.RegisterModel<ITraceModel>(new TraceModel());
            this.RegisterModel<IDialogueModel>(new DialogueModel());
            this.RegisterModel<ITimerModel>(new TimerModel());

            // 注册业务系统（PlayerAssetSystem 最先初始化，确保配置优先加载）
            this.RegisterSystem<IPlayerAssetSystem>(new PlayerAssetSystem());
            this.RegisterSystem<ICollisionSystem>(new CollisionSystem());
            this.RegisterSystem<IEnemyAssetSystem>(new EnemyAssetSystem());
            this.RegisterSystem<IPlayerSystem>(new PlayerSystem());
            this.RegisterSystem<ITrickSystem>(new TrickSystem());
            this.RegisterSystem<ILevelSystem>(new LevelSystem());
            this.RegisterSystem<ILevelProgressSystem>(new LevelProgressSystem());
            this.RegisterSystem<IRespawnSystem>(new RespawnSystem());
            this.RegisterSystem<ITraceSystem>(new TraceSystem());
            this.RegisterSystem<IDialogueSystem>(new DialogueSystem());
            this.RegisterSystem<IInputGateSystem>(new InputGateSystem());
            this.RegisterSystem<IEnergySystem>(new EnergySystem());
            this.RegisterSystem<ITimerSystem>(new TimerSystem());
        }
    }
}
