using FunkyCode.Utilities;
using QFramework;
using UnityEngine;

namespace SkateGame
{
    public interface IPlayerSystem : ISystem
    {
        void FixedUpdate();
    }

    public class PlayerSystem : AbstractSystem, IPlayerSystem, ICanSendCommand
    {
        private PlayerController playerController;
        private IPlayerModel playerModel;
        private Rigidbody2D rb;
        private float cachedMoveInput;
        private bool jumpQueued;
        private bool pushing;
        private bool powerGrinding;
        private bool grinding;
        private bool trickingB;
        private float trickBdirection;
        private bool trickingC;
        private bool TrickBResetSpeedQueued;
        private bool TrickCResetSpeedQueued;
        private bool trickARewardQueued;
        private bool trickBRewarding;
        private bool trickCRewardQueued;
        protected override void OnInit()
        {
            // 获取玩家控制器
            UpdatePlayerController();
            
            // 获取玩家参数
            playerModel = this.GetModel<IPlayerModel>();
            
            // 监听输入事件
            this.RegisterEvent<MoveInputEvent>(OnMoveInput);
            this.RegisterEvent<JumpExecuteEvent>(OnJumpInput);
            this.RegisterEvent<TrickARewardEvent>(OnTrickAReward);
            this.RegisterEvent<TrickBRewardEvent>(OnTrickBReward);
            this.RegisterEvent<TrickCRewardEvent>(OnTrickCReward);
            this.RegisterEvent<PushInputEvent>(OnPushInput);
            this.RegisterEvent<GrindInputEvent>(OnGrindInput);
            this.RegisterEvent<PowerGrindInputEvent>(OnPowerGrindInput);
            this.RegisterEvent<TrickAInputEvent>(OnTrickAInput);
            this.RegisterEvent<TrickBInputEvent>(OnTrickBInput);
            this.RegisterEvent<TrickCInputEvent>(OnTrickCInput);
            this.RegisterEvent<TrickBResetSpeedEvent>(OnTrickBResetSpeed);
            this.RegisterEvent<TrickCResetSpeedEvent>(OnTrickCResetSpeed);
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
            rb = playerController.GetComponent<Rigidbody2D>();
            if (playerController != null)
            {
                Debug.Log("PlayerSystem: 找到 PlayerController");
            }
            else
            {
                Debug.LogWarning("PlayerSystem: 场景中没有找到 PlayerController");
            }
        }

