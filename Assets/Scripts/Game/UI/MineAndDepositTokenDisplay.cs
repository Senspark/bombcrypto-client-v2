using System;
using App;
using Game.UI;
using Senspark;

using UnityEngine;
using UnityEngine.UI;

public enum MineAndDepositType {
    Bcoin,
    Senspark
}
public class MineAndDepositTokenDisplay : MonoBehaviour {
    [SerializeField]
    private Text coinTxt;
    
    [SerializeField]
    private MineAndDepositType tokenType;
    
    [SerializeField]
    private DataType dataType;
    
    [SerializeField]
    private WalletDisplayInfo walletDisplayInfo;
    
    private const int MAX_DIGIT_ROUNDED = 5;
    
    private IChestRewardManager _chestRewardManager;
    private ObserverHandle _handle;
    
    private void Awake() {
        _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        _handle = new ObserverHandle();
        _handle.AddObserver(_chestRewardManager, new ChestRewardManagerObserver() {
            OnRewardBalanceChanged = UpdateValue,
        });

        //Update gía trị lần đầu tiên
        UpdateValue(dataType, true);
    }

    private void OnDestroy() {
        _handle.Dispose();
    }

    private void UpdateValue(BlockRewardType type, DataType scope, double value) {
        //Khi có thay đổi value gọi để cập nhật lại value
        UpdateValue(scope);
    }

    private void UpdateValue(DataType network, bool firstTime = false) {
        if(dataType != network && !firstTime)
            return;

        if (tokenType == MineAndDepositType.Bcoin) {
            double depositedVal = _chestRewardManager.GetChestReward(BlockRewardType.BCoinDeposited, dataType);
            double minedVal = _chestRewardManager.GetChestReward(BlockRewardType.BCoin, dataType);
            double totalVal = _chestRewardManager.GetBcoinRewardAndDeposit(dataType);

            UpdateValue(totalVal);
            walletDisplayInfo.SetInfo(depositedVal, minedVal, totalVal, dataType, tokenType);
        }
        else {
            double depositedVal = _chestRewardManager.GetChestReward(BlockRewardType.SensparkDeposited, dataType);
            double minedVal = _chestRewardManager.GetChestReward(BlockRewardType.Senspark, dataType);
            double totalVal = _chestRewardManager.GetSenRewardAndDeposit(dataType);

            UpdateValue(totalVal);
            walletDisplayInfo.SetInfo(depositedVal, minedVal, totalVal, dataType, tokenType);
        }
    }

    private void UpdateValue(double value) {
        coinTxt.text = RoundToDigitsAfterDecimal(value).ToString();
    }

    private double RoundToDigitsAfterDecimal(double value) {
        double multiplier = Math.Pow(10, MAX_DIGIT_ROUNDED);
        return Math.Round(value * multiplier) / multiplier;
    }
}