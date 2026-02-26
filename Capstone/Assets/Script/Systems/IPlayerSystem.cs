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
            GrindResetSpeedQueued = false;
            TrickBResetSpeedQueued = false;
            TrickCLandQueued = false;
            TrickARewardQueued = false;
            PortalTeleportQueued = false;
        }
        public void ClearAll()
        {
            JumpQueued = false;
            JumpCutQueued = false;
            PushQueued = false;
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
        private Vector2 vPhysics;
        private Vector2 vPush;
        private Vector2 vMove;
        private Vector2 vOveride;
        private float cachedMoveInput;
        private PendingActions pending;
        private float TrickARewardDirection;
        private float lastTrickBDirection;
        private Vector2 PortalTeleportExitDirection;
        public bool IsPushingRight;
        public bool IsGraceWallJump;
        public float TrickBDirection;
        public Vector2 GrappleDirection;
        private bool isWalking;
        
        private float pushSpeed;
        private float moveSpeed;// movespeed != 0 means walking
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
            this.RegisterEvent<HitEvent>(OnHit);
            this.RegisterEvent<PlayerRespawnEvent>(OnPlayerRespawn);
            this.RegisterEvent<PortalTeleportEvent>(OnPortalTeleport);
            // 每次场景更新自动获取PlayerController
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // 场景加载后重新查找 PlayerController
            UpdatePlayerController();
            vPhysics = Vector2.zero;
            vPush = Vector2.zero;
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
        private void OnWallJumpInput(WallJumpExecuteEvent evt) { pending.WallJumpQueued = true; IsGraceWallJump = evt.IsGraceWallJump; }
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
        private void OnTrickAReward(TrickARewardEvent evt) { pending.TrickARewardQueued = true; TrickARewardDirection = evt.RewardDirection; }
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
            pushSpeed = 0f;
            powerGrindStartSpeed = 0f;
            pending.ClearAll();
        }
        private void OnPortalTeleport(PortalTeleportEvent evt)
        {
            pending.PortalTeleportQueued = true;
            PortalTeleportExitDirection = evt.ExitDirection;
        }
        #endregion

        #region Basic Movement
        public void ApplyMovement()
        {
            bool isGrounded = playerModel.IsGrounded.Value;

            // 0: gravity
            ApplyCustomGravity();
            // Hit Stun
            if (pending.HitStunning) ApplyHit();
            else
            {
                // 1: Base movement
                ProjectVPush(isGrounded);
                if (pending.PushQueued) ApplyPushBurst();
                if (pending.Pushing) ApplyPushSpeed();
                else if (pending.PowerGrinding) ApplyPowerGrind();

                
                // 2: ground support
                if (isGrounded)
                {
                    if (!isWalking)
                    {
                        if (vUpPhysics < 0f) { vPhysics -= Vector2.Dot(vPhysics, groundUp) * groundUp;}
                        // When nearly stopped on slope，stop sliding down
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
                if (pending.WallJumpQueued) ApplyWallJumpImpulse(IsGraceWallJump);
                if (pending.Jumping) ApplyJumpHeld();
                if (pending.JumpCutQueued) ApplyJumpCut();

                // 5: physics forces
                if (pending.TrickARewardQueued) ApplyTrickAReward(TrickARewardDirection);
                if (pending.TrickCLandQueued) ApplyTrickCLand();
                if (pending.GrappleImpulseQueued) ApplyGrappleImpulse(GrappleDirection);
                if (pending.Slamming) ApplyTrickC();
                if (pending.Grapplling) ApplyGrappleForce(GrappleDirection);

                // 6: overrides
                if (pending.Grinding) ApplyGrind();
                if (pending.GrindResetSpeedQueued) ApplyGrindResetSpeed();
                if (pending.Dashing) ApplyTrickB(TrickBDirection);
                if (pending.TrickBResetSpeedQueued) ApplyTrickBResetSpeed(TrickBDirection != 0f ? TrickBDirection : lastTrickBDirection);
                if (pending.PortalTeleportQueued) ApplyPortalTeleport();
                
                // 7: Check walk and update push speed
                playerModel.PushSpeed.Value = pushSpeed;
                CheckWalk(cachedMoveInput);
            }
            
            Debug.Log("vPhysics: " + vPhysics + " vPush: " + vPush + " vOveride: " + vOveride);
            Debug.Log("isGrounded: " + isGrounded);
            pending.Clear();
            rb.linearVelocity = vOveride==Vector2.zero ? vPhysics + vPush + vMove : vOveride;
            vPhysics*= playerModel.Config.Value.vPhysicsDecay;
            if (Mathf.Abs(vPhysics.x) < 0.01f && Mathf.Abs(vPhysics.y) < 0.01f) vPhysics = Vector2.zero;
        }

        private void ProjectVPush(bool isGrounded)
        {
            bool crash = Vector2.Dot(vPush, groundUp) < 0f;
            if (isGrounded && !isWalking)
            {
                pushSpeed = Vector2.Dot(vPush, groundRight);
                vPush = pushSpeed * groundRight;
            }
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
            if (Mathf.Abs(pushSpeed) < playerModel.Config.Value.powerGrindStopSpeedThreshold) isWalking = true;
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
            if (playerModel.IsGrounded.Value)
            {
                float intoSlope = Vector2.Dot(g, groundUp);
                Vector2 gravityTangent = g - intoSlope * groundUp;
                vPhysics += gravityTangent * Time.fixedDeltaTime;
            }
            else
            {
                vPhysics += g * Time.fixedDeltaTime;
                if (!pending.Slamming)
                    vPhysics = new Vector2(vPhysics.x, Mathf.Max(vPhysics.y, playerModel.Config.Value.maxFallSpeed+vPush.y));
            }
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
        private void ApplyJumpImpulse()
        {
            Vector2 upDir = Vector2.up;
            vPhysics -= Vector2.Dot(vPhysics, upDir) * upDir;
            vPhysics += upDir * playerModel.Config.Value.jumpForce;
        }

        private void ApplyJumpHeld()
        {
            // Vector2 upDir = playerModel.IsGrounded.Value ? groundUp : Vector2.up;   
            Vector2 upDir = Vector2.up;
            float vUp = Vector2.Dot(vPhysics, upDir);
            if (vUp < playerModel.Config.Value.jumpHoldForce)
            {
                vPhysics -= vUp * upDir;
                vPhysics += playerModel.Config.Value.jumpHoldForce * upDir;
            }
        }

        private void ApplyJumpCut()
        {
            Vector2 upDir = Vector2.up;
            vPhysics -= Vector2.Dot(vPhysics, upDir) * upDir * playerModel.Config.Value.jumpCutMultiplier;
        }

        private void ApplyWallJumpImpulse(bool isGraceWallJump)
        {
            Vector2 wallN = isGraceWallJump ? playerModel.WallJumpWallNormal.Value : Quaternion.Euler(0f, 0f, playerModel.FgWallAngle.Value) * Vector2.up;
            Vector2 jumpDir = Vector2.Lerp(wallN, Vector2.up, playerModel.Config.Value.wallJumpUpMultiplier).normalized; vPhysics = jumpDir * playerModel.Config.Value.wallJumpImpulse;
            bool isJumpright = wallN.x > 0;
            pushSpeed = isJumpright ? playerModel.Config.Value.wallJumpSpeed : -playerModel.Config.Value.wallJumpSpeed;
            vPush = pushSpeed* groundRight;
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
            pushSpeed = Mathf.Max(pushSpeed, playerModel.Config.Value.grindResetSpeed) * Mathf.Sign(playerModel.GrindDirection.Value.x);
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
            vPhysics = slamIntoSlope * groundRight;
        }

        private void ApplyTrickAReward(float direction)
        {
            if (direction != 0f) 
            {
                ChangePushDirection(direction);
            }
            vPush = pushSpeed* groundRight;
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
            pushSpeed = PortalTeleportExitDirection.magnitude * Mathf.Sign(PortalTeleportExitDirection.x);
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
