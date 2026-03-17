using QFramework;
using Unity.Mathematics;
using UnityEngine;

namespace SkateGame
{
    public struct PendingActions
    {
        public bool JumpQueued;
        public bool JumpCutQueued;
        public bool WallJumpQueued;
        public bool ReverseQueued;
        public bool GrappleImpulseQueued;
        public bool GrindResetSpeedQueued;
        public bool TrickBResetSpeedQueued;
        public bool TrickCLandQueued;
        public bool TrickARewardQueued;
        public bool PortalTeleportQueued;

        public bool Jumping;
        public bool Dashing;
        public bool Slamming;
        public bool Grapplling;
        public bool Grinding;
        public bool PowerGrinding;
        public bool pushQueued;
        public bool HitStunning;

        // 触发一次后自动清空
        public void Clear()
        {
            JumpQueued = false;
            JumpCutQueued = false;
            WallJumpQueued = false;
            ReverseQueued = false;
            GrappleImpulseQueued = false;
            GrindResetSpeedQueued = false;
            TrickBResetSpeedQueued = false;
            TrickCLandQueued = false;
            TrickARewardQueued = false;
            PortalTeleportQueued = false;
            pushQueued = false;
        }
        public void ClearAll()
        {
            JumpQueued = false;
            JumpCutQueued = false;
            WallJumpQueued = false;
            ReverseQueued = false;
            GrappleImpulseQueued = false;
            GrindResetSpeedQueued = false;
            TrickBResetSpeedQueued = false;
            TrickCLandQueued = false;
            TrickARewardQueued = false;
            PortalTeleportQueued = false;
            Jumping = false;
            Dashing = false;
            Slamming = false;
            Grapplling = false;
            Grinding = false;
            PowerGrinding = false;
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
        private Vector2 vPhysics;
        private Vector2 vPush;
        private Vector2 vMove;
        private Vector2 vOveride;
        private float cachedMoveInput;
        private PendingActions pending;
        private float lastTrickBDirection;
        private Vector2 PortalTeleportExitDirection;
        public bool IsPushingRight;
        public bool isFirstPush;
        public bool IsGraceWallJump;
        public float TrickBDirection;
        public Vector2 GrappleDirection;
        private bool isWalking;

        private bool isGrounded;
        private bool wasGrounded;
        Vector2 jumpDir;
        private float pushSpeed;
        private float moveSpeed;// movespeed != 0 means walking
        private float powerGrindStartTime;
        private float powerGrindStartSpeed;
        private float powerGrindDirection;
        
        private bool isInUpdraft;
        private Vector2 updraftDirection;
        private float updraftForce;
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
            this.RegisterEvent<HitEvent>(OnHit);
            this.RegisterEvent<PlayerRespawnEvent>(OnPlayerRespawn);
            this.RegisterEvent<PortalTeleportEvent>(OnPortalTeleport);
            this.RegisterEvent<UpperAirEvent>(OnUpdraft);
            // 每次场景更新自动获取PlayerController
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // 场景加载后重新查找 PlayerController
            UpdatePlayerController();
            vPhysics = Vector2.zero;
            vPush = Vector2.zero;
            vOveride = Vector2.zero;
            pushSpeed = 0f;
            powerGrindStartSpeed = 0f;
            pending.ClearAll();
            isInUpdraft = false;
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
        private void OnWallJumpInput(WallJumpExecuteEvent evt) { pending.WallJumpQueued = true; IsGraceWallJump = evt.IsGraceWallJump; }
        private void OnStateChanged(StateChangedEvent evt)
        {
            ApplyStateChanged(evt);
            UpdateAnimatorOnStateChanged(evt);
        }
        private void OnPushInput(PushInputEvent evt)
        {
            IsPushingRight = evt.IsPushingRight;
            isFirstPush = evt.isFirstPush;
            if (evt.IsPushing) pending.pushQueued = true;
        }
        private void OnPowerGrindInput(PowerGrindInputEvent evt) 
        { 
            pending.PowerGrinding = evt.IsPowerGrinding;
            if (evt.IsPowerGrinding)
            {
                powerGrindStartTime = Time.time;
                powerGrindStartSpeed = Mathf.Abs(pushSpeed);
                powerGrindDirection = Mathf.Sign(pushSpeed);
            }
            // If not interrupted, ensure push speed is stopped
            else if (!evt.IsInterrupted)
            {
                // TODO: queue it
                pushSpeed = 0f;
                vPush = Vector2.zero;
            }
        }
        private void OnReverseInput(ReverseInputEvent evt) { pending.ReverseQueued = true; }
        private void OnGrindInput(GrindInputEvent evt) 
        { 
            pending.Grinding = evt.IsGrinding; 
            pending.GrindResetSpeedQueued = !evt.IsGrinding;
        }
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
        private void OnTrickAReward(TrickARewardEvent evt) { pending.TrickARewardQueued = true; }
        private void OnGrapple(GrappleEvent evt)
        {
            pending.GrappleImpulseQueued = evt.IsGrappling;
            pending.Grapplling = evt.IsGrappling;
            GrappleDirection = evt.pullDirection;
        }
        private void OnHit(HitEvent evt)
        {
            pending.HitStunning = evt.IsHitting;
        }
        private void OnPlayerRespawn(PlayerRespawnEvent evt)
        {
            vPhysics = Vector2.zero;
            vPush = Vector2.zero;
            vOveride = Vector2.zero;
            pushSpeed = 0f;
            powerGrindStartSpeed = 0f;
            pending.ClearAll();
            isInUpdraft = false;
        }
        private void OnPortalTeleport(PortalTeleportEvent evt)
        {
            pending.PortalTeleportQueued = true;
            PortalTeleportExitDirection = evt.ExitDirection;
        }
        private void OnUpdraft(UpperAirEvent evt)
        {
            isInUpdraft = evt.IsTriggerEnter;
            if (evt.IsTriggerEnter)
            {
                updraftDirection = evt.Direction.normalized;
                updraftForce = evt.ForceMagnitude;
            }
        }
        #endregion

        #region Basic Movement
        public void ApplyMovement()
        {
            wasGrounded = isGrounded;
            isGrounded = playerModel.IsGrounded.Value;

            if (playerModel.CurrentMovementState.Value == MovementStates.WallSlideState) ProjectVelocityToWall();
            // clear slope-accumulated horizontal on leaving ground
            if (wasGrounded && !isGrounded) vPhysics.x = 0f;
            // 0: gravity
            ApplyCustomGravity();
            // Hit Stun
            if (pending.HitStunning) ApplyHit();
            else
            {
                // 1: Base movement
                if(isGrounded) ProjectVPush();
                if (pending.pushQueued)
                    ApplyPushKick(isFirstPush);
                else if (pending.PowerGrinding) ApplyPowerGrind();

                // 2: ground support
                if (isGrounded)
                {
                    if (!wasGrounded) ReconcilePushOnLanding();
                    if (!isWalking)
                    {
                        if (vUpPhysics < 0f) vPhysics -= vUpPhysics * groundUp;
                        if (Mathf.Abs(vRightPhysics) > playerModel.Config.Value.maxSlopeSlideSpeed)
                        {
                            float sign = Mathf.Sign(vRightPhysics);
                            vPhysics -= vRightPhysics * groundRight;
                            vPhysics += sign * playerModel.Config.Value.maxSlopeSlideSpeed * groundRight;
                        }
                        // When nearly stopped on slope, stop sliding down
                        float totalSpeed = Vector2.Dot(vPhysics + vPush, groundRight);
                        if (Mathf.Abs(totalSpeed) < playerModel.Config.Value.powerGrindStopSpeedThreshold)
                            vPhysics -= Vector2.Dot(vPhysics, groundRight) * groundRight;
                    }
                    else // High friction when walking
                    {
                        vPhysics = Vector2.zero;
                    }
                    // ApplySlopeCompensation();
                }
                // Actions
                // 3: Reverse (deprecated)
                // if (pending.ReverseQueued) ApplyReverse();
                // 4: jumps
                if (pending.JumpQueued) ApplyJumpImpulse();
                if (pending.WallJumpQueued) ApplyWallJumpImpulse();
                if (pending.Jumping) ApplyJumpHeld();
                if (pending.JumpCutQueued) ApplyJumpCut();

                // 5: physics forces
                if (pending.TrickARewardQueued) ApplyTrickAReward();
                if (pending.GrappleImpulseQueued) ApplyGrappleImpulse(GrappleDirection);
                if (pending.Slamming) ApplyTrickC();
                if (pending.TrickCLandQueued) ApplyTrickCLand();
                // FOR JERRY'S AUDIO - HOOK SWING
                if (pending.Grapplling) ApplyGrappleForce(GrappleDirection);
                if (isInUpdraft) ApplyUpdraft();

                // 6: overrides
                if (pending.Grinding) ApplyGrind();
                if (pending.GrindResetSpeedQueued) ApplyGrindResetSpeed();
                if (pending.Dashing) ApplyTrickB(TrickBDirection);
                if (pending.TrickBResetSpeedQueued) ApplyTrickBResetSpeed(TrickBDirection != 0f ? TrickBDirection : lastTrickBDirection);
                if (pending.PortalTeleportQueued) ApplyPortalTeleport();
                
                // 7: Check walk
                CheckWalk(cachedMoveInput);
            }
            
            pending.Clear();
            rb.linearVelocity = vOveride == Vector2.zero ? vPhysics + vPush + vMove : vOveride;

            if (isGrounded) vPhysics = new Vector2(vPhysics.x * playerModel.Config.Value.vPhysicsDecay, vPhysics.y);
            if (Mathf.Abs(vPhysics.x) < 0.01f && Mathf.Abs(vPhysics.y) < 0.01f) vPhysics = Vector2.zero;

            if (isGrounded) pushSpeed = pushSpeed * playerModel.Config.Value.vPushDecay;
            if (Mathf.Abs(pushSpeed) < 0.01f) pushSpeed = 0f;
            vPush = Mathf.Abs(pushSpeed) * vPush.normalized;
            
            playerModel.PushSpeed.Value = pushSpeed;
            Debug.Log($"Applied Movement: vPhysics={vPhysics}, vPush={vPush}, vMove={vMove}, pushSpeed={pushSpeed}");
        }

        private void ProjectVPush()
        {
            bool crashPush = Vector2.Dot(vPush, groundUp) < 0f;
            if (!wasGrounded || (crashPush && !isWalking))
                vPush = pushSpeed * groundRight;
            bool crashPhysics = Vector2.Dot(vPhysics, groundUp) < 0f;
            if (!wasGrounded || crashPhysics)
                vPhysics = Vector2.Dot(vPhysics, groundRight) * groundRight;
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
                pushSpeed = burstSpeed * pushDir;
                vPush = pushSpeed * groundRight;
            }
        }

