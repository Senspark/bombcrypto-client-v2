using System;
using System.Collections;
using System.Collections.Generic;

using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

public class WalletTokenUI : MonoBehaviour {
    [SerializeField]
    private Text coinTxt;
    private IBlockchainManager _blockchainManager;
    private IBlockchainStorageManager _blockchainStorageManager;
    private ObserverHandle _handle;
    [SerializeField]
    private ObserverCurrencyType tokenType;

    private async void Awake() {
        _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
        _blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
        _handle = new ObserverHandle();
        _handle.AddObserver(_blockchainStorageManager, new BlockchainStorageManagerObserver() {
                OnCurrencyChanged = UpdateValue,
            });
        var amount = await _blockchainManager.GetBalance(RpcTokenCategory.SenPolygon);
        UpdateValue(amount);
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
