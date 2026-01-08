using Engine.UI;

using UnityEngine;

namespace BomberLand.DirectionInput {
    public class JoyStickDirectionInput : IDirectionInput {
        private IJoystick _joystick;
        
        public bool Enabled { get; set; }


        public JoyStickDirectionInput(IJoystick joystick) {
            _joystick = joystick;
        }
        
        public Vector2 GetDirection() {
            return _joystick.Direction;
        }
    }
}