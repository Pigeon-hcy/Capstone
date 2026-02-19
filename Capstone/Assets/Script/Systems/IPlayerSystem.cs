using QFramework;
using UnityEngine;

namespace SkateGame
{
    public struct PendingActions
    {
        public bool JumpQueued;
        public bool JumpCutQueued;
        public bool PushQueued;
        public bool WallJumpQueued;
        public bool ReverseQueued;
        public bool GrappleImpulseQueued;
        public bool TrickBResetSpeedQueued;
        public bool TrickCLandQueued;
        public bool TrickARewardQueued;

        public bool Jumping;
        public bool Dashing;
        public bool Slamming;
        public bool Grapplling;
        public bool Grinding;
        public bool PowerGrinding;
        public bool Pushing;
        public bool HitStunning;

        // 触发一次后自动清空
        public void Clear()
        {
            JumpQueued = false;
            JumpCutQueued = false;
            PushQueued = false;
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
            JumpCutQueued = false;
            PushQueued = false;
            WallJumpQueued = false;
            ReverseQueued = false;
            GrappleImpulseQueued = false;
            TrickBResetSpeedQueued = false;
            TrickCLandQueued = false;
            TrickARewardQueued = false;
            Jumping = false;
            Dashing = false;
            Slamming = false;
            Grapplling = false;
            Grinding = false;
            PowerGrinding = false;
            Pushing = false;
            HitStunning = false;
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
        private float TrickARewardDirection;
        private float lastTrickBDirection;
        public bool IsPushingRight;
        public float TrickBDirection;
        public Vector2 GrappleDirection;
        
        private float pushSpeed;
        private float moveSpeed;
        private float powerGrindStartTime;
        private float powerGrindStartSpeed;
        private float powerGrindDirection;
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
            this.RegisterEvent<HitEvent>(OnHitInput);
            // 每次场景更新自动获取PlayerController
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // 场景加载后重新查找 PlayerController
            UpdatePlayerController();
            moveVel = Vector2.zero;
            bonusVel = Vector2.zero;
            pushSpeed = 0f;
            powerGrindStartSpeed = 0f;
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
        private void OnJumpInput(JumpExecuteEvent evt) { pending.JumpQueued = evt.IsJumping; pending.Jumping = evt.IsJumping; pending.JumpCutQueued = !evt.IsJumping; }
        private void OnWallJumpInput(WallJumpExecuteEvent evt) { pending.WallJumpQueued = true; }
        private void OnStateChanged(StateChangedEvent evt)
        {
            ApplyStateChanged(evt);
            UpdateAnimatorOnStateChanged(evt);
        }
        private void OnPushInput(PushInputEvent evt) { pending.PushQueued = evt.IsPushing; pending.Pushing = evt.IsPushing; IsPushingRight = evt.IsPushingRight; }
        private void OnPowerGrindInput(PowerGrindInputEvent evt) 
        { 
            pending.PowerGrinding = evt.IsPowerGrinding;
            if (evt.IsPowerGrinding)
            {
                powerGrindStartTime = Time.time;
                powerGrindStartSpeed = Mathf.Abs(pushSpeed);
                powerGrindDirection = Mathf.Sign(pushSpeed);
            }
            else
            {
                pushSpeed = 0f;
            }
        }
        private void OnReverseInput(ReverseInputEvent evt) { pending.ReverseQueued = true; }
        private void OnGrindInput(GrindInputEvent evt) { pending.Grinding = evt.IsGrinding; }
        private void OnTrickAInput(TrickAInputEvent evt) { }
        private void OnTrickBInput(TrickBInputEvent evt)
        {
            pending.Dashing = evt.IsTrickingB;
            if (pending.Dashing)
            {
                TrickBDirection = evt.Direction;
                lastTrickBDirection = evt.Direction;
            }
        }
        private void OnTrickCInput(TrickCInputEvent evt) { pending.Slamming = evt.IsTrickingC; }
        private void OnTrickBResetSpeed(TrickBResetSpeedEvent evt) { pending.TrickBResetSpeedQueued = true; }
        private void OnTrickCLand(TrickCLandEvent evt) { pending.TrickCLandQueued = true; }
        private void OnTrickAReward(TrickARewardEvent evt) { pending.TrickARewardQueued = true; TrickARewardDirection = evt.RewardDirection; }
        private void OnGrapple(GrappleEvent evt)
        {
            pending.GrappleImpulseQueued = evt.IsGrappling;
            pending.Grapplling = evt.IsGrappling;
            GrappleDirection = evt.pullDirection;
        }
        private void OnHitInput(HitEvent evt)
        {
            pending.HitStunning = evt.IsHitting;
        }
        #endregion

        #region Basic Movement
        public void ApplyMovement()
        {
            moveVel = rb.linearVelocity - bonusVel;
            bool isGrounded = playerModel.IsGrounded.Value;

            // Hit Stun
            if (pending.HitStunning) ApplyHit();
            else
            {
                // Base movement
                if (pending.PushQueued) ApplyPushBurst();
                if (pending.Pushing) ApplyPushSpeed();
                else if (pending.PowerGrinding) ApplyPowerGrind();
                playerModel.PushSpeed.Value = pushSpeed;
                ApplyHorizontalSpeed(cachedMoveInput);
            
                // Actions
                // 1: Reverse (deprecated)
                if (pending.ReverseQueued) ApplyReverse();
                // 2: leave ground
                if (pending.JumpQueued) ApplyJumpImpulse();
                if (pending.WallJumpQueued) ApplyWallJumpImpulse();
                if (pending.Jumping) ApplyJumpHeld();
                if (pending.JumpCutQueued) ApplyJumpCut();
                // 3: trick one-shots
                if (pending.TrickARewardQueued) ApplyTrickAReward(TrickARewardDirection);
                if (pending.TrickBResetSpeedQueued) ApplyTrickBResetSpeed(TrickBDirection != 0f ? TrickBDirection : lastTrickBDirection);
                if (pending.TrickCLandQueued) ApplyTrickCLand();
                if (pending.GrappleImpulseQueued) ApplyGrappleImpulse(GrappleDirection);
                // 4: sustained actions
                if (pending.Grinding) ApplyGrind();
                if (pending.Dashing) ApplyTrickB(TrickBDirection);
                if (pending.Slamming) ApplyTrickC();
                if (pending.Grapplling) ApplyGrappleForce(GrappleDirection);
            }
            
            // 5: gravity
            ApplyCustomGravity();
            
            // 6: ground support
            if (isGrounded)
            {
                if (vUpMove < 0f) { moveVel -= vUpMove * groundUp; }
                if (vUpBonus < 0f) { bonusVel -= vUpBonus * groundUp; }
                ApplySlopeCompensation();
            }

            pending.Clear();
            // Bonus velocity
            rb.linearVelocity = moveVel + bonusVel;
            bonusVel *= playerModel.Config.Value.bonusVelDecay;
            if (Mathf.Abs(bonusVel.x) < 0.01f && Mathf.Abs(bonusVel.y) < 0.01f)
                bonusVel = Vector2.zero;
        }


        private void ApplyPushBurst()
        {
            float pushDir = IsPushingRight ? 1f : -1f;
            if (Mathf.Abs(pushSpeed) < playerModel.Config.Value.pushBurstSpeed)
            {
                float burstSpeed = playerModel.Config.Value.pushBurstSpeed;
                if (playerModel.PushSpeedBeforeReverse.Value > 0f)
                {
                    burstSpeed = Mathf.Max(burstSpeed, Mathf.Min(playerModel.PushSpeedBeforeReverse.Value, playerModel.Config.Value.maxPushSpeed));
                    playerModel.PushSpeedBeforeReverse.Value = 0f;
                }
                pushSpeed = playerModel.Config.Value.pushBurstSpeed * pushDir;
                pushSpeed = burstSpeed * pushDir;
            }
        }

        private void ApplyPushSpeed()
        {
            float pushSpeedDelta = playerModel.Config.Value.maxPushSpeed - playerModel.Config.Value.pushBurstSpeed;
            float pushAccel = pushSpeedDelta / Mathf.Max(playerModel.Config.Value.pushTimeToMaxSpeed, 0.01f);
            float pushDir = IsPushingRight ? 1f : -1f;
            pushSpeed += pushDir * pushAccel * Time.fixedDeltaTime;
            pushSpeed = Mathf.Clamp(pushSpeed, -playerModel.Config.Value.maxPushSpeed, playerModel.Config.Value.maxPushSpeed);
        }
        private void ApplyPowerGrind()
        {
            float T = Mathf.Max(playerModel.Config.Value.powerGrindDuration, 0.01f);
            float scale = playerModel.Config.Value.powerGrindDistanceMultiplier;
            float t = Time.time - powerGrindStartTime;
            float newSpeed = powerGrindStartSpeed * (1f - Mathf.Pow(Mathf.Clamp01(t / T), scale));
            pushSpeed = powerGrindDirection * newSpeed;
        }

        private void ApplyHorizontalSpeed(float horizontalInput)
        {
            moveSpeed = 0f;
            if (Mathf.Abs(horizontalInput) > 0.01f && Mathf.Abs(pushSpeed) < playerModel.Config.Value.powerGrindStopSpeedThreshold)
            {
                moveSpeed = Mathf.Sign(horizontalInput) * playerModel.Config.Value.maxMoveSpeed;
            }
            moveVel = (pushSpeed + moveSpeed) * groundRight + vUpMove * groundUp;
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
                if (!pending.Slamming)
                    moveVel = new Vector2(moveVel.x, Mathf.Max(moveVel.y, playerModel.Config.Value.maxFallSpeed));
            }
        }

