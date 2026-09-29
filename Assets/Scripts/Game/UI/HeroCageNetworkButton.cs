using App;

using Senspark;

using UnityEngine;

namespace Game.UI {
    public class HeroCageNetworkButton : MonoBehaviour {
        [SerializeField]
        private HeroCageRewardPanel panel;

        [SerializeField]
        private NetworkTypeInServer network;

        private ISoundManager _soundManager;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public void OnBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            panel.Claim(network);
        }
    }
}
