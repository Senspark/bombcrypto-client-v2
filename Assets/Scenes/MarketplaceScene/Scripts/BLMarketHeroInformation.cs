using App;
using Data;
using Senspark;
using Game.Dialog;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BLMarketHeroInformation : BaseBLHeroInformation {
        [SerializeField]
        private InputField inputAmount;

        [SerializeField]
        private Button button;

        [SerializeField]
        private Button minusButton;

        [SerializeField]
        private Button plusButton;
        
        [SerializeField]
        private GameObject premiumFrame;
        
        private ISoundManager _soundManager;
        private IChestRewardManager _chestRewardManager;

        
        private UIHeroData _heroData;
        private int _amount;

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        }

        public override int GetInputAmount() {
            return _amount;
        }
        
        public override void UpdateHero(UIHeroData data, bool showButton = true) {
            base.UpdateHero(data, showButton);
            if (data == null) {
                button.interactable = false;
                return;
            }
            button.interactable = true;
            
            _heroData = data;
            
            _amount = 1;
            inputAmount.text = $"{_amount}";
            UpdateTotal();
            
            if (premiumFrame) {
                var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
                premiumFrame.SetActive(productItemManager.GetItem(data.HeroId).ItemKind == ProductItemKind.Premium);
            }
        }

        public void OnMinusButtonClicked() {
            _amount -= 1;
            UpdateTotal();
        }

        public void OnPlusButtonClicked() {
            _amount += 1;
            UpdateTotal();
        }
        
        public void OnAmountAllClicked() {
            _soundManager.PlaySound(Audio.Tap);
            inputAmount.text = $"{_heroData.Quantity}";
            UpdateTotal();
        }

        public void OnInputChange() {
            _amount = GetValueFromInput(inputAmount);
            UpdateTotal();
        }
        
        public void OnOrderItemMarketClick() {
            _soundManager.PlaySound(Audio.Tap);
            
            var minPrice = _heroData.HeroData.DataBase.ItemConfig.MinPrice;
            var totalMinPrice = _amount * minPrice;
            var bLGem = _chestRewardManager.GetChestReward(BlockRewardType.Gem) +
                        _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
            if (bLGem < totalMinPrice) {
                OnOrderErrorCallback?.Invoke();
                return;
            }
            
            OnShowDialogOrderCallback?.Invoke(new OrderDataRequest(_heroData.HeroData.DataBase.ItemId, _amount, 0));
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
    }
}