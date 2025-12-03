using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using QFramework;

namespace SkateGame
{
    public class PlayerInputs : MonoBehaviour, ICanGetModel, IBelongToArchitecture
    {
        public PlayerInput _playerInput;
        InputDevice _currentDevice;
        InputAction _moveAction;
        InputAction _jumpAction;
        InputAction _grindAction;
        InputAction _switchItemAction;
        InputAction _trickAAction;
        InputAction _trickBAction;
        InputAction _trickCAction;
        InputAction _pushAction;
        InputAction _shootStartAction;
        InputAction _shootEndAction;
        InputAction _aimDirectionAction;
        private bool _isShootLocked = false;
        public IArchitecture GetArchitecture() => GameApp.Interface;
        
        void Awake()
        {
            var _actions = GetComponent<PlayerInput>().actions;
            _moveAction = _actions.FindAction("Player/Move");
            _jumpAction = _actions.FindAction("Player/Jump");
            _grindAction = _actions.FindAction("Player/Grind");
            _switchItemAction = _actions.FindAction("Player/SwitchItem");
            _trickAAction = _actions.FindAction("Player/TrickA");
            _trickBAction = _actions.FindAction("Player/TrickB");
            _trickCAction = _actions.FindAction("Player/TrickC");
            _pushAction = _actions.FindAction("Player/Push");
            _shootStartAction = _actions.FindAction("Player/Shoot");
            _shootEndAction = _actions.FindAction("Player/Shoot");
            _aimDirectionAction = _actions.FindAction("Player/AimDirection");
            
        }

    
		private void OnEnable()
		{
			InputSystem.onEvent += OnInputEvent;
		}

		private void OnDisable()
		{
			InputSystem.onEvent -= OnInputEvent;
		} 
        void Update()
        {
            var inputModel = this.GetModel<IInputModel>();
            inputModel.Move.Value = _moveAction.ReadValue<Vector2>();
            inputModel.JumpStart.Value = _jumpAction.WasPressedThisFrame();
            inputModel.Grind.Value = _grindAction.IsPressed();
            inputModel.GrindStart.Value = _grindAction.WasPressedThisFrame();
            inputModel.SwitchItem.Value = _switchItemAction.WasPressedThisFrame();
            inputModel.TrickA.Value = _trickAAction.IsPressed();
            inputModel.TrickAStart.Value = _trickAAction.WasPressedThisFrame();
            inputModel.TrickB.Value = _trickBAction.IsPressed();
            inputModel.TrickBStart.Value = _trickBAction.WasPressedThisFrame();
            inputModel.TrickC.Value = _trickCAction.IsPressed();
            inputModel.TrickCStart.Value = _trickCAction.WasPressedThisFrame();
            inputModel.Push.Value = _pushAction.WasPressedThisFrame();

            bool shootStart = _shootStartAction.WasPressedThisFrame();
            bool shootEnd = _shootEndAction.WasReleasedThisFrame();

            if (_isShootLocked)
            {
                shootStart = false;
                shootEnd = false;
            }

            inputModel.ShootStart.Value = shootStart;
            inputModel.ShootEnd.Value = shootEnd;
            Vector2 aimDirection = _aimDirectionAction.ReadValue<Vector2>();
			inputModel.AimDirection.Value = GetAimDirection(aimDirection);
        }

		private Vector2 GetAimDirection(Vector2 aimDirection)
		{
            if (_currentDevice is Gamepad)
            {
                return aimDirection;
            }
            else if (_currentDevice is Mouse)
            {
                // 获取鼠标屏幕位置
                Vector3 mouseScreenPos = aimDirection;
                
                // 根据相机投影模式设置正确的z值
                if (Camera.main.orthographic)
                {
                    // 正交模式下，z值不重要，可以使用0
                    mouseScreenPos.z = 0;
                }
                else
                {
                    // 透视模式下，需要设置z值为玩家在屏幕空间的深度
                    mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
                }
                
                Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                    return (worldMousePos - (Vector2)transform.position).normalized;
            }
            else if (_currentDevice is Keyboard)
            {
                return aimDirection;
            }
            else return Vector2.right;
		}

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (device == null) return;
            
            // switch device
            if (_currentDevice != device)
            {
                _currentDevice = device;
            }
        }

        public void SetShootLock(bool locked)
        {
            _isShootLocked = locked;
            if (locked)
            {
                var inputModel = this.GetModel<IInputModel>();
                inputModel.ShootStart.Value = false;
                inputModel.ShootEnd.Value = false;
            }
        }

    }
}