        private void ApplyPushKick(bool isFirstPush)
        {
            float pushDir = IsPushingRight ? 1f : -1f;
            float newSpeed;
            if (isFirstPush && Mathf.Abs(pushSpeed) < playerModel.Config.Value.pushBurstSpeed)
            {
                newSpeed = playerModel.Config.Value.pushBurstSpeed;
            }
            else newSpeed = Mathf.Abs(pushSpeed) + playerModel.Config.Value.pushKickSpeedIncrement;
            pushSpeed = pushDir * Mathf.Min(newSpeed, playerModel.Config.Value.maxPushSpeed);
            vPush = pushSpeed * groundRight;
        }

        private void ApplyPushSpeed()
        {
            float pushSpeedDelta = playerModel.Config.Value.maxPushSpeed - playerModel.Config.Value.pushBurstSpeed;
            float pushAccel = pushSpeedDelta / Mathf.Max(playerModel.Config.Value.pushTimeToMaxSpeed, 0.01f);
            float pushDir = IsPushingRight ? 1f : -1f;
            pushSpeed += pushDir * pushAccel * Time.fixedDeltaTime;
            pushSpeed = Mathf.Clamp(pushSpeed, -playerModel.Config.Value.maxPushSpeed, playerModel.Config.Value.maxPushSpeed);
            vPush = Mathf.Abs(pushSpeed) * vPush.normalized;
        }
        private void ApplyPowerGrind()
        {
            float T = Mathf.Max(playerModel.Config.Value.powerGrindDuration, 0.01f);
            float scale = playerModel.Config.Value.powerGrindDistanceMultiplier;
            float t = Time.time - powerGrindStartTime;
            float newSpeed = powerGrindStartSpeed * (1f - Mathf.Pow(Mathf.Clamp01(t / T), scale));
            pushSpeed = powerGrindDirection * newSpeed;
            vPush = Mathf.Abs(pushSpeed) * vPush.normalized;
        }

