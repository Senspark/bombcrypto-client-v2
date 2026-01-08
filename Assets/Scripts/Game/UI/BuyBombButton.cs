using App;

using Cysharp.Threading.Tasks;

using Senspark;

using JetBrains.Annotations;

using Scenes.FarmingScene.Scripts;

using UnityEngine;

namespace Game.UI {
    public class BuyBombButton : MonoBehaviour {
        [SerializeField] [CanBeNull]
        private Canvas canvasDialog;

        [SerializeField]
        private bool autoHide;
    
        private ISoundManager _soundManager;
        private ObserverHandle _handle;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            if (!featureManager.EnableClaim || !canvasDialog) {
                gameObject.SetActive(false);
                return;
            }
            if (!autoHide) {
                return;
            }
            var storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            var blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
            var bomb = blockchainStorageManager.GetBalance(RpcTokenCategory.Bomb);
            var newUser = storageManager.NewUser;
            var enable = newUser && bomb == 0;
            gameObject.SetActive(enable);
            if (enable) {
                _handle = new ObserverHandle();
                _handle.AddObserver(blockchainStorageManager, new BlockchainStorageManagerObserver {
                    OnCurrencyChanged = OnCurrencyChanged
                });
            }
        }

        private void OnDestroy() {
            _handle?.Dispose();
        }

        public void OnBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            DialogShopCoin.Create().ContinueWith(dialog => {
                dialog.Show(canvasDialog);
            });
        }

        private void OnCurrencyChanged(ObserverCurrencyType type, double value) {
            if (type == ObserverCurrencyType.WalletBomb && value > 0) {
                gameObject.SetActive(false);
            }
        }
    }
}