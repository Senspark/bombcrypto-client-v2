using Game.UI.Animation;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Button {
    public class ReviveButtonAds : MonoBehaviour {
        [SerializeField]
        private float countDownTime = 6f;
        
        [SerializeField]
        private Text countText;

        [SerializeField]
        private ButtonZoomAndFlash buttonColor;
        
        [SerializeField]
        private GameObject buttonGrey;

        [SerializeField]
        private DelayButton delayButton;
        
        private float _elapse;
        private bool _isCountdown;

        private void Start() {
            buttonColor.SetActive(true);
            buttonGrey.SetActive(false);
            _elapse = countDownTime;
            _isCountdown = true;
        }
        
        private void Update() {
            if (!_isCountdown) {
                return;
            }
            _elapse -= Time.deltaTime;
            if (_elapse > 0) {
                countText.text = $"{Mathf.CeilToInt(_elapse)}";
                return;
            }
            _isCountdown = false;
            ShowButtonNext();
        }

        public void StopCountDown() {
            _isCountdown = false;
            ShowButtonNext();
        }
        
        private void ShowButtonNext() {
            buttonColor.SetActive(false);
            buttonGrey.SetActive(true);
            delayButton.gameObject.SetActive(true);
            delayButton.ShowButton();
        }
    }
}