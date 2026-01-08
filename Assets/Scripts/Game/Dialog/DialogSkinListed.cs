using App;

using Senspark;

using Services;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogSkinListed : Dialog {
        [FormerlySerializedAs("_icon")]
        [SerializeField]
        private Image icon;
        
        [FormerlySerializedAs("_id")]
        [SerializeField]
        private Text id;
        
        private ISoundManager _soundManager;
        
        public static DialogSkinListed Create() {
            var prefab = Resources.Load<DialogSkinListed>($"Prefabs/Dialog/{nameof(DialogSkinListed)}");
            return Instantiate(prefab);
        }

        public void Initialize(int id, int skinId, string skinName, (int Type, double Value) price) {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            icon.sprite = Resources.Load<Sprite>($"SkinChest/{skinId}");
            this.id.text = $"Item ID: {id}";
        }

        public void OnButtonCloseClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
        
        public void OnButtonOkClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}