        private void CheckWalk(float horizontalInput)
        {
            moveSpeed = 0f;
            if (!playerModel.IsSlidingWall.Value &&
                 Mathf.Abs(pushSpeed) < playerModel.Config.Value.powerGrindStopSpeedThreshold)
            {
                isWalking = true;
            }
            else isWalking = false;
            if (Mathf.Abs(horizontalInput) > 0.01f && isWalking)
            {
                moveSpeed = Mathf.Sign(horizontalInput) * playerModel.Config.Value.maxMoveSpeed;
            }
            vMove = moveSpeed * groundRight;
        }

        private void ApplyCustomGravity()
        {
            Vector2 g = Vector2.down * playerModel.Config.Value.gravityMagnitude * playerModel.CurrentGravityScale.Value;
            if (playerModel.IsGrounded.Value) ApplyGroundGravity(g);
            else ApplyAirGravity(g);
        }

        private void ApplyGroundGravity(Vector2 g)
        {
            float intoSlope = Vector2.Dot(g, groundUp);
            vPhysics += (g - intoSlope * groundUp) * Time.fixedDeltaTime;
        }

        private void ApplyAirGravity(Vector2 g)
        {
            vPhysics += g * Time.fixedDeltaTime;
            if (!pending.Slamming)
                vPhysics = new Vector2(vPhysics.x, Mathf.Max(vPhysics.y, playerModel.Config.Value.maxFallSpeed-vPush.y-vMove.y));
        }

