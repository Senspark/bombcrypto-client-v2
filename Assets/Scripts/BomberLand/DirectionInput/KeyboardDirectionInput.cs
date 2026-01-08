using App;

using Engine.Input;

using Senspark;

using UnityEngine;

namespace BomberLand.DirectionInput {
    public class KeyboardDirectionInput : IDirectionInput {
        public bool Enabled { get; set; }
        private readonly IInputManager _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
        private readonly HotkeyCombo _hotkeyCombo = ServiceLocator.Instance.Resolve<IHotkeyControlManager>().GetHotkeyCombo();
        
        public Vector2 GetDirection() {
            var direction = Vector2.zero;

            var leftArrow = false;
            var rightArrow = false;
            var upArrow = false;
            var downArrow = false;

            // leftArrow = Input.GetKey(KeyCode.LeftArrow);
            // rightArrow = Input.GetKey(KeyCode.RightArrow);
            // upArrow = Input.GetKey(KeyCode.UpArrow);
            // downArrow = Input.GetKey(KeyCode.DownArrow);
            
 
            leftArrow = Input.GetKey(_hotkeyCombo.GetControl(ControlKey.MoveLeft));
            rightArrow = Input.GetKey(_hotkeyCombo.GetControl(ControlKey.MoveRight));
            upArrow = Input.GetKey(_hotkeyCombo.GetControl(ControlKey.MoveUp));
            downArrow = Input.GetKey(_hotkeyCombo.GetControl(ControlKey.MoveDown));
            
            float xAxisL = _inputManager.ReadAxis(ControllerAxisName.LeftStickX);
            float yAxisL = _inputManager.ReadAxis(ControllerAxisName.LeftStickY);

            if (leftArrow || xAxisL < -0.5f) {
                direction = Vector2.zero;
                direction.x = -1;
            }

            if (rightArrow || xAxisL > 0.5f) {
                direction = Vector2.zero;
                direction.x = 1;
            }

            if (downArrow || yAxisL < -0.5f) {
                direction = Vector2.zero;
                direction.y = -1;
            }

            if (upArrow|| yAxisL > 0.5f) {
                direction = Vector2.zero;
                direction.y = 1;
            }

            return direction;
        }
    }
}