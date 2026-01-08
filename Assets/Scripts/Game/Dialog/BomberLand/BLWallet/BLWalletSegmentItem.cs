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
            //DevHoang_20250715: Another format for Base
            if (AppConfig.IsBase()) {
                balanceLbl.text = App.Utils.FormatBaseValue(data.ClaimValue);
            } else {
                balanceLbl.text = App.Utils.FormatBcoinValue(data.ClaimValue);
            }
            if (AppConfig.IsBase()) {
                pendingLbl.text = App.Utils.FormatBaseValue(data.PendingValue);
            } else {
                pendingLbl.text = App.Utils.FormatBcoinValue(data.PendingValue);
            }
            if (data.RefTokenData != null) {
                lbNetwork.text = data.RefTokenData.networkSymbolDisplayName;
            }
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