        // On landing, if momentum has reversed direction, adopt actual speed as new pushSpeed
        private void ReconcilePushOnLanding()
        {
            float landingSlope = Vector2.Dot(rb.linearVelocity, groundRight);
            Debug.Log("groundRight"+ groundRight + "landigslope: " + landingSlope + ", new pushSpeed: " + pushSpeed);
            if (pushSpeed == 0f || Mathf.Sign(landingSlope) == Mathf.Sign(pushSpeed)) return;
            if (Mathf.Abs(landingSlope) <= playerModel.Config.Value.powerGrindStopSpeedThreshold) return;
            pushSpeed = Mathf.Clamp(landingSlope, -playerModel.Config.Value.maxPushSpeed, playerModel.Config.Value.maxPushSpeed);
            vPhysics -= Vector2.Dot(vPhysics, groundRight) * groundRight;
            vPush = pushSpeed * groundRight;
        }

        private void ApplyCustomDamping(bool isGrounded)
        {
            float damping = isGrounded ? playerModel.Config.Value.groundLinearDamping : playerModel.Config.Value.airLinearDamping;
            vPhysics *= Mathf.Exp(-damping * Time.fixedDeltaTime);
        }
        #endregion

        #region Ground & Slope
        private void ApplySlopeCompensation()
        {
            Vector2 g = Vector2.down * playerModel.Config.Value.gravityMagnitude * playerModel.CurrentGravityScale.Value;
            Vector2 gTangent = Vector2.Dot(g, groundRight) * groundRight * Mathf.Sign(vRightPhysics);
            vPhysics += -playerModel.Config.Value.slopeCompensationForce * gTangent * Time.fixedDeltaTime;
        }

        private void ApplyGroundForce()
        {
            Vector2 down = (Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.down).normalized;
            vPhysics += down * (playerModel.Config.Value.groundForce * Time.fixedDeltaTime);
        }
        #endregion
        #region Jumps
        // 沿push方向跳跃
        private void ApplyJumpImpulse()
        {
            Vector2 upReflected = -Vector2.Dot(Vector2.up, groundRight) * groundRight;

            float uphillAmount = Mathf.Sign(pushSpeed) * Vector2.Dot(groundRight, Vector2.up);
            float speedFactor = Mathf.Clamp01(Mathf.Abs(pushSpeed) / playerModel.Config.Value.maxPushSpeed);
            float t = 0.5f + 0.5f * uphillAmount * speedFactor;
            Vector2 upDir = Vector2.Lerp(upReflected, Vector2.up, t).normalized;
            jumpDir = upDir;
            vPhysics = upDir * playerModel.Config.Value.jumpForce;
        }

        private void ApplyJumpHeld()
        {
            // Vector2 upDir = playerModel.IsGrounded.Value ? groundUp : Vector2.up;   
            Vector2 upDir = jumpDir;
            float vUp = Vector2.Dot(vPhysics, upDir);
            if (vUp < playerModel.Config.Value.jumpHoldForce)
                vPhysics += (playerModel.Config.Value.jumpHoldForce - vUp) * upDir;
        }

        private void ApplyJumpCut()
        {
            Vector2 upDir = Vector2.up;
            vPhysics -= Vector2.Dot(vPhysics, upDir) * upDir * playerModel.Config.Value.jumpCutMultiplier;
            vPush = new Vector2(vPush.x, Mathf.Min(vPush.y, 0f));
            pushSpeed = vPush.magnitude * Mathf.Sign(vPush.x);
        }

        private void ProjectVelocityToWall()
        {
            Vector2 wallTangent = groundRight;
            vPhysics = Vector2.Dot(vPhysics, wallTangent) * wallTangent;
            float projPush = Vector2.Dot(vPush, wallTangent);
            pushSpeed = projPush;
            vPush = projPush * wallTangent;
        }

