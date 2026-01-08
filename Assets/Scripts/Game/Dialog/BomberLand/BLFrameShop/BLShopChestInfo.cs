using System;
using System.Collections.Generic;

using App;

using BomberLand.Button;

using Data;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopChestInfo : MonoBehaviour {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text title;

        [SerializeField]
        private Text tQuantity;

        [SerializeField]
        private BLShopButtonBuy buttonBuyPrefab;

        [SerializeField]
        private ChestShopType chestShopType;

        [SerializeField]
        private XButton[] buttonXs;

        private Action<GachaChestPrice, int> _onBuy;
        private Action _onShowInfo;

        private readonly List<BLShopButtonBuy> _buttons = new List<BLShopButtonBuy>();
        private int _buyChestIndex;
        private readonly int[] _buyChestAmount = { 1, 5, 10, 15 };

        public void SetData(BLShopResource shopResource, GachaChestShopData d) {
            chestShopType = d.ChestType;
            var r = shopResource.GetChestShop(chestShopType);
            icon.sprite = shopResource.GetImageChestShop(chestShopType);
            title.text = r.name;
            tQuantity.text = $"+{d.ItemQuantity} Random items";

            ClearButtons();
            foreach (var it in d.Prices) {
                var button = Instantiate(buttonBuyPrefab, transform);
                button.SetInfo(it, d.Prices.Length == 1, OnBtBuyClick);
                _buttons.Add(button);
            }
            _buyChestIndex = 0;
            foreach (var iter in buttonXs) {
                if (iter.Index == 0) {
                    iter.SetActive(true);
                } else {
                    iter.SetActive(false);
                }
            }
        }

        private void ClearButtons() {
            foreach (var button in _buttons) {
                Destroy(button.gameObject);
            }
            _buttons.Clear();
        }

        public void SetOnBuy(Action<GachaChestPrice, int> onBuy) {
            _onBuy = onBuy;
        }

        public void SetOnShowInfo(Action onShowInfo) {
            _onShowInfo = onShowInfo;
        }

        private void OnBtBuyClick(GachaChestPrice price, int quantity) {
            _onBuy?.Invoke(price, quantity);
        }

        public void OnBtInfoClick() {
            _onShowInfo?.Invoke();
        }

        public void OnXButtonClicked(XButton button) {
            // _soundManager.PlaySound(Audio.Tap);
            foreach (var iter in buttonXs) {
                if (iter == button) {
                    iter.SetActive(true);
                    _buyChestIndex = iter.Index;
                    RenderPrice(_buyChestIndex);
                } else {
                    iter.SetActive(false);
                }
            }
        }

        private void RenderPrice(int index) {
            var quantity = _buyChestAmount[index];
            foreach (var it in _buttons) {
                it.SetQuantiy(quantity);
            }
        }
    }
}