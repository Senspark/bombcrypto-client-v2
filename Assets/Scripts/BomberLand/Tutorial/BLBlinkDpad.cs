using BLPvpMode.UI;

using UnityEngine;

namespace BomberLand.Tutorial {
    public class BLBlinkDpad : MonoBehaviour {
        [SerializeField]
        private RuntimeAnimatorController animatorController;

        private Animator _animator;
        
        private static readonly int Blink = Animator.StringToHash("Blink");
        private static readonly int Idle = Animator.StringToHash("Idle");

        public void Initialized(IBLInputKey input) {
            var image = input.GetBlinkButton();
            _animator = image.gameObject.GetComponent<Animator>();
            if (_animator == null) {
                _animator = image.gameObject.AddComponent<Animator>();
            }
            _animator.runtimeAnimatorController = animatorController;
        }

        public void StartBlink() {
            _animator.SetTrigger(Blink);
        }

        public void StopBlink() {
            _animator.SetTrigger(Idle);
        }
    }
}