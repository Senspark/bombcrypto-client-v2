using System;
using App;
using PvpMode.Manager;
using Senspark;
using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Component {
    public class ButtonUseBooster : MonoBehaviour {
        [SerializeField]
        private GameObject content;

        [SerializeField]
        private UnityEngine.UI.Button button;

        [SerializeField]
        private BoosterType type;

        public BoosterType Type {
            get => type;
        }

        [SerializeField]
        private Text quantityText;

        [SerializeField]
        private Text coolDownText;

        [SerializeField]
        private Image progress;

        [SerializeField]
        private Text hotKey;

        [SerializeField]
        private Animator animator;

        private float _coolDown;
        private float _elapsed;
        private bool _isCountDown;
        private Action _onEndCountDown;
        private static readonly int Blink = Animator.StringToHash("Blink");
        private static readonly int Idle = Animator.StringToHash("Idle");

        public bool Interactable {
            get => button.interactable;
            set {
                if (value && !_isCountDown) {
                    button.interactable = true;
                    if (animator) {
                        animator.SetTrigger(Blink);
                    }
                } else {
                    button.interactable = false;
                    if (animator) {
                        animator.SetTrigger(Idle);
                    }
                }
            }
        }

        public void SetVisible(bool value) {
            content.SetActive(value);
        }
        
        private void Awake() {
            ShowHotKey(true);
            progress.fillAmount = 0;
            coolDownText.text = "";
            // Fix vói trường hợp tournament, sẽ không có dataBooster 
            var boosterManager = ServiceLocator.Instance.Resolve<IBoosterManager>();
            var dataBooster = boosterManager.GetDataBooster(type);
            _coolDown = dataBooster?.CoolDown ?? 0;
            if (type == BoosterType.Shield || type == BoosterType.Key) {
                KeyCode keyCode = ServiceLocator.Instance.Resolve<IHotkeyControlManager>()
                    .GetHotkeyCombo()
                    .GetControl(ControlKey.UseItem);
                hotKey.text = CustomHotKeyUI.GetStringName(keyCode);
            }
        }

        private string SetHotKeyText(KeyCode keyCode) {
            switch (keyCode) {
                case KeyCode.Return:
                    return "Enter";
                case KeyCode.LeftControl:
                    return "Ctrl";
                case KeyCode.RightControl:
                    return "Ctrl";
                case KeyCode.LeftAlt:
                    return "Alt";
                case KeyCode.RightAlt:
                    return "Alt";
                case KeyCode.LeftShift:
                    return "Shift";
                case KeyCode.RightShift:
                    return "Shift";
            }
            return keyCode.ToString();
        }

        private void Update() {
            if (!_isCountDown) {
                return;
            }
            _elapsed -= Time.deltaTime;
            if (_elapsed <= 0) {
                progress.fillAmount = 0;
                coolDownText.text = "";
                _isCountDown = false;
                _onEndCountDown?.Invoke();
                return;
            }
            var percent = (_coolDown - _elapsed) / _coolDown;
            progress.fillAmount = 1 - percent;
            coolDownText.text = $"{(int)_elapsed}";
        }

        public void ShowHotKey(bool value) {
#if UNITY_WEBGL || FORCE_USE_KEYBOARD
            hotKey.gameObject.SetActive(value);
#else
            hotKey.gameObject.SetActive(false);
#endif            
        }

        public void SetQuantity(int value) {
            quantityText.text = value > 99 ? "99+" : $"{value}";
            if (value > 0) {
                quantityText.color = Color.green;
            }
        }

        public void SetCoolDown(float value) {
            _coolDown = value;
        }

        public void StartCoolDown(Action callback) {
            _onEndCountDown = callback;
            _elapsed = _coolDown;
            progress.fillAmount = 1;
            _isCountDown = true;
        }

        public void Reset() {
            progress.fillAmount = 0;
            coolDownText.text = "";
            _isCountDown = false;
        }
    }
}