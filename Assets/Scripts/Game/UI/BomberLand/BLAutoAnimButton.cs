using UnityEngine;

namespace Game.UI.BomberLand {
    public class BLAutoAnimButton : MonoBehaviour {
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private float loopDelay = 10f;

        private float _timeProgress;
        private static readonly int Action = Animator.StringToHash("Action");
        private static readonly int Stop = Animator.StringToHash("Stop");

        private void Update() {
            _timeProgress += Time.deltaTime;
            if (_timeProgress < loopDelay) {
                return;
            }
            _timeProgress = 0;
            animator.SetTrigger(Action);
            animator.SetTrigger(Stop);
        }
    }
}