        public void FixedUpdate()
        {
            ApplyMovement();
            ApplyRotation();
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
        private void OnTrickAReward(TrickARewardEvent evt)
        {
            trickARewardQueued = true;
        }
        private void OnTrickBReward(TrickBRewardEvent evt)
        {
            trickBRewarding = evt.IsTrickBRewarding;
        }
        private void OnTrickCReward(TrickCRewardEvent evt)
        {
            trickCRewardQueued = true;
        }
        private void OnTrickBResetSpeed(TrickBResetSpeedEvent evt)
        {
            TrickBResetSpeedQueued = true;
        }
        private void OnTrickCResetSpeed(TrickCResetSpeedEvent evt)
        {
            TrickCResetSpeedQueued = true;
        }
        #endregion

        #region Method
        public void ApplyMovement()
        {
            bool isGrounded = playerModel.IsGrounded.Value;
    
            rb.linearDamping = isGrounded ? playerModel.Config.Value.groundLinearDamping : playerModel.Config.Value.airLinearDamping;
            if (isGrounded)
            { 
                ApplySlopeCompensation();
                // ApplyGroundForce(); 
            }

            ApplyHorizontalSpeed(cachedMoveInput, isGrounded);

            if (jumpQueued){ ApplyJumpImpulse(); jumpQueued = false; }
            if (trickARewardQueued){ ApplyTrickAReward(); trickARewardQueued = false; }
            if (trickCRewardQueued){ ApplyTrickCReward(); trickCRewardQueued = false; }
            if (pushing){ ApplyPushSpeed();}
            if (powerGrinding){ ApplyPowerGrind();}
            if (grinding){ ApplyGrind();}
            if (trickingB){ ApplyTrickB(trickBdirection);}
            if (trickingC){ ApplyTrickC();}
            if (TrickBResetSpeedQueued){ ResetSpeedAfterTrickB(); TrickBResetSpeedQueued = false; }
            if (TrickCResetSpeedQueued){ ResetSpeedAfterTrickC(); TrickCResetSpeedQueued = false; }
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

        private void ApplyHorizontalSpeed(float horizontalInput, bool isGrounded)
        {
            float accel = isGrounded ? playerModel.Config.Value.groundAccel : playerModel.Config.Value.airAccel;

            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                // 如果当前速度和输入方向相反，且速度大于0.1f，则减速
                if (Mathf.Sign(vRight) != Mathf.Sign(horizontalInput) && Mathf.Abs(vRight) > 0.1f)
                {
                    float turnDecel = isGrounded ? playerModel.Config.Value.turnDecel : playerModel.Config.Value.airTurnDecel;
                    turnDecel *= Mathf.Pow(Mathf.Abs(vRight), playerModel.Config.Value.stopDecelIncrement);
                    rb.linearVelocity -= right * Mathf.Sign(vRight) * turnDecel * Time.fixedDeltaTime;
                    if (Mathf.Sign(vRight) == Mathf.Sign(horizontalInput))
                    {
                        rb.linearVelocity = vUp*up;
                    }
                }
                // 如果当前速度和输入方向相同，且速度小于最大速度，则加速
                else if (Mathf.Abs(vRight) < playerModel.Config.Value.maxMoveSpeed)
                {
                    rb.linearVelocity += right * (horizontalInput * accel * Time.fixedDeltaTime);
                    float newVRight = Mathf.Clamp(vRight, -playerModel.Config.Value.maxMoveSpeed, playerModel.Config.Value.maxMoveSpeed);
                    rb.linearVelocity = newVRight*right + vUp*up;
                }
            }
        }

        private void ApplyJumpImpulse()
        {
			// Override up direction, force up when not grounded
			Vector2 up = playerModel.IsGrounded.Value ? 
                (Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.up).normalized : Vector2.up;
			float vUp = Vector2.Dot(rb.linearVelocity, up);
			rb.linearVelocity -= vUp * up;
            rb.AddForce(up * playerModel.Config.Value.maxJumpForce * rb.mass, ForceMode2D.Impulse);
        }

        private void ApplyPushSpeed()
        {
            float dir = playerModel.IsFacingRight.Value ? 1f : -1f;
            float pushAccel = playerModel.Config.Value.pushAccel;
            if (Mathf.Abs(vRight) < playerModel.Config.Value.maxMoveSpeed)
            {
                rb.linearVelocity += right * (dir * pushAccel * Time.fixedDeltaTime);
                float newVRight = Mathf.Clamp(vRight, -playerModel.Config.Value.maxMoveSpeed, playerModel.Config.Value.maxMoveSpeed);
                rb.linearVelocity = newVRight*right + vUp*up;
            }
        }

        private void ApplyPowerGrind()
        {   
            Vector2 right = (Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.right).normalized;
            float vRight = Vector2.Dot(rb.linearVelocity, right);
            
            float deceleration = playerModel.Config.Value.powerGrindDeceleration;
            float direction = Mathf.Sign(vRight);
            // 逐渐减少的速度，保持方向不变
            float newVx = vRight - direction * deceleration * Time.fixedDeltaTime;

            // 防止越过零点
            if (Mathf.Sign(newVx) != direction || Mathf.Abs(newVx) < 0.01f)
            {
                newVx = 0f;
            }

            rb.linearVelocity = new Vector2(newVx, rb.linearVelocity.y);
        }

        private void ApplyGrind()
        {
        }
        
        private void ApplyTrickB(float direction)
        {
            float speed = Mathf.Max(playerModel.Config.Value.TrickBspeed, playerModel.VelocityBeforeTrick.Value * direction);
            rb.linearVelocity = new Vector2(direction * speed, 0);
        }
        private void ResetSpeedAfterTrickB()
        {
            float speed = Mathf.Max(playerModel.Config.Value.TrickBspeed, playerModel.VelocityBeforeTrick.Value * trickBdirection);
            rb.linearVelocity = new Vector2(speed * trickBdirection * playerModel.Config.Value.TrickBinertia, 0);
        }
        private void ApplyTrickC()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -playerModel.Config.Value.TrickCspeed);
        }
        private void ResetSpeedAfterTrickC()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, playerModel.Config.Value.TrickCspeed * playerModel.Config.Value.TrickCinertia);
        }

        private void ApplyTrickAReward()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * playerModel.Config.Value.maxJumpForce * rb.mass, ForceMode2D.Impulse);
        }

        private void ApplyTrickCReward()
        {
        }

        // 将玩家稍微吸向地面
        private void ApplyGroundForce()
        {
            Vector2 down = (Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.down).normalized;
            rb.AddForce(down * (playerModel.Config.Value.groundForce * rb.mass), ForceMode2D.Force);
        }

        // 如果坡度发生变化，补偿损失的速度
        private void ApplySlopeCompensation()
        {
			Vector2 g = Physics2D.gravity;
			Vector2 gTangent = Vector2.Dot(g, right) * right * Mathf.Sign(vRight);
			rb.linearVelocity += -playerModel.Config.Value.slopeCompensationForce * gTangent * Time.fixedDeltaTime;
        }

        #endregion

        #region Helper

        // Direction
        private Vector2 up => Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.up;
        private Vector2 right => Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.right;
        private float vUp => Vector2.Dot(rb.linearVelocity, up);
        private float vRight => Vector2.Dot(rb.linearVelocity, right);


        // Update animator on state changed
        private void UpdateAnimatorOnStateChanged(StateChangedEvent evt)
        {
            var anim = playerController.animator;
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
                return MovementStates.Idle;
        }

        private ActionStates ToActionEnum(string stateName)
        {
            if (System.Enum.TryParse<ActionStates>(stateName, out var result))
            {
                return result;
            }
            return ActionStates.None;
        }
        #endregion
    }
}
