using Game.Dialog;

using UnityEngine;

namespace PvpMode.Dialogs {
    public class DialogHurryUp : MonoBehaviour {
        [SerializeField]
        private Animator animator;

        private const float HurryTime = 5;
        private float _timeProcess;
        private static readonly int Intro = Animator.StringToHash("Intro");

        public static DialogHurryUp Create(Transform parent) {
            var prefab = Resources.Load<DialogHurryUp>("Prefabs/PvpMode/Dialog/BomberLand/DialogHurryUp");
            return Instantiate(prefab, parent);
        }

        private void Start() {
            _timeProcess = 0;
        }

        private void Update() {
            _timeProcess -= Time.deltaTime;
            if (_timeProcess >= 0) {
                return;
            }
            Hide();
        }

        public void Show() {
            _timeProcess = HurryTime;
            gameObject.SetActive(true);
            animator.SetTrigger(Intro);
        }

        public void Hide() {
            gameObject.SetActive(false);
        }
    }
}