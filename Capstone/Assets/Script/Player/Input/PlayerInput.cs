using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using QFramework;

namespace SkateGame
{
    public class PlayerInputs : MonoBehaviour, ICanGetModel, IBelongToArchitecture, ICanGetSystem, ICanSendEvent
    {
        public PlayerInput _playerInput;
        private IInputGateSystem gate;
        InputDevice _currentDevice;

        // Player action map
        InputAction _moveAction;
        InputAction _jumpAction;
        InputAction _grindAction;
        InputAction _switchItemAction;
        InputAction _grabAction;
        InputAction _dashAction;
        InputAction _slamAction;
        InputAction _pushAction;
        InputAction _shootStartAction;
        InputAction _shootEndAction;
        InputAction _pauseAction;
        InputAction _brakeAction;

        // UI action map
        InputAction _uiCancelAction;
        private bool _isShootLocked = false;
        public IArchitecture GetArchitecture() => GameApp.Interface;

        bool _lastPlayerEnabled = true;
        bool _lastUiEnabled = false;
        
        void Awake()
        {
            if (_playerInput == null) _playerInput = GetComponent<PlayerInput>();
            var _actions = _playerInput.actions;

            // Player action map
            _moveAction = _actions.FindAction("Player/Move");
            _jumpAction = _actions.FindAction("Player/Jump");
            _grindAction = _actions.FindAction("Player/Grind");
            _switchItemAction = _actions.FindAction("Player/SwitchItem");
            _grabAction = _actions.FindAction("Player/Grab");
            _dashAction = _actions.FindAction("Player/Dash");
            _slamAction = _actions.FindAction("Player/Slam");
            _pushAction = _actions.FindAction("Player/Push");
            _shootStartAction = _actions.FindAction("Player/Shoot");
            _shootEndAction = _actions.FindAction("Player/Shoot");
            _pauseAction = _actions.FindAction("Player/Pause");
            _brakeAction = _actions.FindAction("Player/Brake");
            // UI action map
            _uiCancelAction = _actions.FindAction("UI/Cancel");
        }
        void Start()
        {
            gate = this.GetSystem<IInputGateSystem>();
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

            bool playerEnabled = !gate.PlayerInputBlocked;
            bool uiEnabled = gate.UiInputEnabled;
            if (_playerInput != null && (playerEnabled != _lastPlayerEnabled || uiEnabled != _lastUiEnabled))
            {
                var asset = _playerInput.actions;
                if (asset != null)
                {
                    var playerMap = asset.FindActionMap("Player", true);
                    var uiMap = asset.FindActionMap("UI", true);
                    if (playerMap.enabled != playerEnabled)
                    {
                        if (playerEnabled) playerMap.Enable(); else playerMap.Disable();
                    }
                    if (uiMap.enabled != uiEnabled)
                    {
                        if (uiEnabled) uiMap.Enable(); else uiMap.Disable();
                    }
                }
                _lastPlayerEnabled = playerEnabled;
                _lastUiEnabled = uiEnabled;
            }

            // toggle pause
            if (_pauseAction.WasPressedThisFrame() || _uiCancelAction.WasPressedThisFrame())
            {
                this.SendEvent<TogglePauseEvent>();
            }
            // read player input
            var inputModel = this.GetModel<IInputModel>();
            inputModel.Move.Value = _moveAction.ReadValue<Vector2>();
            inputModel.JumpStart.Value = _jumpAction.WasPressedThisFrame();
            inputModel.Grind.Value = _grindAction.IsPressed();
            inputModel.GrindStart.Value = _grindAction.WasPressedThisFrame();
            inputModel.SwitchItem.Value = _switchItemAction.WasPressedThisFrame();
            inputModel.Grab.Value = _grabAction.IsPressed();
            inputModel.GrabStart.Value = _grabAction.WasPressedThisFrame();
            inputModel.Dash.Value = _dashAction.IsPressed();
            inputModel.DashStart.Value = _dashAction.WasPressedThisFrame();
            inputModel.Slam.Value = _slamAction.IsPressed();
            inputModel.SlamStart.Value = _slamAction.WasPressedThisFrame();
            inputModel.Push.Value = _pushAction.IsPressed();
            inputModel.PushStart.Value = _pushAction.WasPressedThisFrame();
            inputModel.Brake.Value = _brakeAction.IsPressed();
            inputModel.BrakeStart.Value = _brakeAction.WasPressedThisFrame();
            bool shootStart = _shootStartAction.WasPressedThisFrame();
            bool shootEnd = _shootEndAction.WasReleasedThisFrame();

            if (_isShootLocked)
            {
                shootStart = false;
                shootEnd = false;
            }

            inputModel.ShootStart.Value = shootStart;
            inputModel.ShootEnd.Value = shootEnd;
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