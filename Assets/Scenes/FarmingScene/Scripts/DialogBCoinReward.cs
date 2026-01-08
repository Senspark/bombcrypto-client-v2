using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Services.Rewards;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogBCoinReward : Dialog {
        [SerializeField]
        private Text tokenTxt;

        [SerializeField]
        private Image tokenIcon;

        private ISoundManager _soundManager;

        public static UniTask<DialogBCoinReward> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogBCoinReward>();

        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public DialogBCoinReward SetReward(TokenData data, double amount) {
            if (data != null) {
                tokenTxt.text = $"{App.Utils.FormatBcoinValue(amount)} {data.displayName}";
                tokenIcon.sprite = data.icon;
            }
            return this;
        }

        public void OnOkBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}