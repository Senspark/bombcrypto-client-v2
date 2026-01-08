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
            OnSameNetworkRewardChanged = UpdateValue,
        });
        
        //Update gía trị lần đầu tiên
        UpdateValue(ConvertToDataType(dataType), true);
    }
    
    private void OnDestroy() {
        _handle.Dispose();
    }
    
    private void UpdateValue(BlockRewardType type, double value, string network) {
        //Khi có thay đổi value gọi để cập nhật lại value
        UpdateValue(network);
    }
    
    private void UpdateValue(string network, bool firstTime = false) {
        if(dataType != ConvertToNetwork(network) && !firstTime)
            return;
        
        if (tokenType == MineAndDepositType.Bcoin) {
            double depositedVal = _chestRewardManager.GetChestReward(BlockRewardType.BCoinDeposited, dataType.ToString());
            double minedVal = _chestRewardManager.GetChestReward(BlockRewardType.BCoin, dataType.ToString());
            double totalVal = _chestRewardManager.GetBcoinRewardAndDeposit(network);
            
            UpdateValue(totalVal);
            walletDisplayInfo.SetInfo(depositedVal, minedVal, totalVal, dataType, tokenType);
        }
        else {
            double depositedVal = _chestRewardManager.GetChestReward(BlockRewardType.SensparkDeposited, dataType.ToString());
            double minedVal = _chestRewardManager.GetChestReward(BlockRewardType.Senspark, dataType.ToString());
            double totalVal = _chestRewardManager.GetSenRewardAndDeposit(network);
            
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
    
    private DataType ConvertToNetwork(string network) {
        return network switch {
            "BSC" => DataType.BSC,
            "POLYGON" => DataType.POLYGON,
            "TR" => DataType.TR,
            _ => DataType.BSC
        };
    }
    private string ConvertToDataType(DataType type) {
        return type switch {
            DataType.BSC => "BSC",
            DataType.POLYGON => "POLYGON",
            DataType.TR => "TR",
            _ => "BSC"
        };
    }
}