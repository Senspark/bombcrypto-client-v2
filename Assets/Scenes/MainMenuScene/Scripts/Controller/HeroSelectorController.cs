using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App;

using Constant;

using Data;

using Engine.Entities;

using Game.Dialog;

using PvpMode.Services;

using Senspark;

using Services;

using UnityEngine;

using IBoosterManager = PvpMode.Manager.IBoosterManager;
using IServiceBoosterManager = Services.IBoosterManager;

namespace Scenes.MainMenuScene.Scripts.Controller {
    public class HeroSelectorController {
        public static int[] GetSelectedBooster() {
            var strBoosters = PlayerPrefs.GetString("SELECTED_BOOSTER_IDS", "");
            if (string.IsNullOrEmpty(strBoosters)) {
                return Array.Empty<int>();
            }
            var idBoosters = strBoosters.Split(",")
                .Select(s => {
                    var i = 0;
                    int.TryParse(s, out i);
                    return i;
                })
                .ToArray();

            return idBoosters;
        }

        public static void SaveSelectedBooster(int[] selectedBoosters) {
            var strBoosters = string.Join(",", selectedBoosters);
            PlayerPrefs.SetString("SELECTED_BOOSTER_IDS", strBoosters);
            PlayerPrefs.Save();
        }
        
        private IServerManager _serverManager;
        private ITRHeroManager _trHeroManager;
        private IBoosterManager _boosterManager;
        private IServiceBoosterManager _serviceBoosterManager;
        private IStorageManager _storageManager;

        public IPvpRankingItemResult CurrentRank { get; private set; }

        public List<HeroGroupData> PvpHeroes { get; private set; }

        public async Task Initialized() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _trHeroManager = ServiceLocator.Instance.Resolve<ITRHeroManager>();
            _boosterManager = ServiceLocator.Instance.Resolve<IBoosterManager>();
            _serviceBoosterManager = ServiceLocator.Instance.Resolve<IServiceBoosterManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();

            // GetPvpRanking
            var rankingResult = await _serverManager.Pvp.GetPvpRanking();
            CurrentRank = rankingResult.CurrentRank;

            // Get User Booster
            var boosterResult = await _serviceBoosterManager.GetPvPBoostersAsync();
            _boosterManager.SetBoosterData(boosterResult);

            // Get Heroes Tr
            await GetHeroTrFromServer();
        }

        public async Task GetHeroTrFromServer() {
            var result = await _trHeroManager.GetHeroesAsync("HERO");
            LoadHeroesFrom(result.ToArray());
        }

        public async Task<IPvpOtherUserInfo> GetOtherUserInfo(int userId, string userName) {
            return await _serverManager.Pvp.GetOtherUserInfo(userId, userName);
        }
        
        public PlayerData GetCurrentPvpHero() {
            var lastHeroId = GetLastPlayedHeroId();
            return GetPvpHero(lastHeroId);
        }

        public PlayerData GetPvpHero(int heroId) {
            foreach (var iter in PvpHeroes) {
                if (iter.PlayerData.heroId.Id == heroId) {
                    return iter.PlayerData;
                }
            }
            if (PvpHeroes.Count > 0) {
                var playerData = PvpHeroes[0].PlayerData;
                _storageManager.SelectedHeroKey = playerData.heroId.Id;
                return playerData;
            }
            return null;
        }

        private void LoadHeroesFrom(TRHeroData[] trHeroes) {
            PvpHeroes = new List<HeroGroupData>();
            foreach (var iter in trHeroes) {
                var player = new PlayerData() {
                    itemId = iter.ItemId,
                    heroId = new HeroId(iter.InstanceId, HeroAccountType.Tr),
                    playerType = UIHeroData.ConvertFromHeroId(iter.ItemId),
                    playercolor = PlayerColor.HeroTr
                };
                var inventoryHero = new HeroGroupData() {PlayerData = player, Quantity = iter.Quantity};
                var upgradeStats = new Dictionary<StatId, int>();
                upgradeStats[StatId.Range] = iter.UpgradedRange;
                upgradeStats[StatId.Speed] = iter.UpgradedSpeed;
                upgradeStats[StatId.Count] = iter.UpgradedBomb;
                upgradeStats[StatId.Health] = iter.UpgradedHp;
                upgradeStats[StatId.Damage] = iter.UpgradedDmg;
                inventoryHero.PlayerData.UpgradeStats = upgradeStats;
                PvpHeroes.Add(inventoryHero);

                if (iter.IsActive) {
                    _storageManager.SelectedHeroKey = iter.InstanceId;
                }
            }
        }

        private int GetLastPlayedHeroId() {
            var id = _storageManager.SelectedHeroKey;
            var heroID = id >= 0 ? id : PvpHeroes[0].PlayerData.heroId.Id;
            if (id < 0) {
                _storageManager.SelectedHeroKey = heroID;
            }
            return heroID;
        }
    }
}