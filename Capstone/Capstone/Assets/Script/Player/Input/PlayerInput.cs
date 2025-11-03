using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;
using QFramework;
using System.Linq;

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
        InputAction _trickAction;
        InputAction _pushAction;
        InputAction _shootStartAction;
        InputAction _shootEndAction;
        InputAction _aimDirectionAction;
        public IArchitecture GetArchitecture() => GameApp.Interface;
        
        void Awake()
        {
            var _actions = GetComponent<PlayerInput>().actions;
            _moveAction = _actions.FindAction("Player/Move");
            _jumpAction = _actions.FindAction("Player/Jump");
            _grindAction = _actions.FindAction("Player/Grind");
            _switchItemAction = _actions.FindAction("Player/SwitchItem");
            _trickAction = _actions.FindAction("Player/Trick");
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

        public FrameInput Gather()
        {
            return new FrameInput
            {
                Move = _moveAction.ReadValue<Vector2>(),
                JumpStart = _jumpAction.WasPressedThisFrame(),
                Grind = _grindAction.IsPressed(),
                SwitchItem = _switchItemAction.WasPressedThisFrame(),
                Trick = _trickAction.IsPressed(),
                TrickStart= _trickAction.WasPressedThisFrame(),
                Push = _pushAction.WasPressedThisFrame(),
                ShootStart = _shootStartAction.WasPressedThisFrame(),
                ShootEnd = _shootEndAction.WasReleasedThisFrame(),
                AimDirection = _aimDirectionAction.ReadValue<Vector2>(),
            };
        }

        void Update()
        {
            FrameInput frameInput = Gather();
            var inputModel = this.GetModel<IInputModel>();
            inputModel.Move.Value = frameInput.Move;
            inputModel.JumpStart.Value = frameInput.JumpStart;
            inputModel.Grind.Value = frameInput.Grind;
            inputModel.SwitchItem.Value = frameInput.SwitchItem;
            inputModel.Trick.Value = frameInput.Trick;
            inputModel.TrickStart.Value = frameInput.TrickStart;
            inputModel.Push.Value = frameInput.Push;
            inputModel.ShootStart.Value = frameInput.ShootStart;
            inputModel.ShootEnd.Value = frameInput.ShootEnd;
			inputModel.AimDirection.Value = GetAimDirection();
        }

		private Vector2 GetAimDirection()
		{
            if (_currentDevice is Gamepad)
            {
                return Gather().AimDirection;
            }
            else if (_currentDevice is Mouse)
            {
                // 获取鼠标屏幕位置
                Vector3 mouseScreenPos = Gather().AimDirection;
                
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
                return Gather().AimDirection;
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

        public struct FrameInput
        {
            public Vector2 Move;
            public bool JumpStart;
            public bool Grind;
            public bool SwitchItem;
            public bool Trick;
            public bool TrickStart;
            public bool Push;
            public bool ShootStart;
            public bool ShootEnd;
            public Vector2 AimDirection;
        }

    }
}