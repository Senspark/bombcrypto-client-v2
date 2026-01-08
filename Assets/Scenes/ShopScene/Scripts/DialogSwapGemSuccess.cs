using App;

using Constant;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;

namespace Scenes.ShopScene.Scripts {
    public class DialogSwapGemSuccess : Dialog {
        [SerializeField]
        private TMP_Text amountToken;

        private ISoundManager _soundManager;
        private INetworkConfig _network;

        private static UniTask<DialogSwapGemSuccess> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogSwapGemSuccess>();
        }
        
        public static void ShowInfo(Canvas canvas, double amount, int tokenType) {
            Create().ContinueWith(dialog => {
                dialog.SetInfo(amount, tokenType);
                dialog.Show(canvas);
            });
        }

        private void SetInfo(double amount, int tokenType) {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _network = ServiceLocator.Instance.Resolve<INetworkConfig>();
            
            if (tokenType == (int)RewardType.BCOIN) {
                if (_network.NetworkType == NetworkType.Binance) {
                    amountToken.text = "<sprite=3>+" + amount.ToString("0.########");
                } else {
                    amountToken.text = "<sprite=0>+" + amount.ToString("0.########");
                }
            } else {
                if (_network.NetworkType == NetworkType.Binance) {
                    amountToken.text = "<sprite=2>+" + amount.ToString("0.########");
                } else {
                    amountToken.text = "<sprite=1>+" + amount.ToString("0.########");
                }
            }
        }

        public void OnOkBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}