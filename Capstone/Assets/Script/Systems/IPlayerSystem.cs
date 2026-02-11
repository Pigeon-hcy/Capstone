using QFramework;
using UnityEngine;

namespace SkateGame
{
    public struct PendingActions
    {
        public bool JumpQueued;
        public bool WallJumpQueued;
        public bool ReverseQueued;
        public bool GrappleImpulseQueued;
        public bool TrickBResetSpeedQueued;
        public bool TrickCLandQueued;
        public bool TrickARewardQueued;

        public bool Dashing;
        public bool Slamming;
        public bool Grapplling;
        public bool Grinding;
        public bool PowerGrinding;
        public bool Pushing;
        public float TrickBDirection;
        public Vector2 GrappleDirection;

        public void Clear()
        {
            JumpQueued = false;
            WallJumpQueued = false;
            ReverseQueued = false;
            GrappleImpulseQueued = false;
            TrickBResetSpeedQueued = false;
            TrickCLandQueued = false;
            TrickARewardQueued = false;
        }
        public void ClearAll()
        {
            JumpQueued = false;
            WallJumpQueued = false;
            ReverseQueued = false;
            GrappleImpulseQueued = false;
            TrickBResetSpeedQueued = false;
            TrickCLandQueued = false;
            TrickARewardQueued = false;
            Dashing = false;
            Slamming = false;
            Grapplling = false;
            Grinding = false;
            PowerGrinding = false;
            Pushing = false;
        }
    }

    public interface IPlayerSystem : ISystem
    {
        void ApplyMovement();
        void ApplyRotation();
    }

