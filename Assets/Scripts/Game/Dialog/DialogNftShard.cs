using App;

using Senspark;

using UnityEngine;

namespace Game.Dialog {
    public class DialogNftShard : Dialog {
        private IAudioManager _audioManager;

        public static DialogNftShard Create() {
            var prefab = Resources.Load<DialogNftShard>("Prefabs/Dialog/DialogNftShard");
            return Instantiate(prefab);
        }

        public void OnButtonCloseClicked() {
            _audioManager.PlaySound(Audio.Tap);
            Hide();
        }

        public void OnButtonCollectClicked() {
            _audioManager.PlaySound(Audio.Tap);
            
        }

        private void Start() {
            _audioManager = ServiceLocator.Instance.Resolve<IAudioManager>();
        }
    }
}
