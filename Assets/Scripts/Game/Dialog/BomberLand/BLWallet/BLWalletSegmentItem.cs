using System;
using App;
using Scenes.FarmingScene.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLWallet {
    public class BLWalletSegmentItem : MonoBehaviour
    {
        [SerializeField]
        private GameObject selected;
        
        [SerializeField]
        private Image icon;
        
        [SerializeField]
        private Text tokenName;
    
        [SerializeField]
        private Text balanceLbl;
        
        [SerializeField]
        private Text pendingLbl;
        
        [SerializeField]
        private Text lbNetwork;

        public float Balance { get; private set; }
        public IRewardType RewardType { get; private set; }
        
        private Action _onSelected;
        private readonly int _minHeightIcon = 34;
        private TypeMenuLeftWallet _tab;

        protected void Awake() {
            selected.SetActive(false);
        }

        public void ApplyData(DataWallet data) {
            if (data.IsBridge) {
                ApplyBridgeData(data);
                return;
            }
            if (data.RefTokenData != null) {
                tokenName.text = data.RefTokenData.displayName;
                if (data.RefTokenData.icon != null) {
                    data.RefTokenData.icon.texture.filterMode = FilterMode.Point;
                    icon.sprite = data.RefTokenData.icon;
                    // icon.SetNativeSize();
                    // if (icon.sprite.rect.height > _minHeightIcon) {
                    //     var scale = _minHeightIcon / icon.sprite.rect.height; 
                    //     icon.transform.localScale = new Vector3(scale, scale, 1.0f);
                    // } else {
                    //     icon.transform.localScale = new Vector3(1, 1, 1.0f);
                    // }
                }
                
            }
            Balance = data.ClaimValue;
            RewardType = data.RefRewardType;
            var isIntegerType = data.RefRewardType != null && App.RewardUtils.IsIntegerDisplayType(data.RefRewardType.Type);
            balanceLbl.text = App.Utils.FormatSmartMoney(data.ClaimValue, isIntegerType);
            pendingLbl.text = App.Utils.FormatSmartMoney(data.PendingValue, isIntegerType);
            if (data.RefTokenData != null) {
                lbNetwork.text = RewardUtils.NetworkDisplayName(data.RefTokenData.networkSymbol);
            }
        }

        private void ApplyBridgeData(DataWallet data) {
            tokenName.text = $"{data.BridgeSymbol} BRIDGE";
            if (data.BridgeIcon) {
                icon.sprite = data.BridgeIcon;
            }
            Balance = data.ClaimValue;
            RewardType = data.RefRewardType;
            balanceLbl.text = data.BridgeBalanceKnown ? App.Utils.FormatSmartMoney(data.ClaimValue, false) : "-";
            if (pendingLbl) {
                pendingLbl.text = "";
            }
            if (lbNetwork) {
                lbNetwork.text = $"{BridgeNetworkDisplay(data.BridgeDepositChain)} -> {BridgeNetworkDisplay(data.BridgeWithdrawChain)}";
            }
        }

        private static string BridgeNetworkDisplay(string chain) {
            return chain == "POLYGON" ? "POLYGON" : "BNB";
        }

        public void SetItemTab(TypeMenuLeftWallet tab) {
            _tab = tab;
        }

        public TypeMenuLeftWallet GetItemTab() {
            return _tab;
        }

        public void UiSetSelect(bool isSelect) {
            selected.SetActive(isSelect);
        }

        public void SetOnBtSelect(Action onSelected) {
            _onSelected = onSelected;
        }
        
        public void OnBtSelect() {
            _onSelected?.Invoke();
        }
    }
}
