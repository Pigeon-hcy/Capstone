using QFramework;
using UnityEngine;

namespace SkateGame
{
    public interface IPlayerSystem : ISystem
    {
        void FixedUpdate(Rigidbody2D rb);
    }

    public class PlayerSystem : AbstractSystem, IPlayerSystem, ICanSendCommand
    {
        private PlayerController playerController;
        private IPlayerModel playerModel;
        private float cachedMoveInput;
        private bool jumpQueued;
        private bool pushQueued;
        private bool rewardJumpQueued;
        private bool powerGrindQueued;
        private bool grindQueued;
        protected override void OnInit()
        {
            // 获取玩家控制器
            UpdatePlayerController();
            
            // 获取玩家参数
            playerModel = this.GetModel<IPlayerModel>();
            
            // 监听输入事件
            this.RegisterEvent<MoveInputEvent>(OnMoveInput);
            this.RegisterEvent<JumpExecuteEvent>(OnJumpInput);
            this.RegisterEvent<PushInputEvent>(OnPushInput);
            this.RegisterEvent<StateChangedEvent>(OnStateChanged);
            this.RegisterEvent<RewardJumpEvent>(OnRewardJump);
            this.RegisterEvent<GrindInputEvent>(OnGrindInput);
            this.RegisterEvent<PowerGrindInputEvent>(OnPowerGrindInput);
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
            if (playerController != null)
            {
                Debug.Log("PlayerSystem: 找到 PlayerController");
            }
            else
            {
                Debug.LogWarning("PlayerSystem: 场景中没有找到 PlayerController");
            }
        }

        public void FixedUpdate(Rigidbody2D rb)
        {
            bool isGrounded = playerModel.IsGrounded.Value;

            rb.linearDamping = isGrounded ? playerModel.Config.Value.groundLinearDamping : playerModel.Config.Value.airLinearDamping;

            ApplyHorizontalAddForce(rb, cachedMoveInput, isGrounded);

            if (jumpQueued)
            {
                ApplyJumpImpulse(rb);
                jumpQueued = false;
            }

            if (pushQueued)
            {
                ApplyPushForce(rb);
                pushQueued = false;
            }
            if (rewardJumpQueued)
            {
                ApplyRewardJump(rb);
                rewardJumpQueued = false;
            }
            if (powerGrindQueued)
            {
                ApplyPowerGrind(rb);
                powerGrindQueued = false;
            }
            if (grindQueued)
            {
                ApplyGrind(rb);
                grindQueued = false;
            }
            ClampHorizontalSpeed(rb, isGrounded);
        }

        // 输入事件处理
        
        #region Event
        private void OnMoveInput(MoveInputEvent evt)
        {
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
            pushQueued = true;
        }
        private void OnRewardJump(RewardJumpEvent evt)
        {
            rewardJumpQueued = true;
        }
        private void OnPowerGrindInput(PowerGrindInputEvent evt)
        {
            powerGrindQueued = true;
        }
        private void OnGrindInput(GrindInputEvent evt)
        {
            grindQueued = true;
        }
        #endregion

        #region Method
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



        private void ApplyHorizontalAddForce(Rigidbody2D rb, float horizontalInput, bool isGrounded)
        {
            float accel = isGrounded ? playerModel.Config.Value.groundAccel : playerModel.Config.Value.airAccel;

            float currentSpeed = rb.linearVelocity.x;
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                if (Mathf.Sign(currentSpeed) != Mathf.Sign(horizontalInput) && Mathf.Abs(currentSpeed) > 0.1f)
                {
                    float turnDecel = playerModel.Config.Value.turnDecel;
                    rb.AddForce(Vector2.left * Mathf.Sign(currentSpeed) * turnDecel * rb.mass, ForceMode2D.Force);
                }
                rb.AddForce(Vector2.right * (horizontalInput * accel * rb.mass), ForceMode2D.Force);
            }
        }

        private void ApplyJumpImpulse(Rigidbody2D rb)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * playerModel.Config.Value.maxJumpForce * rb.mass, ForceMode2D.Impulse);
        }

        private void ApplyPushForce(Rigidbody2D rb)
        {
            float dir = playerModel.IsFacingRight.Value ? 1f : -1f;
            float pushAccel = playerModel.Config.Value.pushAccel;
            rb.AddForce(Vector2.right * (dir * pushAccel * rb.mass), ForceMode2D.Force);
        }

        private void ApplyRewardJump(Rigidbody2D rb)
        {
            rb.AddForce(Vector2.up * playerModel.Config.Value.maxJumpForce * rb.mass, ForceMode2D.Impulse);
        }

        private void ApplyPowerGrind(Rigidbody2D rb)
        {   
            float vx = rb.linearVelocity.x;
            
            float deceleration = playerModel.Config.Value.powerGrindDeceleration;
            float direction = playerModel.IsFacingRight.Value ? 1f : -1f;
            // 逐渐减少的速度，保持方向不变
            float newVx = vx - direction * deceleration * Time.fixedDeltaTime;

            // 防止越过零点
            if (Mathf.Sign(newVx) != direction || Mathf.Abs(newVx) < 0.01f)
            {
                newVx = 0f;
            }

            rb.linearVelocity = new Vector2(newVx, rb.linearVelocity.y);
        }

        private void ApplyGrind(Rigidbody2D rb)
        {
        }

        private void ClampHorizontalSpeed(Rigidbody2D rb, bool isGrounded)
        {
            float max = isGrounded ? playerModel.Config.Value.maxMoveSpeed : playerModel.Config.Value.maxAirHorizontalSpeed;
            Vector2 v = rb.linearVelocity;
            if (Mathf.Abs(v.x) > max)
            {
                v.x = Mathf.Sign(v.x) * max;
                rb.linearVelocity = v;
            }
        }
        #endregion

        #region Helper
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