        private void ApplyCustomDamping(bool isGrounded)
        {
            float damping = isGrounded ? playerModel.Config.Value.groundLinearDamping : playerModel.Config.Value.airLinearDamping;
            moveVel *= Mathf.Exp(-damping * Time.fixedDeltaTime);
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
            // Vector2 upDir = playerModel.IsGrounded.Value ? groundUp : Vector2.up;
            Vector2 upDir = Vector2.up;
            moveVel -= Vector2.Dot(moveVel, upDir) * upDir;
            bonusVel -= Vector2.Dot(bonusVel, upDir) * upDir;
            moveVel += upDir * playerModel.Config.Value.jumpForce;
        }

        private void ApplyJumpHeld()
        {
            // Vector2 upDir = playerModel.IsGrounded.Value ? groundUp : Vector2.up;    
            Vector2 upDir = Vector2.up;
            float vUp = Vector2.Dot(moveVel, upDir);
            if (vUp < playerModel.Config.Value.jumpHoldForce)
            {
                moveVel -= vUp * upDir;
                moveVel += playerModel.Config.Value.jumpHoldForce * upDir;
            }
        }

        private void ApplyJumpCut()
        {
            Vector2 upDir = Vector2.up;
            moveVel -= Vector2.Dot(moveVel, upDir) * upDir * playerModel.Config.Value.jumpCutMultiplier;
        }

