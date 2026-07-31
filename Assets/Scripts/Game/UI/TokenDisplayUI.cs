using App;
using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class TokenDisplayUI : MonoBehaviour {
        [SerializeField]
        private Text coinTxt;
        
        [SerializeField]
        private BlockRewardType tokenType;
        
        [SerializeField]
        private DataType dataType;
        
        [SerializeField]
        private WalletDisplayInfo walletDisplayInfo;

        // Hide the whole display when the build's network is not the one dataType names. Off by default:
        // most displays are shared across networks and must keep showing whatever the balance is.
        [SerializeField]
        private bool hideOnOtherNetwork;

        private IChestRewardManager _chestRewardManager;
        private ObserverHandle _handle;

        private void Awake() {
            if (hideOnOtherNetwork && !MatchesCurrentNetwork()) {
                gameObject.SetActive(false);
                return;
            }
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_chestRewardManager, new ChestRewardManagerObserver() {
                    OnRewardBalanceChanged = UpdateValue,
                });

            if (dataType == DataType.TR) {
                UpdateValue(_chestRewardManager.GetChestReward(tokenType));
            } else {
                UpdateValue(_chestRewardManager.GetChestReward(tokenType, dataType));
            }
        }
        
        private void OnDestroy() {
            _handle?.Dispose();
        }

        // TR is the cross-network scope, so there is nothing for it to mismatch.
        private bool MatchesCurrentNetwork() {
            if (dataType == DataType.TR) {
                return true;
            }
            var network = ServiceLocator.Instance.Resolve<INetworkConfig>().NetworkType;
            return RewardUtils.ConvertNetworkToDatatype(network) == dataType;
        }
        
        private void UpdateValue(BlockRewardType type, DataType scope, double value) {
            if (type != tokenType) {
                return;
            }
            if (dataType == DataType.TR)
                UpdateValue(_chestRewardManager.GetChestReward(tokenType));
            else if (dataType == scope)
                UpdateValue(value);
        }

        private void UpdateValue(double value) {
            var totalVal = App.Utils.FormatSmartMoney(value, App.RewardUtils.IsIntegerDisplayType(tokenType));
            coinTxt.text = totalVal;
            walletDisplayInfo.SetInfo(totalVal);
        }
    }
}