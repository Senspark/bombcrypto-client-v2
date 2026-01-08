using UnityEngine;

namespace Engine.Input {
    public class KeyboardDetection {

        private readonly string _enter;
        private readonly string _back;
        private readonly string _option;

        public KeyboardDetection(InputConfigData inputConfig) {
            _enter = inputConfig.Enter;
            _back = inputConfig.Back;
            _option = inputConfig.Settings;
        }
        
        public bool ReadButton(string buttonName) {
            var keyCode = ConvertToKeyCode(buttonName);
            // Đang full screen mode thỉ phải thoát full screen trước rồi lần sau bấm Esc mới có tác dụng
            if(keyCode == KeyCode.Escape) {
                if (Screen.fullScreen) {
                    return false;
                }
            }
            return UnityEngine.Input.GetKeyDown(keyCode);
        }
        
        
        private KeyCode ConvertToKeyCode(string key) {
            if (key == _enter) {
                return KeyCode.Space;
            }
            if (key == _back) {
                return KeyCode.Escape;
            }
            if (key == _option) {
                return KeyCode.Escape;
            }
            return KeyCode.None;
        }
    }
}