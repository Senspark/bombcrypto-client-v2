using System;
using App;
using Game.UI;
using Senspark;
using UnityEngine;
using UnityEngine.UI;

public class WalletDisplayInfo : MonoBehaviour {
    [SerializeField] private GameObject infoBG;
    [SerializeField] private Text depositedTxt;
    [SerializeField] private Text minedTxt;
    [SerializeField] private Text totalInGameTxt;
    [SerializeField] private Text onChainTxt;
    
    private const string COLOR_ACTIVE = "#FAC660";
    private const string COLOR_DEACTIVE = "#7C7B78";
    private const int MAX_DIGIT_ROUNDED = 5;
    
    private NetworkType _networkType;
    private IBlockchainStorageManager _blockchainStorageManager;
    private bool _isInitDone;

    private void Awake() {
        InitInfo();
    }

    private void InitInfo() {
        if (_isInitDone) return;
        _networkType = ServiceLocator.Instance.Resolve<INetworkConfig>().NetworkType;
        _blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
        infoBG.gameObject.SetActive(false);
        _isInitDone = true;
    }

    public void SetInfo(double depositedVal, double minedVal, double totalVal, DataType dataType, MineAndDepositType tokenType) {
        InitInfo();
        depositedTxt.text = $"Deposited: <color={COLOR_DEACTIVE}>{Math.Round(depositedVal, MAX_DIGIT_ROUNDED)}</color>";
        minedTxt.text = $"Mined: <color={COLOR_DEACTIVE}>{Math.Round(minedVal, MAX_DIGIT_ROUNDED)}</color>";
        totalInGameTxt.text = $"Total in-game: <color={COLOR_ACTIVE}>{Math.Round(totalVal, MAX_DIGIT_ROUNDED)}</color>";
        onChainTxt.text = GetOnChainStatus(dataType, tokenType);
    }

    public void SetInfo(string totalVal) {
        InitInfo();
        totalInGameTxt.text = $"Total in-game: <color={COLOR_ACTIVE}>{totalVal}</color>";
    }
    
    private string GetOnChainStatus(DataType dataType, MineAndDepositType tokenType) {
        if (dataType == DataType.TR) return $"";
            
        if (_networkType == NetworkType.Binance && dataType == DataType.BSC ||
            _networkType == NetworkType.Polygon && dataType == DataType.POLYGON) {

            double onChainVal = 0;
            if (dataType == DataType.BSC && tokenType == MineAndDepositType.Bcoin) {
                onChainVal = _blockchainStorageManager.GetBalance(ObserverCurrencyType.WalletBCoin);
            }
            else if (dataType == DataType.BSC && tokenType == MineAndDepositType.Senspark) {
                onChainVal = _blockchainStorageManager.GetBalance(ObserverCurrencyType.WalletSenBsc);
            }
            else if (dataType == DataType.POLYGON && tokenType == MineAndDepositType.Bcoin) {
                onChainVal = _blockchainStorageManager.GetBalance(ObserverCurrencyType.WalletBomb);
            }
            else if (dataType == DataType.POLYGON && tokenType == MineAndDepositType.Senspark) {
                onChainVal = _blockchainStorageManager.GetBalance(ObserverCurrencyType.WalletSenPolygon);
            }
            return $"On-chain: <color={COLOR_ACTIVE}>{Math.Round(onChainVal, MAX_DIGIT_ROUNDED)}</color>";
        }
        return $"On-chain: <color={COLOR_DEACTIVE}>Not connected</color>";
    }
    
    public void OnInfoTriggerEnter() {
        infoBG.gameObject.SetActive(true);
    }
    
    public void OnInfoTriggerExit() {
        infoBG.gameObject.SetActive(false);
    }
}
