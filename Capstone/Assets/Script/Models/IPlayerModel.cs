using QFramework;
using UnityEngine;
namespace SkateGame
{
    public interface IPlayerModel : IModel
    {
        // Reference to PlayerConfig
        BindableProperty<PlayerConfig> Config { get; }

        // Player Parameters
        BindableProperty<MovementStates> CurrentMovementState { get; }
        BindableProperty<ActionStates> CurrentActionState { get; }
        BindableProperty<bool> IsGrounded { get; }
        BindableProperty<bool> IsNearTrack { get; }
        BindableProperty<bool> IsNearBgWall { get; }
        BindableProperty<bool> IsNearFgWall { get; }
        BindableProperty<float> FgWallAngle { get; }
        
        /// <summary>
        /// Wall相关
        /// </summary>
        BindableProperty<Wall> CurrentBgWall { get; }

        /// <summary>
        /// air相关
        /// </summary>
        BindableProperty<bool> CanDoubleJump { get; }

        /// <summary>
        /// grind相关
        /// </summary>
        BindableProperty<float> Speed { get; }
        BindableProperty<Vector2> GrindDirection { get; }
        BindableProperty<float> NormalG { get; }

        /// <summary>
        /// jump相关
        /// </summary>
        BindableProperty<float> JumpDuration { get; }
        BindableProperty<float> DoubleJumpDuration { get; }

        /// <summary>
        /// land相关
        /// </summary>
        BindableProperty<float> LandDuration { get; }
        BindableProperty<float> DoubleJumpLandDuration { get; }

        /// <summary>
        /// move相关
        /// </summary>
        BindableProperty<float> PushDuration { get; }

        /// <summary>
        /// Trick相关
        /// </summary>
        BindableProperty<bool> IsInPower { get; }
        BindableProperty<float> VelocityBeforeTrick { get; }
        /// <summary>
        /// Action Layer 基础参数
        /// </summary>
        BindableProperty<bool> IsIgnoringMovementLayer { get; }

        /// <summary>
        /// 子弹相关
        /// </summary>
        BindableProperty<int> CurrentBulletIndex { get; }
        BindableProperty<int> CurrentBulletCount  { get; }
        BindableProperty<bool> IsAiming { get; }
        BindableProperty<float> MaxAimTime { get; }
        BindableProperty<float> AimTimer { get; }

        /// <summary>
        /// State Machine相关
        /// </summary>
        BindableProperty<string> LastActionStateName { get; }

        // 非输入的运行时/调参（来自 InputController，但不包含 isEHeld/isWHeld 等原始输入）
        BindableProperty<Track> CurrentTrack { get; }
        BindableProperty<float> GrindJumpTimer { get; } // 用来防止跳跃后被吸附到原先滑轨
        BindableProperty<float> WallRideCooldownTimer { get; } // 用来控制两次滑墙之间的冷却时间
        BindableProperty<bool> WasGrounded { get; }
        BindableProperty<bool> IsCheckingReverseWindow { get; }
        BindableProperty<float> ReverseTimer { get; }
        BindableProperty<bool> HasPerformedTrickInAir { get; }
        BindableProperty<bool> IsFacingRight { get; }
        BindableProperty<float> CurrentRotationDeg { get; }
        BindableProperty<float> TargetRotationDeg { get; }
    }

    public class PlayerModel : AbstractModel, IPlayerModel
    {
        public BindableProperty<PlayerConfig> Config { get; } = new BindableProperty<PlayerConfig>(null);
        public BindableProperty<MovementStates> CurrentMovementState { get; } = new BindableProperty<MovementStates>(MovementStates.IdleState);
        public BindableProperty<ActionStates> CurrentActionState { get; } = new BindableProperty<ActionStates>(ActionStates.NoActionState);
        public BindableProperty<bool> IsGrounded { get; } = new BindableProperty<bool>(true);
        public BindableProperty<bool> IsNearTrack { get; } = new BindableProperty<bool>(false);
        public BindableProperty<bool> IsNearBgWall { get; } = new BindableProperty<bool>(false);
        public BindableProperty<bool> IsNearFgWall { get; } = new BindableProperty<bool>(false);
        public BindableProperty<float> FgWallAngle { get; } = new BindableProperty<float>(0f);
        // Wall相关
        public BindableProperty<Wall> CurrentBgWall { get; } = new BindableProperty<Wall>(null);
        
        // air相关
        public BindableProperty<bool> CanDoubleJump { get; } = new BindableProperty<bool>(true);

        // grind相关
        public BindableProperty<float> Speed { get; } = new BindableProperty<float>(0f);
        public BindableProperty<Vector2> GrindDirection { get; } = new BindableProperty<Vector2>(Vector2.zero);
        public BindableProperty<float> NormalG { get; } = new BindableProperty<float>(1f);
        
        // jump相关
        public BindableProperty<float> JumpDuration { get; } = new BindableProperty<float>(0f);
        public BindableProperty<float> DoubleJumpDuration { get; } = new BindableProperty<float>(0f);
        
        // land相关
        public BindableProperty<float> LandDuration { get; } = new BindableProperty<float>(0f);
        public BindableProperty<float> DoubleJumpLandDuration { get; } = new BindableProperty<float>(0f);

        // move相关
        public BindableProperty<float> PushDuration { get; } = new BindableProperty<float>(0f);

        // Trick相关
        public BindableProperty<bool> IsInPower { get; } = new BindableProperty<bool>(false);
        public BindableProperty<float> VelocityBeforeTrick { get; } = new BindableProperty<float>(0f);

        // Action Layer 基础参数
        public BindableProperty<bool> IsIgnoringMovementLayer { get; } = new BindableProperty<bool>(false);
        
        // 子弹相关
        public BindableProperty<int> CurrentBulletIndex { get; } = new BindableProperty<int>(0);
        public BindableProperty<int> CurrentBulletCount { get; } = new BindableProperty<int>(0);
        public BindableProperty<bool> IsAiming { get; } = new BindableProperty<bool>(false);
        public BindableProperty<float> MaxAimTime { get; } = new BindableProperty<float>(3f);
        public BindableProperty<float> AimTimer { get; } = new BindableProperty<float>(0f);

        // State Machine相关
        public BindableProperty<string> LastActionStateName { get; } = new BindableProperty<string>("None");

        // 非输入的运行时/调参（来自 InputController，但不包含 isEHeld/isWHeld 等原始输入）
        public BindableProperty<Track> CurrentTrack { get; } = new BindableProperty<Track>(null);
        public BindableProperty<float> GrindJumpTimer { get; } = new BindableProperty<float>(0f);
        public BindableProperty<float> WallRideCooldownTimer { get; } = new BindableProperty<float>(0f);
        public BindableProperty<bool> WasGrounded { get; } = new BindableProperty<bool>(true);
        public BindableProperty<bool> IsCheckingReverseWindow { get; } = new BindableProperty<bool>(false);
        public BindableProperty<float> ReverseTimer { get; } = new BindableProperty<float>(0f);
        public BindableProperty<bool> HasPerformedTrickInAir { get; } = new BindableProperty<bool>(false);
        public BindableProperty<bool> IsFacingRight { get; } = new BindableProperty<bool>(true);
        public BindableProperty<float> CurrentRotationDeg { get; } = new BindableProperty<float>(0f);
        public BindableProperty<float> TargetRotationDeg { get; } = new BindableProperty<float>(0f);
        protected override void OnInit()
        {
            // 初始化逻辑
        }
    }
}
