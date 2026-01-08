using System;
using System.Globalization;
using App;
using Game.UI.Custom;
using Senspark;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopRockInfo : MonoBehaviour {
        [SerializeField]
        public Image icon;

        [SerializeField]
        public Text title;

        [SerializeField]
        public Text tQuantity;

        [SerializeField]
        public Button btBuy;
        
        [SerializeField]
        public Text tBcoinPrice;
        
        [SerializeField]
        public Image iconBSC;
        
        [SerializeField]
        public Image iconPolygon;

        [SerializeField]
        private CustomContentSizeFitter customLayout;

        private NetworkType _networkType;
        private Action<BlockRewardType> _onBuy;

        private void Awake() {
            _networkType = ServiceLocator.Instance.Resolve<INetworkConfig>().NetworkType;
        }
        
        public void SetData(BLShopResource shopResource, IRockPackConfig d) {
            btBuy.gameObject.SetActive(true);
            icon.sprite = shopResource.GetImageRock(d.PackageName);
            title.text = ConvertToTitleCase(d.PackageName);
            tQuantity.text = $"+{d.RockAmount}";
            tBcoinPrice.text = $"{d.BcoinPrice}";
            SetupNetworkIcon();
            customLayout.AutoLayoutHorizontal();
        }
        
        private string ConvertToTitleCase(string input) {
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(input.ToLower().Replace("_", " "));
        }
        
        private void SetupNetworkIcon() {
            iconBSC.gameObject.SetActive(_networkType == NetworkType.Binance);
            iconPolygon.gameObject.SetActive(_networkType == NetworkType.Polygon);
        }

        public void SetOnBuy(Action<BlockRewardType> onBuy) {
            _onBuy = onBuy;
        }

        public void OnBtBuyBcoinClick() {
            _onBuy?.Invoke(BlockRewardType.BCoin);
        }
    }
}