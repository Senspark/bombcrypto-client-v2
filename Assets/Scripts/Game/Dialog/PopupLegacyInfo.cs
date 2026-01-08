using System;
using System.Collections;
using System.Collections.Generic;

using App;

using BLPvpMode.Engine.Manager;

using Senspark;

using UnityEngine;

public class PopupLegacyInfo : MonoBehaviour {
    [SerializeField]
    private GameObject infoBsc, infoPolygon;
    
    private INetworkConfig _networkManager;
    
    private void Awake() {
        _networkManager = ServiceLocator.Instance.Resolve<INetworkConfig>();
    }
    
    private void OnEnable() {
        infoBsc.SetActive(_networkManager.NetworkType == NetworkType.Binance);
        infoPolygon.SetActive(_networkManager.NetworkType == NetworkType.Polygon);
    }
}