    public class PlayerSystem : AbstractSystem, IPlayerSystem, ICanSendCommand
    {
        private PlayerController playerController;
        private IPlayerModel playerModel;
        private Rigidbody2D rb;
        private Vector2 moveVel; // 基础速度，会被限速
        private Vector2 bonusVel; // 额外速度，不限速，只随时间衰减
        private float cachedMoveInput;
        private PendingActions pending;
        private float lastTrickBDirection;
        protected override void OnInit()
        {
            // 获取玩家控制器
            UpdatePlayerController();
            
            // 获取玩家参数
            playerModel = this.GetModel<IPlayerModel>();
            
            // 监听输入事件
            this.RegisterEvent<MoveInputEvent>(OnMoveInput);
            this.RegisterEvent<JumpExecuteEvent>(OnJumpInput);
            this.RegisterEvent<WallJumpExecuteEvent>(OnWallJumpInput);
            this.RegisterEvent<PushInputEvent>(OnPushInput);
            this.RegisterEvent<GrindInputEvent>(OnGrindInput);
            this.RegisterEvent<PowerGrindInputEvent>(OnPowerGrindInput);
            this.RegisterEvent<ReverseInputEvent>(OnReverseInput);
            this.RegisterEvent<TrickAInputEvent>(OnTrickAInput);
            this.RegisterEvent<TrickARewardEvent>(OnTrickAReward);
            this.RegisterEvent<TrickBInputEvent>(OnTrickBInput);
            this.RegisterEvent<TrickBResetSpeedEvent>(OnTrickBResetSpeed);
            this.RegisterEvent<TrickCInputEvent>(OnTrickCInput);
            this.RegisterEvent<TrickCLandEvent>(OnTrickCLand);
            this.RegisterEvent<GrappleEvent>(OnGrapple);
            this.RegisterEvent<StateChangedEvent>(OnStateChanged);
            // 每次场景更新自动获取PlayerController
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // 场景加载后重新查找 PlayerController
            UpdatePlayerController();
            moveVel = Vector2.zero;
            bonusVel = Vector2.zero;
            pending.ClearAll();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
        
        private void UpdatePlayerController()
        {
            playerController = Object.FindFirstObjectByType<PlayerController>();
            rb = playerController?.GetComponent<Rigidbody2D>();
            if (playerController != null)
            {
                Debug.Log("PlayerSystem: 找到 PlayerController");
            }
            else
            {
                Debug.LogWarning("PlayerSystem: 场景中没有找到 PlayerController");
            }
        }

        #region Input Cache & Pending Actions
        private void OnMoveInput(MoveInputEvent evt)
        {
            cachedMoveInput = 0;
            cachedMoveInput = evt.HorizontalInput;
        }
        private void OnJumpInput(JumpExecuteEvent evt) { pending.JumpQueued = true; }
        private void OnWallJumpInput(WallJumpExecuteEvent evt) { pending.WallJumpQueued = true; }
        private void OnStateChanged(StateChangedEvent evt)
        {
            ApplyStateChanged(evt);
            UpdateAnimatorOnStateChanged(evt);
        }
        private void OnPushInput(PushInputEvent evt) { pending.Pushing = evt.IsPushing; }
        private void OnPowerGrindInput(PowerGrindInputEvent evt) { pending.PowerGrinding = evt.IsPowerGrinding; }
        private void OnReverseInput(ReverseInputEvent evt) { pending.ReverseQueued = true; }
        private void OnGrindInput(GrindInputEvent evt) { pending.Grinding = evt.IsGrinding; }
        private void OnTrickAInput(TrickAInputEvent evt) { }
        private void OnTrickBInput(TrickBInputEvent evt)
        {
            pending.Dashing = evt.IsTrickingB;
            if (pending.Dashing)
            {
                pending.TrickBDirection = evt.Direction;
                lastTrickBDirection = evt.Direction;
            }
        }
        private void OnTrickCInput(TrickCInputEvent evt) { pending.Slamming = evt.IsTrickingC; }
        private void OnTrickBResetSpeed(TrickBResetSpeedEvent evt) { pending.TrickBResetSpeedQueued = true; }
        private void OnTrickCLand(TrickCLandEvent evt) { pending.TrickCLandQueued = true; }
        private void OnTrickAReward(TrickARewardEvent evt) { pending.TrickARewardQueued = true; }
        private void OnGrapple(GrappleEvent evt)
        {
            pending.GrappleImpulseQueued = evt.IsGrappling;
            pending.Grapplling = evt.IsGrappling;
            pending.GrappleDirection = evt.pullDirection;
        }
        #endregion

        #region Basic Movement
        public void ApplyMovement()
        {
            moveVel = rb.linearVelocity - bonusVel;
            bool isGrounded = playerModel.IsGrounded.Value;

            // Base movement
            ApplyCustomGravity();
            ApplyHorizontalSpeed(cachedMoveInput, isGrounded, pending.Pushing);
            if (isGrounded)
            {
                if (vUpMove < 0f) { moveVel -= vUpMove * groundUp; }
                if (vUpBonus < 0f) { bonusVel -= vUpBonus * groundUp; }
                ApplySlopeCompensation();
                ClampGroundSpeed();
            }
            if (pending.PowerGrinding) ApplyPowerGrind();
            ApplyCustomDamping(isGrounded);

            // Actions
            // Priority 1: Reverse
            if (pending.ReverseQueued) ApplyReverse();
            // Priority 2: leave ground
            if (pending.JumpQueued) ApplyJumpImpulse();
            if (pending.WallJumpQueued) ApplyWallJumpImpulse();
            // Priority 3: trick one-shots
            if (pending.TrickARewardQueued) ApplyTrickAReward();
            if (pending.TrickBResetSpeedQueued) ApplyTrickBResetSpeed(pending.TrickBDirection != 0f ? pending.TrickBDirection : lastTrickBDirection);
            if (pending.TrickCLandQueued) ApplyTrickCLand();
            if (pending.GrappleImpulseQueued) ApplyGrappleImpulse(pending.GrappleDirection);
            // Priority 4: sustained actions
            if (pending.Grinding) ApplyGrind();
            if (pending.Dashing) ApplyTrickB(pending.TrickBDirection);
            if (pending.Slamming) ApplyTrickC();
            if (pending.Grapplling) ApplyGrappleForce(pending.GrappleDirection);

            pending.Clear();

            // Bonus velocity
            rb.linearVelocity = moveVel + bonusVel;
            bonusVel *= playerModel.Config.Value.bonusVelDecay;
            if (Mathf.Abs(bonusVel.x) < 0.01f && Mathf.Abs(bonusVel.y) < 0.01f)
                bonusVel = Vector2.zero;
        }

        private void ApplyHorizontalSpeed(float horizontalInput, bool isGrounded, bool pushing)
        {
            if (pushing)
            {
                moveVel += groundRight * (playerModel.IsFacingRight.Value ? 1 : -1) * playerModel.Config.Value.pushAccel * Time.fixedDeltaTime;
            }
            else if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                // 如果当前速度和输入方向相反，且速度大于0.1f，则减速
                if (Mathf.Sign(vRightMove) != Mathf.Sign(horizontalInput) && Mathf.Abs(vRightMove) > 0.1f)
                {
                    float turnDecel = isGrounded ? playerModel.Config.Value.turnDecel : playerModel.Config.Value.airTurnDecel;
                    turnDecel *= Mathf.Pow(Mathf.Abs(vRightMove), playerModel.Config.Value.stopDecelIncrement);
                    moveVel -= groundRight * Mathf.Sign(vRightMove) * turnDecel * Time.fixedDeltaTime;
                    // 减速最多到0
                    if (Mathf.Sign(vRightMove) == Mathf.Sign(horizontalInput))
                        moveVel = vUpMove * groundUp;
                }
                else if (Mathf.Abs(vRightMove) < playerModel.Config.Value.maxMoveSpeed)
                {
                    float accel = isGrounded ? playerModel.Config.Value.groundAccel : playerModel.Config.Value.airAccel;
                    moveVel += groundRight * horizontalInput * accel * Time.fixedDeltaTime;
                }
            }
        }

