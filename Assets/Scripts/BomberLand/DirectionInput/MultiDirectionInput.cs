using System.Collections.Generic;

using UnityEngine;

namespace BomberLand.DirectionInput {
    public class MultiDirectionInput : IDirectionInput {
        private readonly List<IDirectionInput> _inputs = new List<IDirectionInput>();

        private bool _enabled;
        private int _currentInput;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                foreach (var item in _inputs)
                {
                    item.Enabled = value;
                }
            }
        }

        public int CurrentInput {
            get => _currentInput;
            set {
                _currentInput = value;
                if (_currentInput >= _inputs.Count) {
                    _currentInput = _inputs.Count - 1;
                }
            }
        }

        public MultiDirectionInput AddInput(IDirectionInput input) {
            _inputs.Add(input);
            return this;
        }

        public void Clear() {
            _inputs.Clear();
        }
        
        public void RemoveInput(IDirectionInput input) {
            _inputs.Remove(input);
        }
        
        public Vector2 GetDirection() {
            return _inputs[_currentInput].GetDirection();
        }
    }
}