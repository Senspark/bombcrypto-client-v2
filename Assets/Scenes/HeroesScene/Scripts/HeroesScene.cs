using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Analytics;

using App;

using BomberLand.Inventory;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

using DG.Tweening;

using Engine.Entities;

using Game.Dialog;
using Game.Manager;

using Reconnect;
using Reconnect.Backend;
using Reconnect.View;

using Senspark;

using Services;

using Share.Scripts.Dialog;
using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.HeroesScene.Scripts {
    public class HeroesScene : MonoBehaviour {
        [SerializeField]
        protected Canvas canvasDialog;

        [SerializeField]
        private BLHeroesSelection itemList;

        [SerializeField]
        private BLEquipment equipment;

        [SerializeField]
        private BLHeroInfomation hero;

        [SerializeField]
        private UpgradeStatButton[] statButtons;

        [SerializeField]
        private Image splashFade;
        
        private IServerManager _serverManager;
        private IServerRequester _serverRequester;
        private ITRHeroManager _trHeroManager;
        private IStorageManager _storageManager;
        private IHeroStatsManager _heroStatsManager;
        private IProductItemManager _productItemManager;
        private IChestRewardManager _chestRewardManager;
        private IAnalytics _analytics;
        private IReconnectStrategy _reconnectStrategy;
        private ISoundManager _soundManager;

        private HeroGroupData _hero;
        private List<HeroGroupData> _itemHeroes;
        private Dictionary<int, StatData[]> _equipSkinStats;
        private ConfigUpgradeHeroData[] _upgradeConfig;
        private CrystalData[] _crystals;

        private void Awake() {
            splashFade.gameObject.SetActive(true);
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
            _trHeroManager = ServiceLocator.Instance.Resolve<ITRHeroManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _heroStatsManager = ServiceLocator.Instance.Resolve<IHeroStatsManager>();
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            itemList.SetOnSelectItem(ChooseItem);
            _reconnectStrategy = new DefaultReconnectStrategy(
                ServiceLocator.Instance.Resolve<ILogManager>(),
                new MainReconnectBackend(),
                LoadSceneReconnectView.ToCurrentScene(canvasDialog)
            );
            _reconnectStrategy.Start();
        }
        
        private void OnDestroy() {
            _reconnectStrategy.Dispose();
        }

        private void Start() {
            RefreshList(null);
        }

        private void RefreshList(PlayerData chooseHero) {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await LoadData(chooseHero);
                } catch (Exception e) {
                    DialogOK.ShowError(canvasDialog, e.Message);
                } finally {
                    waiting.End();
                    splashFade.DOFade(0.0f, 0.3f).OnComplete(() => { splashFade.gameObject.SetActive(false); });
                    await PreLoadData();
                }
            });
        }

        private async Task PreLoadData() {
            _crystals = await _serverRequester.GetCrystal();
            var config = await _serverRequester.GetUpgradeConfig();
            _upgradeConfig = config.Heroes;
        }

        private async Task LoadData(PlayerData chooseHero) {
            var result = await _trHeroManager.GetHeroesAsync("HERO");
            _itemHeroes = new List<HeroGroupData>();
            foreach (var iter in result) {
                var player = new PlayerData() {
                    itemId = iter.ItemId,
                    heroId = new HeroId(iter.InstanceId, HeroAccountType.Tr),
                    playerType = UIHeroData.ConvertFromHeroId(iter.ItemId),
                    playercolor = PlayerColor.HeroTr,
                };
                var heroes = new HeroGroupData() {
                    PlayerData = player,
                    Quantity = iter.Quantity,
                };
                var upgradeStats = new Dictionary<StatId, int> {
                    [StatId.Range] = iter.UpgradedRange, 
                    [StatId.Speed] = iter.UpgradedSpeed,
                    [StatId.Count] = iter.UpgradedBomb,
                    [StatId.Health] = iter.UpgradedHp,
                    [StatId.Damage] = iter.UpgradedDmg,
                };
                heroes.PlayerData.UpgradeStats = upgradeStats;
                var maxUpgradeStats = new Dictionary<StatId, int> {
                    [StatId.Range] = iter.MaxUpgradedRange, 
                    [StatId.Speed] = iter.MaxUpgradedSpeed,
                    [StatId.Count] = iter.MaxUpgradedBomb,
                    [StatId.Health] = iter.MaxUpgradedHp,
                    [StatId.Damage] = iter.MaxUpgradedDmg,
                };
                heroes.PlayerData.MaxUpgradeStats = maxUpgradeStats;
                _itemHeroes.Add(heroes);

                if (iter.IsActive) {
                    _storageManager.SelectedHeroKey = iter.InstanceId;
                }
            }

            itemList.LoadHeroes(_itemHeroes);
            if (_itemHeroes.Count > 0) {
                var heroId = GetLastPlayedHeroId();
                itemList.SetSelectedItem(heroId);
                if (chooseHero != null) {
                    itemList.SetSelectItem(chooseHero.heroId);
                } else {
                    itemList.SetSelectItem(heroId);
                }
            }
            await equipment.InitializeAsync(canvasDialog, OnReloadEquip, null, OnWingEquipped);
        }

        private HeroId GetLastPlayedHeroId() {
            var id = _storageManager.SelectedHeroKey;
            return FindHeroWith(id);
        }

        private HeroId FindHeroWith(int heroId) {
            foreach (var iter in _itemHeroes) {
                if (iter.PlayerData.heroId.Id == heroId) {
                    return iter.PlayerData.heroId;
                }
            }
            var firstId = _itemHeroes[0].PlayerData.heroId;
            _storageManager.SelectedHeroKey = firstId.Id;
            return firstId;
        }

        private void OnReloadEquip(Dictionary<int, StatData[]> skinStats) {
            _equipSkinStats = skinStats;
            hero.UpdateStatsFromSkinStats(skinStats);
        }

        private void OnWingEquipped(int itemId) {
            hero.ShowWing(itemId);
        }
        
        private void ChooseItem(int index, bool isSound) {
            if (isSound) {
                _soundManager.PlaySound(Audio.Tap);
            }
            _hero = _itemHeroes[index];
            hero.UpdateHero(_hero.PlayerData);
            hero.UpdateStatsFromSkinStats(_equipSkinStats);
            InitStatButtons();
        }

        private void InitStatButtons() {
            foreach (var button in statButtons) {
                var statId = button.StatId;
                var isHpDamage = statId is StatId.Health or StatId.Damage;
                var currentStat = GetCurrentStat(statId);
                var upgradeStats = _hero.PlayerData.UpgradeStats[statId];
                var maxUpgradeStats = _hero.PlayerData.MaxUpgradeStats[statId];
                int currentValue;
                if (isHpDamage) {
                    currentValue = currentStat.value + upgradeStats;
                } else {
                    currentValue = currentStat.max + upgradeStats;
                }
                button.SetActive(currentValue < maxUpgradeStats);
            }
        }

        private (int value, int max) GetCurrentStat(StatId statId) {
            var stats = _heroStatsManager.GetStats(_hero.PlayerData.itemId);
            foreach (var stat in stats) {
                if (stat.StatId == (int) statId) {
                    return (stat.Value, stat.Max);
                }
            }
            return (0, 0);
        }

        private (int value, int max) GetCurrentStatWithUpgrade(StatId statId) {
            var isHpDamage = statId is StatId.Health or StatId.Damage;
            var stats = _heroStatsManager.GetStats(_hero.PlayerData.itemId);
            foreach (var stat in stats) {
                if (stat.StatId == (int) statId) {
                    var value = isHpDamage ? stat.Value + _hero.PlayerData.UpgradeStats[statId] : stat.Value;
                    var max = isHpDamage ? stat.Max : stat.Max + _hero.PlayerData.UpgradeStats[statId];
                    return (value, max);
                }
            }
            return (0, 0);
        }

        public void OnBackButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            const string sceneName = "MainMenuScene";
            SceneLoader.LoadSceneAsync(sceneName).Forget();
        }

        public void OnSelectButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await _serverRequester.ActiveTRHero(_hero.PlayerData.heroId.Id);
                    _storageManager.SelectedHeroKey = _hero.PlayerData.heroId.Id;
                    itemList.SetSelectedItem(_hero.PlayerData.heroId);
                } catch (Exception e) {
                    DialogOK.ShowError(canvasDialog, e.Message);
                } finally {
                    waiting.End();
                }
            });
        }

        public void OnUpgradeButtonClicked(int id) {
            _soundManager.PlaySound(Audio.Tap);
            var statId = (StatId) id;
            var currentStat = GetCurrentStatWithUpgrade(statId);
            DialogUpgradeStat.Create().ContinueWith(dialog => {
                dialog.SetInfo(_hero, statId, currentStat,
                    _upgradeConfig, _crystals,
                    () => { DoUpgrade(statId, currentStat); });
                dialog.Show(canvasDialog);
            });
        }

        private void DoUpgrade(StatId statId, (int value, int max) currentStat) {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var upgradeHero = _hero.PlayerData;
                    await _serverRequester.UpgradeTRHero(upgradeHero.heroId.Id, GetUpgradeType(statId));
                    TrackUpgradeStat(upgradeHero, statId, currentStat);
                    await _serverManager.General.GetChestReward();
                    RefreshList(upgradeHero);
                    DialogOK.ShowInfo(canvasDialog, "Successfully");
                } catch (Exception e) {
                    DialogOK.ShowError(canvasDialog, e.Message);
                } finally {
                    waiting.End();
                }
            });
        }

        private void TrackUpgradeStat(PlayerData upgradeHero, StatId statId, (int value, int max) currentStat) {
            var isHpDamage = statId is StatId.Health or StatId.Damage;
            var heroId = upgradeHero.heroId.Id;
            var heroName = _productItemManager.GetItem(upgradeHero.itemId).Name;
            var statName = statId switch {
                StatId.Speed => "speed",
                StatId.Range => "fire",
                StatId.Count => "bomb",
                StatId.Health => "hp",
                StatId.Damage => "dmp"
            };
            var index = isHpDamage ? currentStat.value + 1 : upgradeHero.UpgradeStats[statId] + 1;

            // Cost
            var fee = GetFee(isHpDamage, index);
            if (fee == null) {
                throw new Exception($"Not found fee for upgrade {heroName} - stat {statName} - with index {index}");
            }

            var gold = fee.GoldFee;
            var gem = fee.GemFee;
            var lesser = 0;
            var rough = 0;
            var pure = 0;
            var perfect = 0;
            foreach (var iter in fee.Items) {
                var productId = (GachaChestProductId) iter.ItemID;
                switch (productId) {
                    case GachaChestProductId.LesserCrystal:
                        lesser = iter.Quantity;
                        break;
                    case GachaChestProductId.RoughCrystal:
                        rough = iter.Quantity;
                        break;
                    case GachaChestProductId.PureCrystal:
                        pure = iter.Quantity;
                        break;
                    case GachaChestProductId.PerfectCrystal:
                        perfect = iter.Quantity;
                        break;
                }
            }

            if (isHpDamage) {
                TrackUpgradeBaseIndex(heroId, heroName, statName, index,
                    lesser, rough, pure, perfect,
                    gold, gem);
            } else {
                TrackUpgradeMaxIndex(heroId, heroName, statName, index,
                    lesser, rough, pure, perfect,
                    gold, gem);
            }
        }

        private void TrackUpgradeBaseIndex(int heroId, string heroName, string index, int level,
            int lesser, int rough, int pure, int perfect,
            int gold, int gem) {
            var gems = GetLockUnlockGem(gem);
            _analytics.TrackUpgradeBaseIndex(heroId, heroName, index, level,
                lesser, rough, pure, perfect,
                gold, gems.gemLock, gems.gemUnlock);
        }

        private void TrackUpgradeMaxIndex(int heroId, string heroName, string index, int times,
            int lesser, int rough, int pure, int perfect,
            int gold, int gem) {
            var gems = GetLockUnlockGem(gem);
            _analytics.TrackUpgradeMaxIndex(heroId, heroName, index, times,
                lesser, rough, pure, perfect,
                gold, gems.gemLock, gems.gemUnlock);
        }

        private (int gemLock, int gemUnlock) GetLockUnlockGem(int gem) {
            var gemLock = 0;
            var gemUnlock = 0;
            var chestGemLock = (int) _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
            if (chestGemLock >= gem) {
                gemLock = gem;
                gemUnlock = 0;
            } else {
                gemLock = chestGemLock;
                gemUnlock = gem - gemLock;
            }

            return (gemLock, gemUnlock);
        }

        private Fee GetFee(bool isHpDamage, int index) {
            var upgradeType = isHpDamage ? "DMG_HP" : "SPEED_FIRE_BOMB";
            foreach (var iter in _upgradeConfig) {
                if (iter.UpgradeType != upgradeType) {
                    continue;
                }
                foreach (var fee in iter.Fees) {
                    if (fee.Index == index) {
                        return fee;
                    }
                }
            }
            return null;
        }

        private string GetUpgradeType(StatId statId) {
            return statId switch {
                StatId.Range => "RANGE",
                StatId.Speed => "SPEED",
                StatId.Count => "BOMB",
                StatId.Health => "HP",
                StatId.Damage => "DMG"
            };
        }
    }
}