        private void ApplyCustomGravity()
        {
            Vector2 g = Vector2.down * playerModel.Config.Value.gravityMagnitude * playerModel.CurrentGravityScale.Value;
            if (playerModel.IsGrounded.Value)
            {
                float intoSlope = Vector2.Dot(g, groundUp);
                Vector2 gravityTangent = g - intoSlope * groundUp;
                moveVel += gravityTangent * Time.fixedDeltaTime;
            }
            else
            {
                moveVel += g * Time.fixedDeltaTime;
            }
        }

        private void ApplyCustomDamping(bool isGrounded)
        {
            float damping = isGrounded ? playerModel.Config.Value.groundLinearDamping : playerModel.Config.Value.airLinearDamping;
            moveVel *= Mathf.Exp(-damping * Time.fixedDeltaTime);
        }

        private void ApplyPowerGrind()
        {
            float deceleration = playerModel.Config.Value.powerGrindDeceleration;
            float direction = Mathf.Sign(vRightMove);
            // 逐渐减少的速度，保持方向不变
            float newVx = vRightMove - direction * deceleration * Time.fixedDeltaTime;
            float newVxBonus = vRightBonus - direction * deceleration * Time.fixedDeltaTime;
            // 防止越过零点
            if (Mathf.Sign(newVx) != direction || Mathf.Abs(newVx) < 0.01f) newVx = 0f;
            if (newVxBonus < 0f) newVxBonus = 0f;
            bonusVel = newVxBonus * groundRight + vUpBonus * groundUp;
            moveVel = newVx * groundRight + vUpMove * groundUp;
        }

        private void ClampGroundSpeed()
        {
            float newVRight = Mathf.Clamp(vRightMove, -playerModel.Config.Value.maxMoveSpeed, playerModel.Config.Value.maxMoveSpeed);
            moveVel = newVRight * groundRight + vUpMove * groundUp;
        }
        #endregion

        #region Ground & Slope
        private void ApplySlopeCompensation()
        {
            Vector2 g = Vector2.down * playerModel.Config.Value.gravityMagnitude * playerModel.CurrentGravityScale.Value;
            Vector2 gTangent = Vector2.Dot(g, groundRight) * groundRight * Mathf.Sign(vRightMove);
            moveVel += -playerModel.Config.Value.slopeCompensationForce * gTangent * Time.fixedDeltaTime;
        }

        private void ApplyGroundForce()
        {
            Vector2 down = (Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.down).normalized;
            moveVel += down * (playerModel.Config.Value.groundForce * Time.fixedDeltaTime);
        }
        #endregion

        #region Jumps
        private void ApplyJumpImpulse()
        {
            Vector2 upDir = playerModel.IsGrounded.Value ? groundUp: Vector2.up;
            moveVel -= Vector2.Dot(moveVel, upDir) * upDir;
            bonusVel -= Vector2.Dot(bonusVel, upDir) * upDir;
            moveVel += upDir * playerModel.Config.Value.maxJumpForce;
        }

        private void ApplyWallJumpImpulse()
        {
            if (!playerModel.IsNearFgWall.Value) return;
            Vector2 normal = Quaternion.Euler(0f, 0f, playerModel.FgWallAngle.Value).normalized * Vector2.up;
            Vector2 jumpDir =  Vector2.Lerp(normal, Vector2.up, playerModel.Config.Value.wallJumpUpMultiplier).normalized;
            moveVel = new Vector2(0, 0);
            bonusVel = Vector2.zero;
            moveVel += jumpDir * playerModel.Config.Value.maxJumpForce *
                playerModel.Config.Value.wallJumpForceMultiplier;
        }
        #endregion

