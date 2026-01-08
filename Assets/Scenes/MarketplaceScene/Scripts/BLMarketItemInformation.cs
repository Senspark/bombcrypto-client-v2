using System;
using App;
using Data;
using DG.Tweening;
using Senspark;
using Game.Dialog;
using Game.Manager;

using JetBrains.Annotations;

using Scenes.MarketplaceScene.Scripts;

using Services;
using Services.Server;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BLMarketItemInformation : BaseBLItemInformation {
        [SerializeField]
        private InputField inputAmount;

        [SerializeField]
        private Button button;

        [SerializeField]
        private Button minusButton;

        [SerializeField]
        private Button plusButton;

        [SerializeField]
        private Button buttonTip;

        [SerializeField]
        private GameObject tip;

        [SerializeField]
        private Text tipText;
        
        [SerializeField]
        private GameObject premiumFrame;

        [SerializeField]
        private Transform parentTip;
        
        [SerializeField]
        private Text durationText;
        
        [SerializeField][CanBeNull] private ExpirationType expirationType;
        

        private ISoundManager _soundManager;
        private IMarketplace _marketplace;
        private IChestRewardManager _chestRewardManager;
        
        private ItemData _itemData;
        private int _amount;
        private GameObject _tipShow = null;
        private int _expiration;

        protected void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _marketplace = ServiceLocator.Instance.Resolve<IServerManager>().Marketplace;
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        }

        private void Start() {
            On7DayClick();
        }

        public override int GetInputAmount() {
            return _amount;
        }

        public override void UpdateItem(ItemData data, bool showButton = true) {
            DestroyTipShow();
            base.UpdateItem(data, showButton);
            if (data == null) {
                button.interactable = false;
                return;
            }
            button.interactable = true;

            _itemData = data;
            
            _amount = 1;
            tipText.text = data.Description;
            inputAmount.text = $"{_amount}";
            UpdateTotal();
            
            if (premiumFrame) {
                var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
                premiumFrame.SetActive(productItemManager.GetItem(data.ItemId).ItemKind == ProductItemKind.Premium);
            }

            if (durationText) {
                var duration = TimeSpan.FromSeconds(data.ExpirationAfter);
                durationText.text =
                    $"DURATION <color=#33FF00>{TimeUtil.ConvertTimeToStringDay(duration).ToUpper()}</color> AFTER EQUIPPED";
            }
            
            if(expirationType == null) {
                return;
            }
            if (_itemData.ProductData.ItemConfig.IsNoExpiredItem) {
                expirationType.HideExpirationType();
            } else {
                expirationType.gameObject.SetActive(true);
                expirationType.On7DayClick += On7DayClick;
                expirationType.On30DayClick += On30DayClick;
            }
        }

        public void OnAmountAllClicked() {
            _soundManager.PlaySound(Audio.Tap);
            inputAmount.text = $"{_itemData.Quantity}";
            UpdateTotal();
        }

        public void OnMinusButtonClicked() {
            _amount -= 1;
            UpdateTotal();
        }

        public void OnPlusButtonClicked() {
            _amount += 1;
            UpdateTotal();
        }

        public void OnInputChange() {
            _amount = GetValueFromInput(inputAmount);
            UpdateTotal();
        }

        private int GetValueFromInput(InputField input) {
            if (string.IsNullOrEmpty(input.text)) {
                input.text = $"1";
            }
            return int.TryParse(input.text, out var value) ? value : 1;
        }

        private void UpdateTotal() {
            if (_amount <= 0) {
                _amount = 1;
            }
            inputAmount.SetTextWithoutNotify(inputAmount.text == string.Empty ? string.Empty : $"{_amount}");

            minusButton.interactable = _amount > 1;
        }

        private void DestroyTipShow() {
            if (_tipShow != null) {
                Destroy(_tipShow);
            }
            _tipShow = null;
            buttonTip.interactable = true;
        }

        public void ShowTip() {
            if (parentTip == null) {
                return;
            }
            DestroyTipShow();
            buttonTip.interactable = false;
            _tipShow = Instantiate(tip, parentTip, true);
            var canvasGroup = _tipShow.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            _tipShow.SetActive(true);
            DOTween.Sequence()
                .Append(canvasGroup.DOFade(1.0f, 0.3f))
                .AppendInterval(3)
                .Append(canvasGroup.DOFade(0.0f, 0.3f))
                .AppendCallback(DestroyTipShow);
        }
        
        public void OnOrderItemMarketClick() {
            _soundManager.PlaySound(Audio.Tap);
            
            var minPrice = _itemData.ProductData.ItemConfig.MinPrice;
            var totalMinPrice = _amount * minPrice;
            var bLGem = _chestRewardManager.GetChestReward(BlockRewardType.Gem) +
                        _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
            if (bLGem < totalMinPrice) {
                OnOrderErrorCallback?.Invoke();
                return;
            }
            
            var expired = _itemData.ProductData.ItemConfig.IsNoExpiredItem ? 0 : _expiration;
            OnShowDialogOrderCallback?.Invoke(new OrderDataRequest(_itemData.ItemId, _amount, expired));
        }

        private void On7DayClick() {
            _expiration = 60 * 60 * 24 * 7;
        }
        private void On30DayClick() {
            _expiration = 60 * 60 * 24 * 30;
        }
    }
}

public class OrderDataRequest {
    public readonly int ItemId;
    public readonly int Amount;
    public readonly int Expiration;

    public OrderDataRequest(int itemId, int amount, int expiration) {
        ItemId = itemId;
        Amount = amount;
        Expiration = expiration;
    }
}