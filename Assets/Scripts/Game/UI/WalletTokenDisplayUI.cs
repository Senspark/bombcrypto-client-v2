using App;
using Senspark;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class WalletTokenDisplayUI : MonoBehaviour {
        [SerializeField]
        private Text coinTxt;

        [SerializeField]
        private ObserverCurrencyType tokenType;

        private IBlockchainStorageManager _blockchainStorageManager;
        private ObserverHandle _handle;

        private void Awake() {
            _blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_blockchainStorageManager, new BlockchainStorageManagerObserver() {
                    OnCurrencyChanged = UpdateValue,
                });
            UpdateValue(_blockchainStorageManager.GetBalance(tokenType));
        }

        private void OnDestroy() {
            _handle.Dispose();
        }

        private void UpdateValue(ObserverCurrencyType type, double value) {
            if (type != tokenType) {
                return;
            }
            UpdateValue(value);
        }

        private void UpdateValue(double value) {
            coinTxt.text = App.Utils.FormatBcoinValue(value);
        }
    }
}