        #region Tricks
        private void ApplyReverse()
        {
            moveVel = new Vector2(-moveVel.x, moveVel.y);
            bonusVel = new Vector2(-bonusVel.x, bonusVel.y);
        }

        private void ApplyGrind()
        {
        }

        private void ApplyTrickB(float direction)
        {
            float speed = Mathf.Max(playerModel.Config.Value.TrickBspeed, playerModel.VelocityBeforeTrick.Value * direction);
            moveVel = new Vector2(direction * speed, 0);
        }
        private void ApplyTrickBResetSpeed(float direction)
        {
            float speed = Mathf.Max(playerModel.Config.Value.maxMoveSpeed, playerModel.VelocityBeforeTrick.Value * direction);
            moveVel = new Vector2(speed * direction, 0);
        }
        private void ApplyTrickC()
        {
            moveVel = new Vector2(moveVel.x, -playerModel.Config.Value.TrickCspeed);
        }
        private void ApplyTrickCLand()
        {
            float slamIntoSlope = Vector2.Dot((Vector2.down * playerModel.Config.Value.TrickCBoostspeed), groundRight);
            bonusVel += slamIntoSlope * groundRight;
        }

        private void ApplyTrickAReward()
        {
            moveVel = new Vector2(moveVel.x, 0);
            moveVel += Vector2.up * playerModel.Config.Value.maxJumpForce;
        }
        private void ApplyGrappleImpulse(Vector2 dir)
        {
            moveVel += dir * playerModel.Config.Value.grappleImpulse;
        }
        private void ApplyGrappleForce(Vector2 dir)
        {
            moveVel += dir * playerModel.Config.Value.grappleForce * Time.fixedDeltaTime;
        }
        #endregion

        #region Animation
        public void ApplyRotation()
        {
            if (Mathf.Abs(rb.rotation - playerModel.TargetRotationDeg.Value) > 0.01f)
            {
                bool isGrounded = playerModel.IsGrounded.Value;
                float speed = isGrounded ? playerModel.Config.Value.groundRotationSpeed : playerModel.Config.Value.airRotationSpeed;
                rb.rotation = Mathf.Lerp(rb.rotation, playerModel.TargetRotationDeg.Value, Time.fixedDeltaTime * speed);
            }
            else
            {
                rb.rotation = playerModel.TargetRotationDeg.Value;
            }
            playerModel.CurrentRotationDeg.Value = rb.rotation;
        }
        public void ApplyStateChanged(StateChangedEvent evt)
        {
            if (evt.Layer == StateLayer.Movement)
            {
                playerModel.CurrentMovementState.Value = ToMovementEnum(evt.ToState);
            }
            else
            {
                playerModel.CurrentActionState.Value = ToActionEnum(evt.ToState);
            }
        }

        private void UpdateAnimatorOnStateChanged(StateChangedEvent evt)
        {
            var anim = playerController?.animator;
            if (evt.Layer == StateLayer.Movement)
                anim.SetInteger("MovementState", (int)playerModel.CurrentMovementState.Value);
            else
                anim.SetInteger("ActionState", (int)playerModel.CurrentActionState.Value);
        }

        private MovementStates ToMovementEnum(string stateName)
        {
            return System.Enum.TryParse<MovementStates>(stateName, out var result) ? result : MovementStates.IdleState;
        }

        private ActionStates ToActionEnum(string stateName)
        {
            return System.Enum.TryParse<ActionStates>(stateName, out var result) ? result : ActionStates.NoActionState;
        }
        #endregion

        #region Helper
        private Vector2 groundUp => (Quaternion.Euler(0f, 0f, playerModel.TargetRotationDeg.Value) * Vector2.up).normalized;
        private Vector2 groundRight => (Quaternion.Euler(0f, 0f, playerModel.TargetRotationDeg.Value) * Vector2.right).normalized;
        private float vUpMove => Vector2.Dot(moveVel, groundUp);
        private float vRightMove => Vector2.Dot(moveVel, groundRight);
        private float vUpBonus => Vector2.Dot(bonusVel, groundUp);
        private float vRightBonus => Vector2.Dot(bonusVel, groundRight);
        #endregion
    }
}
