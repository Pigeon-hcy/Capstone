using UnityEngine;

namespace SkateGame
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "SkateGame/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("========== Action Layer基础参数 ==========")]
        [Header("No Action State")]
        public bool isLoopNoAction = true;
        public bool ignoringMovementLayerNoAction = false;
        [Header("Push State")]
        public bool isLoopPush = true;
        public bool ignoringMovementLayerPush = false;
        public float maxPushSpeed = 10f;
        public float pushTimeToMaxSpeed = 2f;
        public float pushReverseCooldown = 0.5f;
        [Tooltip("If push hasn't fired when entering push state, check push quicker")]
        public float firstPushKickInterval = 0.4f;
        public float pushKickInterval = 0.8f;
        public float pushKickSpeedIncrement = 2f;
        public float pushMountSpeed = 5f;
        public float applyPushKickThreshold = 20f;
        [Tooltip("Wait for animation time before applying push speed increment")]
        public float pushKickDelay = 0.1f;

        [Header("Power Grind State")]
        public bool isLoopPowerGrind = false;
        public bool ignoringMovementLayerPowerGrind = true;
        public float powerGrindStopSpeedThreshold = 0.5f;
        public float powerGrindDistanceMultiplier = 2f;
        public float powerGrindDecelerationRate = 20f; // units/s
        public float powerGrindWalkDelay = 0.3f; // seconds before walk allowed after full stop

        [Header("Hit State")]
        public bool isLoopHit = false;
        public bool ignoringMovementLayerHit = true;
        public float hitDuration = 0.5f;
        public float hitKnockbackForce = 5f;
        public float hitSpeedDecay = 0.8f;

        [Header("Recovery State")]
        public bool isLoopRecovery = false;
        public bool ignoringMovementLayerRecovery = false;

        [Header("Grind State")]
        public bool isLoopGrind = true;
        public bool ignoringMovementLayerGrind = false;
        public float grindSpeed = 28f;
        public float grindResetSpeed = 10f;
        public float grindJumpIgnoreTime = 0.2f;
        public float autoGJumpIgnoreTime = 0.15f;

        [Header("Bg Wall Ride State")]
        public bool isLoopBgWallRide = true;
        public bool ignoringMovementLayerBgWallRide = false;
        public float BgWallRideCooldown = 1.5f;
        public float BgWallrideDuration = 0.6f;

        [Header("Trick A State")]
        public bool isLoopTrickA = false;
        public float durationTrickA = 0.2f;
        public bool ignoringMovementLayerTrickA = true;
        public float recoveryDurationTrickA = 0.5f;
        public float TrickARewardForceVertical = 14f;
        public float TrickARewardForceHorizontal = 14f;
        public float TrickACooldown = 0.3f;
        [Header("Trick B State")]
        public bool isLoopTrickB = false;
        public float durationTrickB = 0.25f;
        public bool ignoringMovementLayerTrickB = true;
        public float recoveryDurationTrickB = 0.5f;
        [Range(0f, 1f)] public float TrickBinertia = 0.67f;
        public float TrickBspeed = 10f;
        public float TrickBMinExitSpeed = 5f;
        public float TrickBMaxFallSpeed = 10f;
        [Header("Trick B Boost State")] 
        public bool isLoopTrickBBoost = false;
        public float durationTrickBBoost = 0.5f;
        public bool ignoringMovementLayerTrickBBoost = false;
        [Header("Trick C State")]
        public bool isLoopTrickC = true;
        public bool ignoringMovementLayerTrickC = true;
        public float TrickCSpeed = 10f;
        [Range(0f, 1f)] public float TrickCInertia = 0.67f;
        public float recoveryDurationTrickC = 0.5f;
        public float slamHitboxDurationTrickC = 0.25f;
        public float TrickCLandBoostSpeed = 10f;
        [Header("Trick D State")]
        public bool isLoopTrickD = false;
        public float durationTrickD = 0.5f;
        public bool ignoringMovementLayerTrickD = true;
        public float recoveryDurationTrickD = 1f;
        public float extendSpeed = 50f;
        public float retractSpeed = 35f;
        public float maxDistance = 20f;
        public float grappleForce = 20f;
        public float grappleDuration = 0.5f;
        public float grappleImpulse = 10f;

        [Header("========== 基础参数 ==========")]
        
        [Header("Rotation")]
        public float groundRotationSpeed = 50f;
        public float airRotationSpeed = 10f;
        public float maxRotationSnapDeg = 90f;

        [Header("Collision")]
        public LayerMask groundLayer;
        public float groundCheckDistance = 0.35f;
        public float groundCheckOffset = 0.3f;
        public float groundCheckAngle = 60f;
        public float wallCheckDistanceFarFast = 3f;
        public float wallCheckDistanceFarSlow = 1f;
        public float wallCheckDistanceNear = 0.35f;
        public float wallCheckOffset = 0.3f;
        public float landingSpeedThreshold = 2f;
        public float crashVelocity = 8f;
        public float crashAngle = 60f;
        public float crashAngleDiff = 45f;
        
        [Header("跳跃")]
        public float jumpForce = 2f;
        public float jumpHoldForce = 6f;
        public float jumpHoldMaxTime = 0.2f;
        public float coyoteTime = 0.1f;
        public float jumpBufferTime = 0.1f;
        [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;

        [Header("Wall Jump")]
        public float wallSlideEntrySpeed = 5f;
        public float wallJumpImpulse = 6f;
        public float wallJumpSpeed = 10f;
        [Tooltip("越小越接近墙面normal，越大越接近垂直向上")]
        [Range(0f, 1f)] public float wallJumpUpMultiplier = 0.7f;
        public float wallSlideLandSpeed = 5f;
        

        [Header("移动设置")]
        
        [Tooltip("地面吸力")] public float groundForce = 10f;
        
        [Tooltip("坡度补偿力"), Range(0f, 1f)] public float slopeCompensationForce = 0.5f;
        public float maxMoveSpeed = 5f;
        public float maxFallSpeed = -10f;
        public float maxSlopeSlideSpeed = 6f;
        public float groundLinearDamping = 6f;
        public float airLinearDamping = 0.5f;
        public float vPhysicsDecay = 0.98f;
        public float vPushDecay = 0.998f;

        [Header("Air相关")]
        public float airControlForceConfig = 10f;
        public float maxAirHorizontalSpeedConfig = 8f;

        [Header("重力相关")]
        public float normalG = 1f;
        public float gravityMagnitude = 9.81f;
        public float maxUpwardSpeed = 24;


        [Header("瞄准设置")]
        public float baseMaxAimTime = 3f;
        public GameObject[] bulletPrefabs;   // 可切换的子弹类型
        public float bulletSpeed = 15f;
        public int bulletMaxCount = 2;
    }
}
