using System;
using System.Collections.Generic;

using Engine.Entities;

using PvpMode.Services;

using Scenes.FarmingScene.Scripts;

using Senspark;

namespace App {
    [Service(nameof(IPlayerStorageManager))]
    public interface IPlayerStorageManager : IService {
        MapData[,] MapDatas { get; }
        int TileSet { get; }
        void LoadMap(GameModeType type);
        void SetMapDetails(IMapDetails details);
        
        /// <summary>
        /// Hàm này sẽ gỡ bỏ toàn bộ heroes và thêm vào danh sách mới
        /// </summary>
        /// <param name="bbmArr"></param>
        void LoadPlayerFromServer(IHeroDetails[] bbmArr);
        
        /// <summary>
        /// Hàm này sẽ gỡ bỏ toàn bộ locked heroes và thêm vào danh sách mới
        /// </summary>
        /// <param name="bbmArr"></param>
        void LoadLockedHeroesFromServer(IHeroDetails[] bbmArr);

        /// <summary>
        /// Hàm này xe thêm mới hero vào.
        /// </summary>
        /// <param name="heroesTon"></param>
        void AddHeroServer(IHeroDetails[] heroesTon);
        
        /// <summary>
        /// Hàm này gỡ bỏ burn hero ở client trước khi gửi lên server
        /// </summary>
        void RemoveBurnHeroes(HeroId[] lstHeroesIdBurn);
        
        void UpdatePlayerHpFromServer(IHeroDetails detail);
        void UpdateHeroSShield(HeroId id, List<IHeroSAbility> abilities);
        void UpdateHeroActiveState(HeroId id, bool active, bool refreshSort = true);
        void UpdateHeroState(HeroId id, HeroStage newState, bool refreshSort = true);
        void UpdatePlayerPvpBattery(ISyncPvPHeroesResult pvpDetails);
        public void ForceUpdateHero(HeroId heroId, IHeroDetails abilities);
        
        void SetTotalHeroesSize(int value);
        void AdjustTotalHeroesSize(int value);
        int GetTotalHeroesSize();
        int GetPlayerCount();
        int GetActivePlayersAmount();
        int GetInMapPlayerCount();
        int GetHomePlayerCount();
        List<PlayerData> GetInHomePlayers();
        List<PlayerData> GetPlayerDataList(HeroAccountType type, params HeroAccountType[] types);

        (List<IHeroDetails>, int, List<int>) GetSortedPlayerDataList(
            DialogInventory.SortRarity orderRarity,
            int targetRarity,
            DialogInventory.SortOrder1 order1,
            DialogInventory.SortOrder2 order2,
            DialogInventory.HeroTypeFilter order3,
            DialogInventory.ActiveFilter filterActive,
            int[] selectedHeroIds,
            HeroId[] excludeHeroIds,
            int page, int itemsPage,
            HeroAccountType type, params HeroAccountType[] types);

        (List<PlayerData>, int, List<int>) GetSortedPlayerDataList(
            List<PlayerData> input,
            DialogInventory.SortRarity orderRarity,
            int targetRarity,
            DialogInventory.SortOrder2 order2,
            DialogInventory.HeroTypeFilter order3,
            int pageIndex, int itemsPage);
        
        List<PlayerData> GetSortedPlayerForSkinFusion(
            int targetRarity,
            PlayerType playerType,
            HeroId[] excludeHeroIds,
            HeroAccountType type);
        
        List<PlayerData> GetActivePlayerData();
        List<PlayerData> GetInMapPlayerData();
        List<PlayerData> GetPlayerDataList(HeroId[] ids);
        PlayerData GetPlayerDataFromId(HeroId id);
        HeroRarity GetHeroRarity(PlayerData player);
        public Dictionary<HeroAccountType, List<IHeroDetails>> GetLockedHeroesData();
        string GetHeroIndex(PlayerData player);
        string GetHeroAbility(PlayerData player);
        string GetBlockTrackingName(EntityType entityType);
        
        float SimulatePlayerHpOverTime(PlayerData player, float houseCharge);
    }

    public readonly struct HeroId : IComparable {
        public readonly int Id;
        public readonly HeroAccountType Type;

        public static HeroId NullId = new(-1, HeroAccountType.Trial);

        public HeroId(int id, HeroAccountType type) {
            Id = id;
            Type = type;
        }

        public bool IsInvalid() {
            return Id < 0;
        }

        public bool IsValid() {
            return Id >= 0;
        }

        public static bool operator ==(HeroId c1, HeroId c2) {
            return c1.Equals(c2);
        }

        public static bool operator !=(HeroId c1, HeroId c2) {
            return !c1.Equals(c2);
        }
        
        public bool Equals(HeroId other) {
            if (other.Id < 0 && Id < 0) {
                return true;
            }
            return Id == other.Id && Type == other.Type;
        }

        public override bool Equals(object obj) {
            return obj is HeroId other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Id, (int) Type);
        }

        public int CompareTo(object obj) {
            if (obj == null) {
                return 1;
            }

            var o = (HeroId) obj;
            return Id.CompareTo(o.Id);
        }
    }
}