        private void ApplyWallJumpImpulse()
        {
            Vector2 wallN = playerModel.WallJumpWallNormal.Value;
            Vector2 jumpDir = Vector2.Lerp(wallN, Vector2.up, playerModel.Config.Value.wallJumpUpMultiplier).normalized; 
            vPhysics = jumpDir * playerModel.Config.Value.wallJumpImpulse;
            bool isJumpright = wallN.x > 0;
            pushSpeed = isJumpright ? playerModel.Config.Value.wallJumpSpeed : -playerModel.Config.Value.wallJumpSpeed;
            vPush = playerModel.Config.Value.wallJumpSpeed * wallN;
        }
        #endregion

        #region Tricks
        private void ApplyReverse()
        {
            pushSpeed = -pushSpeed;
            vPush = pushSpeed * groundRight;
            ChangePushDirection(-1f);
        }

        private void ApplyGrind()
        {
            vOveride = playerModel.GrindDirection.Value * playerModel.Config.Value.grindSpeed;
        }

        private void ApplyGrindResetSpeed()
        {
            pushSpeed = Mathf.Max(Mathf.Abs(pushSpeed), playerModel.Config.Value.grindResetSpeed) * Mathf.Sign(playerModel.GrindDirection.Value.x);
            vPush = pushSpeed * groundRight;
            vOveride = Vector2.zero;
        }

        private void ApplyTrickB(float direction)
        {
            float speed = Mathf.Max(playerModel.Config.Value.TrickBspeed, playerModel.VelocityBeforeTrick.Value * direction);
            ChangePushDirection(direction);
            vPush = Vector2.right * pushSpeed;
            vOveride = new Vector2(direction * speed, 0);
        }
        private void ApplyTrickBResetSpeed(float direction)
        {
            vOveride = Vector2.zero;
        }
        private void ApplyTrickC()
        {
            vPhysics = new Vector2(vPhysics.x, -playerModel.Config.Value.TrickCspeed);
        }
        private void ApplyTrickCLand()
        {
            float slamIntoSlope = Vector2.Dot(Vector2.down * playerModel.Config.Value.TrickCBoostspeed, groundRight);
            if (Mathf.Sign(slamIntoSlope) == Mathf.Sign(pushSpeed)) vPhysics = slamIntoSlope * groundRight;
            else vPhysics = Vector2.zero;
        }

        private void ApplyTrickAReward()
        {
            float direction = 0f;
            // if (direction != 0f) 
            // {
            //     ChangePushDirection(direction);
            // }
            // vPush = pushSpeed* groundRight;
            Vector2 rewardDir = new Vector2(direction, .5f).normalized;
            float rewardForce = direction == 0f ? playerModel.Config.Value.TrickARewardForceVertical : playerModel.Config.Value.TrickARewardForceHorizontal;
            vPhysics = rewardDir * rewardForce;
        }
        private void ApplyGrappleImpulse(Vector2 dir)
        {
            vPhysics += dir * playerModel.Config.Value.grappleImpulse;
        }
        private void ApplyGrappleForce(Vector2 dir)
        {
            vPhysics += dir * playerModel.Config.Value.grappleForce * Time.fixedDeltaTime;
        }
        private void ApplyUpdraft()
        {
            vPhysics += updraftDirection * (updraftForce * Time.fixedDeltaTime);
        }
        #endregion

        #region Hit
        private void ApplyHit()
        {
            if (playerModel.HitKnockbackDirection.Value != Vector2.zero)
            {
                vOveride = Vector2.zero;
                vPhysics = playerModel.HitKnockbackDirection.Value.normalized * playerModel.Config.Value.hitKnockbackForce;
                vPush = Vector2.zero;
                pushSpeed = 0f;
                moveSpeed = 0f;
                playerModel.HitKnockbackDirection.Value = Vector2.zero;
            }
        }
        private void ApplyPortalTeleport()
        {
            vPush = pushSpeed * PortalTeleportExitDirection;
            pushSpeed = vPush.magnitude * Mathf.Sign(PortalTeleportExitDirection.x);
            vPhysics = Vector2.zero;
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
        }
        private Vector2 groundUp => (Quaternion.Euler(0f, 0f, playerModel.TargetRotationDeg.Value) * Vector2.up).normalized;
        private Vector2 groundRight => (Quaternion.Euler(0f, 0f, playerModel.TargetRotationDeg.Value) * Vector2.right).normalized;
        private float vUpPhysics => Vector2.Dot(vPhysics, groundUp);
        private float vRightPhysics => Vector2.Dot(vPhysics, groundRight);
        #endregion
    }
}
