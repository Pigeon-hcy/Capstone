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

        [Header("Recovery State")]
        public bool isLoopRecovery = false;
        public bool ignoringMovementLayerRecovery = false;

        [Header("Grind State")]
        public bool isLoopGrind = true;
        public bool ignoringMovementLayerGrind = false;

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
        [Header("Trick B State")]
        public bool isLoopTrickB = false;
        public float durationTrickB = 0.25f;
        public bool ignoringMovementLayerTrickB = true;
        public float recoveryDurationTrickB = 0.5f;
        [Range(0f, 1f)] public float TrickBinertia = 0.67f;
        public float TrickBspeed = 10f;
        [Header("Trick B Boost State")] 
        public bool isLoopTrickBBoost = false;
        public float durationTrickBBoost = 0.5f;
        public bool ignoringMovementLayerTrickBBoost = false;
        [Header("Trick C State")]
        public bool isLoopTrickC = true;
        public bool ignoringMovementLayerTrickC = true;
        public float TrickCspeed = 10f;
        [Range(0f, 1f)] public float TrickCinertia = 0.67f;
        public float recoveryDurationTrickC = 0.5f;
        public float slamHitboxDurationTrickC = 0.25f;
        [Header("Trick C Boost State")]
        public bool isLoopTrickCBoost = true;
        public bool ignoringMovementLayerTrickCBoost = true;
        public float minDurationTrickCBoost = 0.5f;
        public float TrickCBoostspeed = 10f;
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

        [Header("Collision")]
        public LayerMask groundLayer;
        public float groundCheckDistance = 0.35f;
        public float groundCheckOffset = 0.3f;
        public float groundCheckAngle = 60f;
        public float wallCheckDistanceFarFast = 3f;
        public float wallCheckDistanceFarSlow = 1f;
        public float wallCheckDistanceNear = 0.35f;
        public float wallCheckOffset = 0.3f;
        public float crashVelocity = 8f;
        public float crashAngle = 60f;
        public float crashAngleDiff = 45f;
        
        [Header("跳跃设置")]
        public float maxJumpForce = 6f;
        public float minJumpForce = 0f;
        [Range(0f, 1f)] public float wallJumpUpMultiplier = 0.7f;
        public float wallJumpForceMultiplier = 1.5f;
        public float doubleJumpForce = 8f;
        public float maxChargeTime = 2f;

        [Header("移动设置")]
        
        [Tooltip("地面吸力")] public float groundForce = 10f;
        
        [Tooltip("坡度补偿力"), Range(0f, 1f)] public float slopeCompensationForce = 0.5f;
        public float maxMoveSpeed = 5f;
        public float maxFallSpeed = -10f;
        public float maxAirHorizontalSpeed = 10f;
        public float airAccel = 20f;
        public float groundAccel = 20f;
        
        [Tooltip("低速状态减速提升"), Range(0f, -2f)] public float stopDecelIncrement = -1f;
        public float turnDecel = 40f;
        public float airTurnDecel = 40f;
        public float pushAccel = 20f;
        public float groundLinearDamping = 6f;
        public float airLinearDamping = 0.5f;

        [Header("Air相关")]
        public float airControlForceConfig = 10f;
        public float maxAirHorizontalSpeedConfig = 8f;

        [Header("Grind相关")]
        public float normalG = 1f;

        [Header("Power Grind相关")]
        public float powerGrindDeceleration = 1f;
        public float reverseInputWindow = 2.0f;
        public float grindJumpIgnoreTime = 0.2f;


        [Header("瞄准设置")]
        public float baseMaxAimTime = 3f;
        public GameObject[] bulletPrefabs;   // 可切换的子弹类型
        public float bulletSpeed = 15f;
        public int bulletMaxCount = 2;
    }
}
