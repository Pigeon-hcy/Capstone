using QFramework;
using UnityEngine;

namespace SkateGame
{
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
        private Vector2 moveVel;
        private Vector2 bonusVel;
        private float cachedMoveInput;
        private bool jumpQueued;
        private bool wallJumpQueued;
        private bool reverseQueued;
        private bool pushing;
        private bool powerGrinding;
        private bool grinding;
        // Tricks
        private bool trickingB;
        private float trickBdirection;
        private bool trickingC;
        private bool trickBResetSpeedQueued;
        private bool trickCLandQueued;
        private bool trickARewardQueued;
        private bool grappleQueued;
        private bool grappling;

        private Vector2 grappleDirection;
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

        // 输入事件处理
        #region Event
        private void OnMoveInput(MoveInputEvent evt)
        {
            cachedMoveInput = 0;
            cachedMoveInput = evt.HorizontalInput;
        }
        private void OnJumpInput(JumpExecuteEvent evt)
        {
            jumpQueued = true;
        }
        private void OnWallJumpInput(WallJumpExecuteEvent evt)
        {
            wallJumpQueued = true;
        }
        private void OnStateChanged(StateChangedEvent evt)
        {
            ApplyStateChanged(evt);
            UpdateAnimatorOnStateChanged(evt);
        }
        private void OnPushInput(PushInputEvent evt)
        {
            pushing = evt.IsPushing;
        }
        private void OnPowerGrindInput(PowerGrindInputEvent evt)
        {
            powerGrinding = evt.IsPowerGrinding;
        }
        private void OnReverseInput(ReverseInputEvent evt)
        {
            reverseQueued = true;
        }
        private void OnGrindInput(GrindInputEvent evt)
        {
            grinding = evt.IsGrinding;
        }
        private void OnTrickAInput(TrickAInputEvent evt)
        {
        }
        private void OnTrickBInput(TrickBInputEvent evt)
        {
            trickingB = evt.IsTrickingB;
            if(trickingB){ trickBdirection = evt.Direction; }
        }
        private void OnTrickCInput(TrickCInputEvent evt)
        {
            trickingC = evt.IsTrickingC;
        }
        private void OnTrickBResetSpeed(TrickBResetSpeedEvent evt)
        {
            trickBResetSpeedQueued = true;
        }
        private void OnTrickCLand(TrickCLandEvent evt)
        {
            trickCLandQueued = true;
        }
        private void OnTrickAReward(TrickARewardEvent evt)
        {
            trickARewardQueued = true;
        }
        private void OnGrapple(GrappleEvent evt)
        {
            grappleQueued = true;
            grappling = evt.IsGrappling;
            grappleDirection = evt.pullDirection;
        }
        #endregion

        #region Method
        public void ApplyMovement()
        {
            moveVel = rb.linearVelocity - bonusVel;
            bool isGrounded = playerModel.IsGrounded.Value;
            ApplyCustomGravity();
            ApplyHorizontalSpeed(cachedMoveInput, isGrounded, pushing);
            if (isGrounded)
            {
                // Remove velocity into ground
                if (vUp < 0f) { moveVel -= vUp * up; }
                ApplySlopeCompensation();
                // ApplyGroundForce(); 
                ClampGroundSpeed();
            }
            if (powerGrinding) ApplyPowerGrind();
            if (reverseQueued) { ApplyReverse(); reverseQueued = false; }
            if (jumpQueued) { ApplyJumpImpulse(); jumpQueued = false; }
            if (wallJumpQueued) { ApplyWallJumpImpulse(); wallJumpQueued = false; }
            if (trickARewardQueued) { ApplyTrickAReward(); trickARewardQueued = false; }
            if (grinding) ApplyGrind();
            if (trickingB) ApplyTrickB();
            if (trickingC) ApplyTrickC();
            if (trickBResetSpeedQueued) { ApplyTrickBResetSpeed(); trickBResetSpeedQueued = false; }
            if (trickCLandQueued) { ApplyTrickCLand(); trickCLandQueued = false; }
            if (grappleQueued) { ApplyGrappleImpulse(); grappleQueued = false; }
            if (grappling) ApplyGrappleForce();
            ApplyCustomDamping(isGrounded);

            rb.linearVelocity = moveVel + bonusVel;
            bonusVel *= playerModel.Config.Value.bonusVelDecay;
            if (Mathf.Abs(bonusVel.x) < 0.01f && Mathf.Abs(bonusVel.y) < 0.01f)
            {
                bonusVel = Vector2.zero;
            }
        }
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

