using QFramework;
using UnityEngine;

namespace SkateGame
{
    public interface IPlayerSystem : ISystem
    {
    }

    public class PlayerSystem : AbstractSystem, IPlayerSystem, ICanSendCommand
    {
        private PlayerController playerController;
        private IPlayerModel playerModel;
        
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

        // 输入事件处理
        
        #region Event
        private void OnMoveInput(MoveInputEvent evt)
        {
            ApplyHorizontalMovement(evt.HorizontalInput, playerModel.IsGrounded.Value);
        }
         private void OnJumpInput(JumpExecuteEvent evt)
        {
            ApplyJumpMovement();
        }
        private void OnStateChanged(StateChangedEvent evt)
        {
            ApplyStateChanged(evt);
            UpdateAnimatorOnStateChanged(evt);
        }
        private void OnPushInput(PushInputEvent evt)
        {
            ApplyPushMovement();
        }
        #endregion

        #region Method
        public void ApplyHorizontalMovement(float horizontalInput, bool isGrounded)
        {
            // 空值检查，防止场景切换时访问已销毁的对象
            if (playerController == null) return;
            
            Rigidbody2D rb = playerController.GetRigidbody();
            if (rb == null) return;

            float currentSpeed = rb.linearVelocity.x;
            float targetSpeed = horizontalInput * (isGrounded ? playerModel.Config.Value.maxMoveSpeed : playerModel.Config.Value.maxAirHorizontalSpeed);
            float newSpeed = currentSpeed;

            // ------------------------
            // 1. 有输入时
            // ------------------------
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                // (1) 转向时，先减速 → 不能立刻掉头
                if (Mathf.Sign(currentSpeed) != Mathf.Sign(horizontalInput) && Mathf.Abs(currentSpeed) > 0.1f)
                {
                    newSpeed = Mathf.MoveTowards(currentSpeed, 0, playerModel.Config.Value.turnDecel * Time.deltaTime);
                }
                else
                {
                    // (2) 同方向，加速逼近目标速度
                    newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, 
                    (isGrounded ? playerModel.Config.Value.groundAccel : playerModel.Config.Value.airAccel) * Time.deltaTime);
                }
            }
            // ------------------------
            // 2. 没有输入时 → 滑行
            // --------------------
            else
            {
                float decel = isGrounded ? playerModel.Config.Value.groundDecel : playerModel.Config.Value.airDecel;
                newSpeed = Mathf.MoveTowards(currentSpeed, 0, decel * Time.deltaTime);
            }

            rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
        }

        public void ApplyJumpMovement()
        {
            // 空值检查，防止场景切换时访问已销毁的对象
            if (playerController == null) return;
            
            var rb = playerController.GetRigidbody();
            if (rb == null) return;
            
            float jumpForce = playerModel.Config.Value.maxJumpForce;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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

        public void ApplyPushMovement()
        {
            Rigidbody2D rb = playerController.GetRigidbody();
            if (rb == null) return;
            float currentSpeed = rb.linearVelocity.x;
            float targetSpeed = playerModel.Config.Value.maxMoveSpeed * (playerModel.IsFacingRight.Value ? 1 : -1);
            
            float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, 
                    playerModel.Config.Value.pushAccel * Time.deltaTime);
            rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
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
