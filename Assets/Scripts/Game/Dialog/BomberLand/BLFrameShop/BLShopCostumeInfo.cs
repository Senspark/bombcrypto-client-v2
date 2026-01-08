using System;
using System.Collections.Generic;
using System.Linq;
using App;
using BomberLand.Component;
using BomberLand.Inventory;
using BomberLand.Shop;
using Constant;
using Data;
using Senspark;
using Engine.Utils;
using Game.Dialog.BomberLand.BLGacha;
using JetBrains.Annotations;
using Services;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopCostumeInfo : MonoBehaviour {
        [SerializeField, CanBeNull]
        public Image icon;

        [SerializeField]
        public Text title;

        [SerializeField]
        public GameObject premiumFrame;
        
        [SerializeField]
        private ImageAnimation imageAnimation;

        [SerializeField]
        private ExplodeAnimation explodeAnimation;
        
        [SerializeField]
        private OverlayTexture highlight;

        [SerializeField]
        public BLShopCostumeInfoBuy buy;

        [SerializeField]
        public GameObject objHeroStats;

        [SerializeField]
        public BLHeroStats heroStats;

        [SerializeField]
        private GameObject objStats;

        [SerializeField]
        private BLInventoryStat statPrefab;

        // Amount
        [SerializeField]
        private GameObject objAmount;
        
        [SerializeField]
        private InputField inputAmount;

        [SerializeField]
        private Button minusButton;

        [SerializeField]
        private Button plusButton;
        
        [SerializeField]
        private BLItemDesc itemDesc;
        
        [SerializeField]
        private BLProfileCard profileCard;

        private ISoundManager _soundManager;
        private IAbilityManager _abilityManager;
        
        private IChestRewardManager _chestRewardManager;
        private ObserverHandle _handle;

        private List<BLShopCostumeInfoBuy> _buys;
        private Action<int, int> _onBuy;

        private List<BLInventoryStat> _blStats;

        private int _amount;
        private readonly Dictionary<BlockRewardType, float> _availableCurrencies = new ();
        private readonly Dictionary<BlockRewardType, BLShopCostumeInfoBuy> _infoBuy = new ();
        
        protected void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _abilityManager = ServiceLocator.Instance.Resolve<IAbilityManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _handle = new ObserverHandle();
            _buys = new List<BLShopCostumeInfoBuy>(1) { buy };
            _blStats = new List<BLInventoryStat>(1) { statPrefab };

            if (profileCard != null) {
                profileCard.TryLoadData();
            }
        }

        private void OnDestroy() {
            _handle.Dispose();
        }

        public async void SetData(BLGachaRes resource, ProductItemData product, CostumeData costume, bool showAmount = true) {
            if (product.ItemType == (int)InventoryItemType.Hero) {
                objHeroStats.SetActive(true);
                objAmount.SetActive(showAmount);
                heroStats.UpdateHeroFromItemId(product.ItemId);
            } else {
                objHeroStats.SetActive(false);
                objAmount.SetActive(false);
            }

            SetItemDesc(resource, product.ItemId);
            UpdateUiStats(product);

            if (product.ItemType == (int)InventoryItemType.AvatarTR) {
                var sprites = await resource.GetAvatar(product.ItemId);
                imageAnimation.StartAni(sprites);
                explodeAnimation.gameObject.SetActive(false);
            } else {
                var resourcePicker = await resource.GetAnimationByItemId(product.ItemId);
                var type = resourcePicker.Type;
                if (type > 0 && resourcePicker.AnimationIdle.Length > 0) {
                    icon.gameObject.SetActive(false);
                    if (type == InventoryItemType.Fire) {
                        explodeAnimation.StartLoop(resourcePicker.AnimationIdle);
                        explodeAnimation.gameObject.SetActive(true);
                        imageAnimation.gameObject.SetActive(false);
                    } else {
                        imageAnimation.StartLoop(resourcePicker.AnimationIdle);
                        imageAnimation.gameObject.SetActive(true);
                        explodeAnimation.gameObject.SetActive(false);
                    }
                } else {
                    if (icon != null) {
                        icon.sprite = await resource.GetSpriteByItemId(product.ItemId);
                        icon.gameObject.SetActive(true);
                    }
                    imageAnimation.gameObject.SetActive(false);
                    explodeAnimation.gameObject.SetActive(false);
                }
            }

            title.text = product.Name;

            UpdateUiPrices(resource, costume);
            
            if(highlight) {
                // highlight.enabled = product.ItemKind == ProductItemKind.Premium;
                highlight.enabled = false;
                title.color = product.ItemKind == ProductItemKind.Premium
                    ? highlight.m_OverlayColor : Color.white;
            }
            if (premiumFrame) {
                premiumFrame.SetActive(product.ItemKind == ProductItemKind.Premium);
            }

            _amount = 1;
            UpdateTotal();
        }
        
        private async void SetItemDesc(BLGachaRes resource, int itemId) {
            if (itemDesc != null) {
                var text = await resource.GetDescription(itemId);
                itemDesc.SetText(text);
            }
        }

        private void UpdateUiStats(ProductItemData product) {
            if (product.Abilities.Length == 0) {
                objStats.SetActive(false);
                return;
            }
            var itemStats = product.Abilities
                .SelectMany(it => _abilityManager.GetStats(it))
                .Sum()
                .Select(it => new StatData(it.Key, 0, 0, it.Value))
                .ToArray();
            if (itemStats.Length == 0) {
                objStats.SetActive(false);
                return;
            }
            objStats.SetActive(true);
            if (itemStats.Length > _blStats.Count) {
                var numSpawn = itemStats.Length - _buys.Count;
                for (var idx = 0; idx < numSpawn; idx++) {
                    var newStat = Instantiate(statPrefab, statPrefab.transform.parent);
                    _blStats.Add(newStat);
                }
            }
            for (var idx = 0; idx < _blStats.Count; idx++) {
                var b = _blStats[idx];
                if (idx >= itemStats.Length) {
                    b.gameObject.SetActive(false);
                    continue;
                }
                b.gameObject.SetActive(true);
                var s = itemStats[idx];
                b.SetInfo(s.StatId, s.Value);
            }
        }

        private void UpdateUiPrices(BLGachaRes resource, CostumeData costume) {
            var prices = costume.Prices;
            if (prices.Length == 0) {
                buy.gameObject.SetActive(false);
                return;
            }
            if (prices.Length > _buys.Count) {
                var numSpawn = prices.Length - _buys.Count;
                for (var idx = 0; idx < numSpawn; idx++) {
                    var newBuy = Instantiate(buy, buy.transform.parent);
                    _buys.Add(newBuy);
                }
            }
            for (var idx = 0; idx < _buys.Count; idx++) {
                var b = _buys[idx];
                if (idx >= prices.Length) {
                    b.gameObject.SetActive(false);
                    continue;
                }
                b.gameObject.SetActive(true);
                b.SetData(resource, prices[idx]);
                var idxPrice = idx;
                b.SetOnBuy(() => { _onBuy?.Invoke(idxPrice, GetInputAmount()); });

                var rewardType = RewardUtils.ConvertToBlockRewardType(prices[idx].RewardType);
                _infoBuy[rewardType] = b;
                
                // Chỉ tạo 1 key = Gem (trong đó value = gem + lockedGem)
                if (rewardType == BlockRewardType.LockedGem) {
                    rewardType = BlockRewardType.Gem;
                }
                _availableCurrencies[rewardType] = GetAvailableCurrency(rewardType);
            }
            
            _handle.AddObserver(_chestRewardManager, new ChestRewardManagerObserver() {
                OnRewardChanged = UpdateAvailableCurrency
            });
            
        }

        private void UpdateAvailableCurrency(BlockRewardType type, double value) {
            // khi thay đổi lockedGem thì xem như thay đổi gem vì lấy value = Gem + LockedGem.
            if (type == BlockRewardType.LockedGem) {
                type = BlockRewardType.Gem;
            }
            if (!_availableCurrencies.ContainsKey(type)) {
                return;
            }
            _availableCurrencies[type] = GetAvailableCurrency(type);
            UpdateTotal();
        }

        public void SetOnBuy(Action<int, int> onBuy) {
            _onBuy = onBuy;
        }

        // Amount
        private int GetInputAmount() {
            return _amount;
        }
        
        public void OnMinusButtonClicked() {
            _amount -= 1;
            UpdateTotal();
        }

        public void OnPlusButtonClicked() {
            _amount += 1;
            UpdateTotal();
        }
        
        public void OnAmountMaxClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _amount = int.MaxValue;
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
            foreach (var key in _infoBuy.Keys) {
                UpdateTotal(_infoBuy[key]);
            }
        }

        private void UpdateTotal(BLShopCostumeInfoBuy infoBuy) {
            var unitPrice = infoBuy.Price;
            var max = (int)(_availableCurrencies[infoBuy.RewardType] / infoBuy.Price);
            if (_amount > max) {
                _amount = max;
            }
            if (_amount == 0) {
                _amount = 1;
            }
            inputAmount.SetTextWithoutNotify(inputAmount.text == string.Empty ? string.Empty : $"{_amount}");

            var total = _amount * unitPrice;
            infoBuy.UpdateTotalText(App.Utils.FormatBcoinValue(total));

            minusButton.interactable = _amount > 1;
            plusButton.interactable = _amount < max;
            
        }
        
        private float GetAvailableCurrency(BlockRewardType rewardType) {
            if (rewardType is BlockRewardType.Gem or BlockRewardType.LockedGem) {
                var gemUnlock = _chestRewardManager.GetChestReward(BlockRewardType.Gem);
                var gemLock = _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
                return gemLock + gemUnlock;
            } else {
                return _chestRewardManager.GetChestReward(rewardType);
            }
        }
    }
}