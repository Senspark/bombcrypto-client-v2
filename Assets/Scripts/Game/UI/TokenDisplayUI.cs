using System;
using App;
using Senspark;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public enum DataType {
        TR,
        BSC,
        POLYGON,
        TON,
        SOL,
        RON,
        BAS,
        VIC
    }
    
    public class TokenDisplayUI : MonoBehaviour {
        [SerializeField]
        private Text coinTxt;
        
        [SerializeField]
        private BlockRewardType tokenType;
        
        [SerializeField]
        private DataType dataType;
        
        [SerializeField]
        private WalletDisplayInfo walletDisplayInfo;
        
        private IChestRewardManager _chestRewardManager;
        private ObserverHandle _handle;
        
        private void Awake() {
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_chestRewardManager, new ChestRewardManagerObserver() {
                    OnSameNetworkRewardChanged = UpdateValue,
                });
            
            if (dataType == DataType.TR) {
                UpdateValue(_chestRewardManager.GetChestReward(tokenType));
            } else {
                UpdateValue(_chestRewardManager.GetChestReward(tokenType, dataType.ToString()));
            }
        }
        
        private void OnDestroy() {
            _handle.Dispose();
        }
        
        private void UpdateValue(BlockRewardType type, double value, string network) {
            if (type != tokenType) {
                return;
            }
            if (dataType == DataType.TR)
                UpdateValue(value);
            
            else if (dataType == ConvertToNetwork(network)) {
                UpdateValue(value);
            }
        }
        
        private void UpdateValue(double value) {
            var totalVal = Math.Truncate(value).ToString("N0");
            coinTxt.text = totalVal;
            walletDisplayInfo.SetInfo(totalVal);
        }
        
        private DataType ConvertToNetwork(string network) {
            return network switch {
                "BSC" => DataType.BSC,
                "POLYGON" => DataType.POLYGON,
                "TR" => DataType.TR,
                _ => DataType.BSC
            };
        }
    }
}