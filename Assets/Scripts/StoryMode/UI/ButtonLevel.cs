using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace StoryMode.UI {
    public class ButtonLevel : MonoBehaviour {
        [SerializeField]
        private Button button;

        [SerializeField]
        private Sprite spriteOn;

        [SerializeField]
        private Sprite spriteOff;

        [SerializeField]
        private Text levelTxt;

        private System.Action<int> _onClickCallback;
        private int _myIndex;
        private Image _myImage;

        private void Awake() {
            _myImage = button.GetComponent<Image>();
        }

        public void Init(int index, System.Action<int> callback) {
            _myIndex = index;
            _onClickCallback = callback;
        }

        public void OnButtonClick() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            _onClickCallback?.Invoke(_myIndex);
        }

        public void SetInteractable(bool value) {
            button.interactable = value;
        }

        public void SetHighLight(bool value) {
            _myImage.sprite = value ? spriteOn : spriteOff;
        }

        public void SetLevelText(int level, int stage) {
            levelTxt.text = "" + stage + "-" + level;
        }
    }
}
