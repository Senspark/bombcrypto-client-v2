using App;
using Senspark;

using TMPro;

using UnityEngine;

namespace Game.UI {
    
    public class ChestRewardDepositDisplayUI : MonoBehaviour {
        [SerializeField]
        private TMP_Text coinTxt;
        
        [SerializeField]
        private BlockRewardType tokenType;
        
        [SerializeField] private DataType dataType;
        
        private IChestRewardManager _chestRewardManager;
        private ObserverHandle _handle;
        
        private void Awake() {
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_chestRewardManager, new ChestRewardManagerObserver() {
                    OnSameNetworkRewardChanged = UpdateValue,
                });
            UpdateValue(_chestRewardManager.GetChestReward(tokenType));

        }
        
        private void OnDestroy() {
            _handle.Dispose();
        }
        
        private void UpdateValue(BlockRewardType type, double value, string network) {
            if (type != tokenType) {
                return;
            }
            if (dataType == ConvertToNetwork(network)) {
                UpdateValue(value);
            }
        }
        
        private void UpdateValue(double value) {
            var totalVal = App.Utils.FormatBcoinValue(value);
            coinTxt.text = totalVal;
        }
        
        private DataType ConvertToNetwork(string network) {
            return network switch {
                "BSC" => DataType.BSC,
                "POLYGON" => DataType.POLYGON,
                "TR" => DataType.TR,
                "TON" => DataType.TON,
                "SOL" => DataType.SOL,
                _ => DataType.BSC
            };
        }
    }
}