using System;
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
        
        private IChestRewardManager _chestRewardManager;
        private ObserverHandle _handle;
        
        private void Awake() {
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
            _handle.Dispose();
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
            var totalVal = Math.Truncate(value).ToString("N0");
            coinTxt.text = totalVal;
            walletDisplayInfo.SetInfo(totalVal);
        }
    }
}