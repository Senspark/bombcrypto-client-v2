using System.Collections.Generic;

using Senspark;

using UnityEngine;

namespace BomberLand.KeyboardInput {
    
    public class KeyInputObserver {
        public System.Action<int> KeyDownOnItem;
    }
    
    public class KeyboardInputListener : ObserverManager<KeyInputObserver> {

        private readonly Dictionary<KeyCode, int> _detectKeys = new ();

        public void AddKeys(KeyCode[] keyCodes, int itemId) {
            foreach (var keyCode in keyCodes) {
                _detectKeys.Add(keyCode, itemId);
            }
        }

        public void RemoveKey(KeyCode keyCode) {
            _detectKeys.Remove(keyCode);
        }

        public void ClearKeys() {
            _detectKeys.Clear();
        }
        
        public void OnProcess(float delta) {
            foreach (var keyCode in _detectKeys.Keys) {
                if (Input.GetKeyDown(keyCode)) {    
                    KeyDownOnItem(keyCode);
                }
            }
        }

        public void KeyDownOnItem(KeyCode keyCode) {
            DispatchEvent(e=> e.KeyDownOnItem(_detectKeys[keyCode]));
        }
    }
}