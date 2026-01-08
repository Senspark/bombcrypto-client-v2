using App;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

using Game.Dialog;

using Senspark;

using Services;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.HeroesScene.Scripts {
    public class DialogUpgradeStat : Dialog {
        [SerializeField]
        private Text title;

        [SerializeField]
        private Text capacityTitle;

        [SerializeField]
        private Image[] statIcons;

        [SerializeField]
        private Sprite[] statSprites;

        [SerializeField]
        private Text capacityFromText;

        [SerializeField]
        private Text capacityToText;

        [SerializeField]
        private UpgradeStatFee[] fees;

        private IHeroStatsManager _heroStatsManager;
        private IChestRewardManager _chestRewardManager;
        private ISoundManager _soundManager;

        private HeroGroupData _hero;
        private StatId _statId;
        private CrystalData[] _crystals;
        
        private System.Action _onUpgradeCallback;

        private bool _notEnoughCrystal;
        private bool _notEnoughGold;
        private bool _notEnoughGem;

        private bool _isClicked;
        
        public static UniTask<DialogUpgradeStat> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogUpgradeStat>();
        }
        
        protected override void Awake() {
            base.Awake();
            _heroStatsManager = ServiceLocator.Instance.Resolve<IHeroStatsManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public void SetInfo(HeroGroupData hero, StatId statId, 
            (int value, int max) currentStat,
            ConfigUpgradeHeroData[] upgradeConfig,
            CrystalData[] crystals,
            System.Action callback) {
            _hero = hero;
            _statId = statId;
            _crystals = crystals;
            _onUpgradeCallback = callback;
            var statName = GetStatName();
            title.text = $"{statName.ToUpper()} UPGRADE";
            capacityTitle.text = _statId < StatId.Health ? $"{statName} Capacity" : $"Max {statName}";
            var sprite = statSprites[(int) _statId];
            foreach (var icon in statIcons) {
                icon.sprite = sprite;
            }

            // Capacity
            var isHpDamage = statId is StatId.Health or StatId.Damage;
            var capacityFrom = isHpDamage ? currentStat.value : currentStat.max;
            var capacityTo = capacityFrom + 1;
            var prefix = isHpDamage ? "+" : "Max ";
            capacityFromText.text = $"{prefix}{capacityFrom}";
            capacityToText.text = $"{prefix}{capacityTo}";

            // Cost
            int feeIndex;
            if (isHpDamage) {
                feeIndex = capacityTo;
            } else {
                feeIndex = hero.PlayerData.UpgradeStats[_statId] + 1;
            }

            var upgradeType = isHpDamage ? "DMG_HP" : "SPEED_FIRE_BOMB";
            foreach (var iter in upgradeConfig) {
                if (iter.UpgradeType != upgradeType) {
                    continue;
                }
                UpdateWithFees(iter.Fees, feeIndex);
                break;
            }
        }

        private void UpdateWithFees(Fee[] feesConfig, int index) {
            foreach (var fee in feesConfig) {
                if (fee.Index == index) {
                    UpdateUiFee(fee);
                }
            }
        }
        
        private void UpdateUiFee(Fee fee) {
            var gold = _chestRewardManager.GetChestReward(BlockRewardType.BLGold);
            var gemUnlock = _chestRewardManager.GetChestReward(BlockRewardType.Gem);
            var gemLock = _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
            var gem = gemLock + gemUnlock;
            GetUiFee(GachaChestProductId.Gold).SetValue(fee.GoldFee, (int)gold);
            GetUiFee(GachaChestProductId.RewardGem).SetValue(fee.GemFee, (int)gem);
            _notEnoughGold = gold < fee.GoldFee;
            _notEnoughGem = gem < fee.GemFee;
            _notEnoughCrystal = false;
            foreach (var iter in fee.Items) {
                var quantity = GetCrystalQuantity(iter.ItemID);
                var productId = (GachaChestProductId) iter.ItemID;
                GetUiFee(productId).SetValue(iter.Quantity, quantity);
                if (!_notEnoughCrystal) {
                    _notEnoughCrystal = quantity < iter.Quantity;
                }
            }
        }

        private int GetCrystalQuantity(int itemId) {
            foreach (var iter in _crystals) {
                if (itemId == iter.ItemId) {
                    return iter.Quantity;
                }
            }
            return 0;
        }
        
        private UpgradeStatFee GetUiFee(GachaChestProductId productId) {
            foreach (var iter in fees) {
                if (iter.ProductId == productId) {
                    return iter;
                }
            }
            return null;
        }
        
        private string GetStatName() {
            var statName = _statId switch {
                StatId.Range => "Range",
                StatId.Speed => "Speed",
                StatId.Count => "Bomb",
                StatId.Health => "HP",
                StatId.Damage => "Damage"
            };
            return statName;
        }
        
        protected override void OnYesClick() {
            if(_isClicked)
                return;
            _isClicked = true;
            OnUpgradeButtonClicked();
        }

        public void OnUpgradeButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (_notEnoughCrystal) {
                DialogNotificationToAltar.ShowOn(DialogCanvas, null, ()=>{_isClicked = false;});
                return;
            }
            if (_notEnoughGold) {
                DialogNotificationToShop.ShowOn(DialogCanvas, DialogNotificationToShop.Reason.NotEnoughGold,
                    null,
                    ()=>{_isClicked = false;});
                return;
            }
            if (_notEnoughGem) {
                DialogNotificationToShop.ShowOn(DialogCanvas, DialogNotificationToShop.Reason.NotEnoughGem,
                    null,
                    ()=>{_isClicked = false;});
                return;
            }
            _onUpgradeCallback();
            Hide();
        }
        
        public void OnCancelButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}