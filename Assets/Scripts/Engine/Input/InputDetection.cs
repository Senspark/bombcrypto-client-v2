using System;
using System.Collections.Generic;
using Engine.Manager;

namespace Engine.Input {

    public static class ControllerButtonName {
        public static readonly string RB = InputEvent.RB.ToString();
        public static readonly string RT = InputEvent.RT.ToString();

        public static readonly string LB = InputEvent.LB.ToString();
        public static readonly string LT = InputEvent.LT.ToString();

        
        public static readonly string A = InputEvent.A.ToString();
        public static readonly string B = InputEvent.B.ToString();
        public static readonly string X = InputEvent.X.ToString();
        public static readonly string Y = InputEvent.Y.ToString();

        public static readonly string Share = InputEvent.Share.ToString();
        public static readonly string Options = InputEvent.Options.ToString();
        public static readonly string DPadDown = InputEvent.DPadDown.ToString();
        public static readonly string DPadUp = InputEvent.DPadUp.ToString();
        public static readonly string DPadLeft = InputEvent.DPadLeft.ToString();
        public static readonly string DPadRight = InputEvent.DPadRight.ToString();
        public static readonly string LStickButton = InputEvent.LStickButton.ToString();
        public static readonly string RStickButton = InputEvent.RStickButton.ToString();
        
        public static readonly string LStickLeft = InputEvent.LStickLeft.ToString();
        public static readonly string LStickRight = InputEvent.LStickRight.ToString();
        public static readonly string LStickUp = InputEvent.LStickUp.ToString();
        public static readonly string LStickDown = InputEvent.LStickDown.ToString();
    }
    
    public static class SpecialButtonName {
        //Do bug unity khi build webgl ko nhận R2 L2 là button mà nhận là asix nên
        //phải tạo thêm 2 biến này để fix tạm bug, 2 biến này có kiểu là asix
        //nhưng sẽ đc dùng như button
        public static readonly string RT = InputEvent.RT.ToString();
        public static readonly string LT = InputEvent.LT.ToString();
    }
    public static class ControllerAxisName {

        public static readonly string LeftStickX = InputEvent.LeftStickX.ToString();
        public static readonly string LeftStickY = InputEvent.LeftStickY.ToString();
        public static readonly string DPadX = InputEvent.DPadX.ToString();
        public static readonly string DPadY = InputEvent.DPadY.ToString();

    }
    public class InputDetection {
        private readonly List<string> _buttonList = new ();
        private readonly List<string> _axisList = new ();
        //asix nhưng dùng như button do bug unity
        private readonly List<string> _specialList = new () {
            ControllerButtonName.RT,
            ControllerButtonName.LT
        };

        //Các asix đc xem như button (như left joystick)
        private readonly List<string> _axisAsButtonList = new() {
            ControllerButtonName.LStickLeft,
            ControllerButtonName.LStickRight,
            ControllerButtonName.LStickUp,
            ControllerButtonName.LStickDown
        };


        private readonly Dictionary<string, bool> _axisHoldList2 = new();
        private readonly Dictionary<string, bool> _buttonHoldList = new();
        private readonly Dictionary<string, bool> _axisHoldList = new() {
            {ControllerButtonName.RT, false},
            {ControllerButtonName.LT, false},
            {ControllerButtonName.LStickLeft, false},
            {ControllerButtonName.LStickRight, false},
            {ControllerButtonName.LStickUp, false},
            {ControllerButtonName.LStickDown, false}
        };
        private InputEvent _eventKey;

        public InputDetection() {
            var buttonFields = typeof(ControllerButtonName)
                .GetFields(System.Reflection.BindingFlags.Public |
                           System.Reflection.BindingFlags.Static |
                           System.Reflection.BindingFlags.FlattenHierarchy);
            
            foreach (var button in buttonFields) {
                // Ensure the field is of type string
                if (button.FieldType == typeof(string)) {
                    _buttonList.Add((string)button.GetValue(null));
                    _buttonHoldList.Add((string)button.GetValue(null), false);
                }
            }
            
            var axisFields = typeof(ControllerAxisName)
                .GetFields(System.Reflection.BindingFlags.Public |
                           System.Reflection.BindingFlags.Static |
                           System.Reflection.BindingFlags.FlattenHierarchy);
            
            foreach (var axis in axisFields) {
                // Ensure the field is of type string
                if (axis.FieldType == typeof(string)) {
                    _axisList.Add((string)axis.GetValue(null));
                    _axisHoldList2.Add((string)axis.GetValue(null), false);
                }
            }
        }
        
        public void Process() {
            //Tạm thời chưa sử dụng
            // ReadButtonValue();
            // ReadAxisValue();
            // ReadSpecialValue();
        }
        