        private void ApplyHorizontalSpeed(float horizontalInput, bool isGrounded, bool pushing)
        {
            if (pushing)
            {
                moveVel += right * (playerModel.IsFacingRight.Value ? 1 : -1) * playerModel.Config.Value.pushAccel * Time.fixedDeltaTime;
            }
            else if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                // 如果当前速度和输入方向相反，且速度大于0.1f，则减速
                if (Mathf.Sign(vRight) != Mathf.Sign(horizontalInput) && Mathf.Abs(vRight) > 0.1f)
                {
                    float turnDecel = isGrounded ? playerModel.Config.Value.turnDecel : playerModel.Config.Value.airTurnDecel;
                    turnDecel *= Mathf.Pow(Mathf.Abs(vRight), playerModel.Config.Value.stopDecelIncrement);
                    moveVel -= right * Mathf.Sign(vRight) * turnDecel * Time.fixedDeltaTime;
                    // 减速最多到0
                    if (Mathf.Sign(vRight) == Mathf.Sign(horizontalInput))
                        moveVel = vUp * up;
                }
                else if (Mathf.Abs(vRight) < playerModel.Config.Value.maxMoveSpeed)
                {
                    float accel = isGrounded ? playerModel.Config.Value.groundAccel : playerModel.Config.Value.airAccel;
                    moveVel += right * horizontalInput * accel * Time.fixedDeltaTime;
                }
            }
        }

        private void ApplyJumpImpulse()
        {
			// Override up direction, force up when not grounded
			Vector2 up = playerModel.IsGrounded.Value ? 
                (Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.up).normalized : Vector2.up;
			moveVel -= vUp * up;
            bonusVel -= Vector2.Dot(bonusVel, up) * up;
            moveVel += up * playerModel.Config.Value.maxJumpForce;
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

        private void ApplyPowerGrind()
        {
            float deceleration = playerModel.Config.Value.powerGrindDeceleration;
            float direction = Mathf.Sign(vRight);
            // 逐渐减少的速度，保持方向不变
            float newVx = vRight - direction * deceleration * Time.fixedDeltaTime;
            bonusVel.x -= direction * deceleration * Time.fixedDeltaTime;
            // 防止越过零点
            if (Mathf.Sign(newVx) != direction || Mathf.Abs(newVx) < 0.01f) newVx = 0f;
            moveVel = newVx * right + vUp * up;
        }

        private void ApplyReverse()
        {
            moveVel = new Vector2(-moveVel.x, moveVel.y);
            bonusVel = new Vector2(-bonusVel.x, bonusVel.y);
        }

        private void ApplyGrind()
        {
        }
        
        private void ApplyTrickB()
        {
            float speed = Mathf.Max(playerModel.Config.Value.TrickBspeed, playerModel.VelocityBeforeTrick.Value * trickBdirection);
            moveVel = new Vector2(trickBdirection * speed, 0);
            bonusVel = Vector2.zero;
        }
        private void ApplyTrickBResetSpeed()
        {
            float speed = Mathf.Max(playerModel.Config.Value.maxMoveSpeed, playerModel.VelocityBeforeTrick.Value * trickBdirection);
            moveVel = new Vector2(speed * trickBdirection, 0);
            bonusVel = Vector2.zero;
        }
        private void ApplyTrickC()
        {
            moveVel = new Vector2(moveVel.x, -playerModel.Config.Value.TrickCspeed);
            bonusVel = Vector2.zero;
        }
        private void ApplyTrickCLand()
        {
            bonusVel += Vector2.down * playerModel.Config.Value.TrickCBoostspeed;
        }

        private void ApplyTrickAReward()
        {
            moveVel = new Vector2(moveVel.x, 0);
            moveVel += Vector2.up * playerModel.Config.Value.maxJumpForce;
        }
        private void ApplyGrappleImpulse()
        {
            moveVel += grappleDirection * playerModel.Config.Value.grappleImpulse;
        }
        private void ApplyGrappleForce()
        {
            moveVel += grappleDirection * playerModel.Config.Value.grappleForce * Time.fixedDeltaTime;
        }

        // 将玩家稍微吸向地面
        private void ApplyGroundForce()
        {
            Vector2 down = (Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.down).normalized;
            moveVel += down * (playerModel.Config.Value.groundForce * Time.fixedDeltaTime);
        }

        private void ApplyCustomGravity()
        {
            Vector2 g = Vector2.down * playerModel.Config.Value.gravityMagnitude * playerModel.CurrentGravityScale.Value;
            if (playerModel.IsGrounded.Value)
            {
                float intoSlope = Vector2.Dot(g, up);
                Vector2 gravityTangent = g - intoSlope * up;
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

        // 如果坡度发生变化，补偿损失的速度
        private void ApplySlopeCompensation()
        {
            Vector2 g = Vector2.down * playerModel.Config.Value.gravityMagnitude * playerModel.CurrentGravityScale.Value;
            Vector2 gTangent = Vector2.Dot(g, right) * right * Mathf.Sign(vRight);
            moveVel += -playerModel.Config.Value.slopeCompensationForce * gTangent * Time.fixedDeltaTime;
        }

        private void ClampGroundSpeed()
        {
            float newVRight = Mathf.Clamp(vRight, -playerModel.Config.Value.maxMoveSpeed, playerModel.Config.Value.maxMoveSpeed);
            moveVel = newVRight * right + vUp * up;
        }

        #endregion

        #region Helper

        // Direction
        private Vector2 up => (Quaternion.Euler(0f, 0f, playerModel.TargetRotationDeg.Value) * Vector2.up).normalized;
        private Vector2 right => (Quaternion.Euler(0f, 0f, playerModel.TargetRotationDeg.Value) * Vector2.right).normalized;
        private float vUp => Vector2.Dot(moveVel, up);
        private float vRight => Vector2.Dot(moveVel, right);


        // Update animator on state changed
        private void UpdateAnimatorOnStateChanged(StateChangedEvent evt)
        {
            var anim = playerController?.animator;
            if (evt.Layer == StateLayer.Movement)
            {
                anim.SetInteger("MovementState", (int)playerModel.CurrentMovementState.Value);
            }
            else
            {
                anim.SetInteger("ActionState", (int)playerModel.CurrentActionState.Value);
            }
        }
        
        private MovementStates ToMovementEnum(string stateName)
        {
            if (System.Enum.TryParse<MovementStates>(stateName, out var result))
            {
                return result;
            }
                return MovementStates.IdleState;
        }

        private ActionStates ToActionEnum(string stateName)
        {
            if (System.Enum.TryParse<ActionStates>(stateName, out var result))
            {
                return result;
            }
            return ActionStates.NoActionState;
        }
        #endregion
    }
}
