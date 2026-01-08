using Engine.Manager;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using Debug = UnityEngine.Debug;

namespace Engine.Input {
    public class InputCheckChange {
        private InputType _inputType;
        public ControllerType ControllerType;
        private readonly ControllerType _defaultController = ControllerType.Xbox;
        public InputType InputType => _inputType;

        public InputCheckChange() {
            _inputType = InputType.Keyboard;

            ControllerType = GetControllerType();

            InputSystem.onDeviceChange +=
                (_, change) => {
                    switch (change) {
                        case InputDeviceChange.Added:
                            CheckController(true);
                            break;
                        case InputDeviceChange.Disconnected:
                            CheckController(false);
                            break;
                    }
                };
        }

        private ControllerType GetControllerType() {
            var gamepad = Gamepad.current;
            if(gamepad != null) {
                if (gamepad.name != null) {
                    Debug.Log("Controller name " + gamepad.name);
                }
                if (gamepad.displayName != null) {
                    Debug.Log("Controller displayName " + gamepad.displayName);
                }
            }

            var controllerType = gamepad switch {
                //Xbox controller
                XInputController => ControllerType.Xbox,
                //PlayStation controller
                DualShockGamepad => ControllerType.PlayStation,

                _ => ControllerType.Unknown
            };
            if (controllerType != ControllerType.Unknown) {
                return controllerType;
            }
            
            if (gamepad == null || gamepad.displayName == null) {
                return _defaultController;
            }

            if (gamepad.displayName.Contains("DualSense") || 
                gamepad.displayName.Contains("PlayStation") || 
                gamepad.displayName.Contains("DualShock")) {
                return ControllerType.PlayStation;
            }

            if (gamepad.displayName.Contains("Xbox")) {
                return ControllerType.Xbox;
            }

            return _defaultController;
            
            // return gamepad.name switch {
            //     //PlayStation controller
            //     "DualSenseGamepadHID" => ControllerType.PlayStation,
            //     "DualSenseGampadiOS" => ControllerType.PlayStation,
            //     "DualShock3GamepadHID" => ControllerType.PlayStation,
            //     "DualShock4GamepadAndroid" => ControllerType.PlayStation,
            //     "DualShock4GamepadHID" => ControllerType.PlayStation,
            //     "DualShock4GampadiOS" => ControllerType.PlayStation,
            //
            //     //Xbox controller
            //     "XboxGamepadMacOS" => ControllerType.Xbox,
            //     "XboxOneGamepadAndroid" => ControllerType.Xbox,
            //     "XboxOneGampadMacOSWireless" => ControllerType.Xbox,
            //     "XboxOneGampadiOS" => ControllerType.Xbox,
            //
            //     _ => ControllerType.Xbox
            // };
        }

        /// <summary>
        /// Kiểm tra xem nếu đang có sử dụng controller thì nó là loại nào để load UI đúng
        /// </summary>
        /// <param name="connected"></param>
        private void CheckController(bool connected) {
            if (!connected)
                return;

            ControllerType = GetControllerType();
            EventManager<ControllerType>.Dispatcher(InputEvent.OnchangeController, ControllerType);
        }

        /// <summary>
        /// Kiểm tra xem trạng thái gần nhất user có thao tác trên controller ko để hiện UI đúng
        /// </summary>
        private void CheckUseController() {
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (_inputType == InputType.Controller)
                return;

            _inputType = InputType.Controller;
            EventManager<InputType>.Dispatcher(InputEvent.OnChangeInput, _inputType);
        }

        /// <summary>
        /// Kiểm tra xem trạng thái gần nhất user có thao tác trên bàn phím hay ko để hiện UI đúng
        /// </summary>
        private void CheckUseKeyboard() {
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (_inputType == InputType.Keyboard)
                return;

            _inputType = InputType.Keyboard;
            EventManager<InputType>.Dispatcher(InputEvent.OnChangeInput, _inputType);
        }

        private static bool IsAnyKeyPress() {
            return UnityEngine.Input.anyKeyDown ||
                   UnityEngine.Input.GetAxis(ControllerAxisName.LeftStickX) != 0 ||
                   UnityEngine.Input.GetAxis(ControllerAxisName.LeftStickY) != 0 ||
                   //special value
                   UnityEngine.Input.GetAxis(ControllerButtonName.RT) > 0 ||
                   UnityEngine.Input.GetAxis(ControllerButtonName.LT) > 0;
        }

        /// <summary>
        /// Kiểm tra xem phím nhấn gần nhất có đc nhấn bởi controller hay ko
        /// </summary>
        /// <returns></returns>
        private bool IsUseController() {
            return //button value
                UnityEngine.Input.GetButtonDown(ControllerButtonName.RB) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.LB) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.A) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.B) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.X) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.Y) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.Share) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.Options) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.LStickButton) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.RStickButton) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.DPadDown) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.DPadUp) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.DPadLeft) ||
                UnityEngine.Input.GetButtonDown(ControllerButtonName.DPadRight) ||
                //Axis value
                UnityEngine.Input.GetAxis(ControllerAxisName.LeftStickX) != 0 ||
                UnityEngine.Input.GetAxis(ControllerAxisName.LeftStickY) != 0 ||
                UnityEngine.Input.GetAxis(ControllerButtonName.RT) > 0.1f ||
                UnityEngine.Input.GetAxis(ControllerButtonName.LT) > 0.1f;
        }

        public void Process() {
            if (!IsAnyKeyPress()) {
                return;
            }

            if (IsUseController()) {
                CheckUseController();
            } else {
                CheckUseKeyboard();
            }
        }
    }
}