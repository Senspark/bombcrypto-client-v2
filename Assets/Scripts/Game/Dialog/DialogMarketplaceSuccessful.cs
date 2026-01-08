using App;

using Senspark;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogMarketplaceSuccessful : Dialog {
        [FormerlySerializedAs("_icon")]
        [SerializeField]
        private Image icon;
        
        [FormerlySerializedAs("_skinName")]
        [SerializeField]
        private Text skinName;

        [FormerlySerializedAs("_text")]
        [SerializeField]
        private Text text;

        [FormerlySerializedAs("_title")]
        [SerializeField]
        private Text title;

        private ISoundManager _soundManager;

        public static DialogMarketplaceSuccessful Create() {
            var prefab =
                Resources.Load<DialogMarketplaceSuccessful>($"Prefabs/Dialog/{nameof(DialogMarketplaceSuccessful)}");
            return Instantiate(prefab);
        }

        public void Initialize(string title, int skinId, string skinName, string text) {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            icon.sprite = Resources.Load<Sprite>($"SkinChest/{skinId}");
            this.skinName.text = skinName;
            this.title.text = title;
            this.text.text = text;
        }

        public void OnButtonCloseClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}