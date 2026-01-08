using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Engine.Entities;

using Game.Dialog;

using PvpMode.Services;

using Scenes.FarmingScene.Scripts;

namespace Scenes.TreasureModeScene.Scripts.Mocks {
    public class NullPlayerStorageManager : IPlayerStorageManager {

        public NullPlayerStorageManager() {
            MapDatas = new MapData[35, 17];
            var d = new MapData(EntityType.Brick, 100, 100);
            for (var i = 0; i < 35; i++) {
                for (var j = 0; j < 17; j++) {
                    MapDatas[i, j] = d;
                }
            }
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public MapData[,] MapDatas { get; }
        public int TileSet { get; }

        public void LoadMap(GameModeType type) {
        }

        public void SetMapDetails(IMapDetails details) {
        }

        public void LoadPlayerFromServer(IHeroDetails[] bbmArr) {
        }
        
        public void LoadLockedHeroesFromServer(IHeroDetails[] bbmArr) {
        }

        public void AddHeroServer(IHeroDetails[] heroesTon) {
        }

        public void RefreshSortedData() {
        }

        public void RemoveBurnHeroes(HeroId[] lstHeroesIdBurn) {
        }

        public void UpdatePlayerHpFromServer(IHeroDetails detail) {
        }

        public void UpdateHeroSShield(HeroId id, List<IHeroSAbility> abilities) {
        }

        public void UpdateHeroActiveState(HeroId id, bool active, bool refreshSort = true) {
        }

        public void UpdateHeroState(HeroId id, HeroStage newState, bool refreshSort = true) {
        }

        public void UpdatePlayerPvpBattery(ISyncPvPHeroesResult pvpDetails) {
        }

        public void ForceUpdateHero(HeroId heroId, IHeroDetails abilities) {
        }

        public void SetTotalHeroesSize(int value) { ;
        }
        
        public void AdjustTotalHeroesSize(int value) { ;
        }
        
        public int GetTotalHeroesSize() {
            return 0;
        }
        
        public int GetPlayerCount() {
            return 0;
        }

        public int GetActivePlayersAmount() {
            return 0;
        }

        public int GetInMapPlayerCount() {
            return 0;
        }

        public int GetHomePlayerCount() {
            return 0;
        }

        public List<PlayerData> GetInHomePlayers() {
            throw new System.NotImplementedException();
        }

        public List<PlayerData> GetPlayerDataList(HeroAccountType type, params HeroAccountType[] types) {
            throw new System.NotImplementedException();
        }

        public (List<IHeroDetails>, int, List<int>) GetSortedPlayerDataList(DialogInventory.SortRarity orderRarity,
            int targetRarity, DialogInventory.SortOrder1 order1, DialogInventory.SortOrder2 order2,
            DialogInventory.HeroTypeFilter order3, DialogInventory.ActiveFilter filterActive, int[] selectedHeroIds,
            HeroId[] excludeHeroIds, int page,
            int itemsPage, HeroAccountType type, params HeroAccountType[] types) {
            throw new System.NotImplementedException();
        }
        
        public (List<PlayerData>, int, List<int>) GetSortedPlayerDataList(
            List<PlayerData> input,
            DialogInventory.SortRarity orderRarity,
            int targetRarity, 
            DialogInventory.SortOrder2 order2,
            DialogInventory.HeroTypeFilter order3, 
            int page, int itemsPage) {
            throw new System.NotImplementedException();
        }
        
        public List<PlayerData> GetSortedPlayerForSkinFusion(
            int targetRarity,
            PlayerType playerType,
            HeroId[] excludeHeroIds, 
            HeroAccountType type) {
            throw new System.NotImplementedException();
        }

        public List<PlayerData> GetActivePlayerData() {
            throw new System.NotImplementedException();
        }

        public List<PlayerData> GetInMapPlayerData() {
            return new List<PlayerData>();
        }

        public List<PlayerData> GetPlayerDataList(HeroId[] ids) {
            return new List<PlayerData>();
        }

        public PlayerData GetPlayerDataFromId(HeroId id) {
            throw new System.NotImplementedException();
        }

        public HeroRarity GetHeroRarity(PlayerData player) {
            return HeroRarity.Common;
        }

        public Dictionary<HeroAccountType, List<IHeroDetails>> GetLockedHeroesData() {
            throw new NotImplementedException();
        }

        public string GetHeroIndex(PlayerData player) {
            throw new System.NotImplementedException();
        }

        public string GetHeroAbility(PlayerData player) {
            throw new System.NotImplementedException();
        }

        public string GetBlockTrackingName(EntityType entityType) {
            throw new System.NotImplementedException();
        }

        public float SimulatePlayerHpOverTime(PlayerData player, float houseCharge) {
            throw new System.NotImplementedException();
        }
    }
}