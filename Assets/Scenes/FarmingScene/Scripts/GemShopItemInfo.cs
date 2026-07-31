using System;

using App;

using Data;

using Game.Dialog.BomberLand.BLFrameShop;
using Game.UI.Custom;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    // Detail panel of DialogGemShop, the gem counterpart of BLShopRockInfo. The native coin is the only
    // currency: a gem pack has no in-game list price, the app store owns the fiat one. Field names match
    // BLShopRockInfo's so swapping the component on a duplicated prefab keeps the references.
    public class GemShopItemInfo : MonoBehaviour {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text title;

        [SerializeField]
        private Text tQuantity;

        [SerializeField]
        private Text tNativePrice;

        [SerializeField]
        private Image nativeIcon;

        [SerializeField]
        private Button btBuy;

        [SerializeField]
        private CustomContentSizeFitter customLayout;

        private const string NoPrice = "--";

        private NativeTokenInfo _nativeToken;
        private Action _onBuy;

        private void Awake() {
            _nativeToken = NativeTokenInfo.Of(
                ServiceLocator.Instance.Resolve<INetworkConfig>(),
                ServiceLocator.Instance.Resolve<ILaunchPadManager>());
        }

        public void SetData(BLShopResource shopResource, IAPGemItemData d) {
            if (icon) icon.sprite = shopResource.GetImageIpaGem(d.ProductId);
            if (title) title.text = d.ItemName;
            if (tQuantity) {
                tQuantity.text = d.GemsBonus > 0
                    ? $"{d.GemReceive} <color=#DDF192ff>+{d.GemsBonus}</color>"
                    : $"+{d.GemReceive}";
            }
            if (nativeIcon && _nativeToken != null && _nativeToken.Icon) {
                nativeIcon.sprite = _nativeToken.Icon;
            }

            // A pack with no native entry keeps its slot and shows "--" rather than disappearing: an unset
            // rate is an operator problem, and hiding it makes the feature look absent. Only buy goes dead.
            var price = d.Prices.FindNative();
            var purchasable = _nativeToken != null && price != null;
            if (tNativePrice) {
                tNativePrice.text = purchasable ? ServerRewardTypes.FormatAmount(price.Price) : NoPrice;
            }
            if (btBuy) btBuy.interactable = purchasable;
            if (customLayout) customLayout.AutoLayoutHorizontal();
        }

        public void SetOnBuy(Action onBuy) {
            _onBuy = onBuy;
        }

        public void OnBtBuyClick() {
            _onBuy?.Invoke();
        }
    }
}