        private void ApplyWallJumpImpulse()
        {
            if (!playerModel.IsNearFgWall.Value) return;
            // reverse push speed
            ChangePushDirection(-1f);
            moveVel = (pushSpeed + moveSpeed) * groundRight + vUpMove * groundUp;
            // jump
            Vector2 normal = Quaternion.Euler(0f, 0f, playerModel.FgWallAngle.Value).normalized * Vector2.up;
            Vector2 jumpDir =  Vector2.Lerp(normal, Vector2.up, playerModel.Config.Value.wallJumpUpMultiplier).normalized;
            moveVel = new Vector2(0, 0);
            bonusVel = Vector2.zero;
            moveVel += jumpDir * playerModel.Config.Value.wallJumpForce *
                playerModel.Config.Value.wallJumpForceMultiplier;
        }
        #endregion

        #region Tricks
        private void ApplyReverse()
        {
            moveVel = new Vector2(-moveVel.x, moveVel.y);
            bonusVel = new Vector2(-bonusVel.x, bonusVel.y);
            ChangePushDirection(-1f);
        }

        private void ApplyGrind()
        {
        }

        private void ApplyTrickB(float direction)
        {
            float speed = Mathf.Max(playerModel.Config.Value.TrickBspeed, playerModel.VelocityBeforeTrick.Value * direction);
            ChangePushDirection(direction);
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
            float slamIntoSlope = Vector2.Dot(Vector2.down * playerModel.Config.Value.TrickCBoostspeed, groundRight);
            bonusVel += slamIntoSlope * groundRight;
        }

        private void ApplyTrickAReward(float direction)
        {
            moveVel = new Vector2(Mathf.Abs(moveVel.x) * direction, 0);
            if (direction != 0f) ChangePushDirection(direction);
            Vector2 rewardDir = new Vector2(direction, .5f).normalized;
            moveVel += rewardDir * playerModel.Config.Value.TrickARewardForce;
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

        #region Hit
        private void ApplyHit()
        {
            if (playerModel.HitKnockbackDirection.Value != Vector2.zero)
            {
                moveVel = Vector2.zero;
                bonusVel = Vector2.zero;
                pushSpeed = 0f;
                playerModel.PushSpeed.Value = pushSpeed;
                moveSpeed = 0f;
                bonusVel = playerModel.HitKnockbackDirection.Value.normalized * playerModel.Config.Value.hitKnockbackForce;
                playerModel.HitKnockbackDirection.Value = Vector2.zero;
            }
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
        private void ChangePushDirection(float direction)
        {
            pushSpeed = direction * Mathf.Abs(pushSpeed);
            playerModel.PushSpeed.Value = pushSpeed;
        }
        private Vector2 groundUp => (Quaternion.Euler(0f, 0f, playerModel.TargetRotationDeg.Value) * Vector2.up).normalized;
        private Vector2 groundRight => (Quaternion.Euler(0f, 0f, playerModel.TargetRotationDeg.Value) * Vector2.right).normalized;
        private float vUpMove => Vector2.Dot(moveVel, groundUp);
        private float vRightMove => Vector2.Dot(moveVel, groundRight);
        private float vUpBonus => Vector2.Dot(bonusVel, groundUp);
        private float vRightBonus => Vector2.Dot(bonusVel, groundRight);
        #endregion
    }
}
