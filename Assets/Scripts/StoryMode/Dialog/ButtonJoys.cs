using App;

using Senspark;

using UnityEngine;

namespace Game.Dialog {
    public class ButtonJoys : MonoBehaviour {
        [SerializeField]
        private GameObject joystick;

        [SerializeField]
        private GameObject joyPad;

        private IStorageManager _storageManager;

        public void ShowMe(bool value) {
            if (value) {
                _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
                gameObject.SetActive(true);
                var isJoyStick = _storageManager.IsJoyStickChoice;
                joystick.SetActive(isJoyStick);
                joyPad.SetActive(!isJoyStick);
            } else {
                gameObject.SetActive(false);
            }
        }

        public void Toggle() {
            var isJoyStick = !_storageManager.IsJoyStickChoice;
            _storageManager.IsJoyStickChoice = isJoyStick;
            joystick.SetActive(isJoyStick);
            joyPad.SetActive(!isJoyStick);
        }
    }
}