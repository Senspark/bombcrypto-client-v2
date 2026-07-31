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
        private GameObject nativeBuyGroup;

        [SerializeField]
        private Text tNativePrice;

        [SerializeField]
        private Image nativeIcon;

        [SerializeField]
        private CustomContentSizeFitter customLayout;

        private NetworkType _networkType;
        private NativeTokenInfo _nativeToken;
        private Action<BlockRewardType> _onBuy;

        private void Awake() {
            var networkConfig = ServiceLocator.Instance.Resolve<INetworkConfig>();
            _networkType = networkConfig.NetworkType;
            _nativeToken = NativeTokenInfo.Of(networkConfig, ServiceLocator.Instance.Resolve<ILaunchPadManager>());
        }

        public void SetData(BLShopResource shopResource, IRockPackConfig d) {
            // BCOIN is retired as a purchase currency, server side included. The button stays in the prefab
            // so nothing has to be rewired — it is just never shown.
            btBuy.gameObject.SetActive(false);
            icon.sprite = shopResource.GetImageRock(d.PackageName);
            title.text = ConvertToTitleCase(d.PackageName);
            tQuantity.text = $"+{d.RockAmount}";
            tBcoinPrice.text = $"{d.BcoinPrice}";
            SetupNetworkIcon();
            SetupNativePrice(d);
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

        // prices[] carries a native entry only on a chain that has a native balance.
        private void SetupNativePrice(IRockPackConfig d) {
            var price = d.Prices.FindNative();
            var available = _nativeToken != null && price != null;
            if (nativeBuyGroup) nativeBuyGroup.SetActive(available);
            if (!available) {
                return;
            }
            if (tNativePrice) tNativePrice.text = ServerRewardTypes.FormatAmount(price.Price);
            if (nativeIcon && _nativeToken.Icon) nativeIcon.sprite = _nativeToken.Icon;
        }

        public void SetOnBuy(Action<BlockRewardType> onBuy) {
            _onBuy = onBuy;
        }

        public void OnBtBuyBcoinClick() {
            _onBuy?.Invoke(BlockRewardType.BCoin);
        }

        public void OnBtBuyNativeClick() {
            if (_nativeToken == null) {
                return;
            }
            _onBuy?.Invoke(_nativeToken.Type);
        }
    }
}