        /// <summary>
        /// Đọc trạng thái nút nhấn, nếu dùng L2, R2 cho nhiều nút khác nhau thì mỗi nút phải có 1 special key riêng
        /// Vì R2, L2 là axis (webgl chỉ nhận 2 nút này là asix) mà cần dùng như button nên là special 
        /// </summary>
        /// <param name="buttonName">name đc lấy trong InputConfigData</param>
        /// thì phải thêm special key vào để phân biệt các nút khác tính năng nhưng map cùng 1 button L2 hoặc R2</param>
        /// <returns></returns>
        public bool ReadButton(string buttonName) {
            //Kiểm tra có phải LStick ko
            if (_axisAsButtonList.Contains(buttonName)) {
                return ReadAxisAsButton(buttonName);
            }
            
            //Kiểm tra xem có phải là nút L2 R2 ko
            if (_specialList.Contains(buttonName)) {
                //Nút này đang đc nhấn
                if (UnityEngine.Input.GetAxis(buttonName) > 0.1f ) {
                    if (_axisHoldList[buttonName])
                        return false;
                    
                    _axisHoldList[buttonName] = true;
                    return true;
                }
                
                //Nút này đã thả ra
                if(_axisHoldList[buttonName]){
                    _axisHoldList[buttonName] = false;
                    return false;
                }
                
            } 
            //Đây là nút nhấn bình thường
            return UnityEngine.Input.GetButtonDown(buttonName);

        }

        readonly Dictionary<string, Func<float, bool>> _axisConditions = new() {
            { ControllerButtonName.LStickLeft, value => value < -0.2f },
            { ControllerButtonName.LStickRight, value => value > 0.2f },
            { ControllerButtonName.LStickDown, value => value < -0.2f },
            { ControllerButtonName.LStickUp, value => value > 0.2f }
        };

        readonly Dictionary<string, string> _axisMappings = new() {
            { ControllerButtonName.LStickLeft, ControllerAxisName.LeftStickX },
            { ControllerButtonName.LStickRight, ControllerAxisName.LeftStickX },
            { ControllerButtonName.LStickDown, ControllerAxisName.LeftStickY },
            { ControllerButtonName.LStickUp, ControllerAxisName.LeftStickY }
        };
        
        /// <summary>
        /// Kiểm tra xem user di chuyển left joystick lên, xuống, trái, phải để đọc như button
        /// </summary>
        /// <param name="buttonName"></param>
        /// <returns></returns>
        private bool ReadAxisAsButton(string buttonName) {
            if (_axisConditions.ContainsKey(buttonName) && _axisMappings.TryGetValue(buttonName, out var mapping)) {
                var axisValue = UnityEngine.Input.GetAxis(mapping);
                if (_axisConditions[buttonName](axisValue)) {
                    if (_axisHoldList[buttonName])
                        return false;

                    _axisHoldList[buttonName] = true;
                    return true;
                }

                if (_axisHoldList[buttonName]) {
                    _axisHoldList[buttonName] = false;
                    return false;
                }
            }

            return false;
        }
        
#region Tạm chưa dùng
        
        private void ReadButtonValue() {
            _eventKey = InputEvent.Unknown;
            
            foreach (var button in _buttonList) {
                GetButtonDown(button);
                GetButtonUp(button);
            }
        }
        private void GetButtonDown(string button) {
            if (!UnityEngine.Input.GetButtonDown(button)) 
                return;
                
            if(_buttonHoldList[button])
                return;
                
            if (Enum.TryParse(button,out _eventKey)) {
                EventManager<bool>.Dispatcher(_eventKey, true);
                _buttonHoldList[button] = true;
            }
        }
        private void GetButtonUp(string button) {
            if (!UnityEngine.Input.GetButtonUp(button)) 
                return;
                
            if(!_buttonHoldList[button])
                return;
                
            if (Enum.TryParse(button,out _eventKey)) {
                EventManager<bool>.Dispatcher(_eventKey, false);
                _buttonHoldList[button] = false;
            }
        }
        private void ReadAxisValue() {
            _eventKey = InputEvent.Unknown;
            
            foreach (var axis in _axisList) {
                var value = UnityEngine.Input.GetAxis(axis);
                if (value != 0) {
                    if (_axisHoldList2[axis])
                        continue;
                    
                    if (Enum.TryParse(axis,out _eventKey)) {
                        EventManager<float>.Dispatcher(_eventKey, value);
                        _axisHoldList2[axis] = true;
                    }
                } 
                else {
                    if (_axisHoldList2[axis] && Enum.TryParse(axis,out _eventKey)) {
                        EventManager<float>.Dispatcher(_eventKey, value);
                        _axisHoldList2[axis] = false;
                    }
                }
            }
        }
        private void ReadSpecialValue() {
            _eventKey = InputEvent.Unknown;
            
            foreach (var axis in _specialList) {
                var value = UnityEngine.Input.GetAxis(axis);
                if (value > 0) {
                    if (_axisHoldList[axis])
                        continue;
                    
                    if (Enum.TryParse(axis,out _eventKey)) {
                        EventManager<bool>.Dispatcher(_eventKey, true);
                        _axisHoldList[axis] = true;
                    }
                }
                else {
                    if (_axisHoldList[axis] && Enum.TryParse(axis,out _eventKey)) {
                        EventManager<bool>.Dispatcher(_eventKey, false);
                        _axisHoldList[axis] = false;
                    }
                }
            }
        }
    
        
        #endregion
        
    }
}
