using UnityEngine;
using UnityEngine.UI;

namespace GroupMainMenu {
    public class MainMenuButton : MMButton {
        [SerializeField]
        private Image icon;
        
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private Text title;
        
        [SerializeField]
        private GuiMainMenuResource resource;
        
        private ButtonType _type;
        private System.Action<ButtonType> _onClickedCallback;

        public void SetType(ButtonType type, System.Action<ButtonType> onClickedCallback) {
            _type = type;
            var picker = resource.GetButtonResource(type);
            icon.sprite = picker.sprite;
            animator.runtimeAnimatorController = picker.animator;
            title.text = picker.title;
            _onClickedCallback = onClickedCallback;
        }

        protected override void Awake() {
            button.onClick.AddListener(OnClicked);
        }
        
        private void OnClicked() {
            _onClickedCallback?.Invoke(_type);
        }
    }
}