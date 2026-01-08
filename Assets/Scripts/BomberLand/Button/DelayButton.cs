using UnityEngine;
using UnityEngine.Serialization;

namespace BomberLand.Button {
    public class DelayButton : MonoBehaviour {
        [SerializeField]
        private float delayTime = 3f;
        
        [SerializeField]
        private UnityEngine.UI.Button button;

        private float _elapse;
        [HideInInspector]
        public bool isCountdown;

        private void Awake() {
            button.gameObject.SetActive(false);
            _elapse = delayTime;
            isCountdown = true;
        }

        private void Update() {
            if (!isCountdown) {
                return;
            }
            _elapse -= Time.deltaTime;
            if (_elapse > 0) {
                return;
            }
            ShowButton();
        }

        public void ShowButton() {
            isCountdown = false;
            button.gameObject.SetActive(true);
        }
    }
}