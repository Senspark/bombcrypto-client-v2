using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using BLPvpMode.Engine.Entity;

using Engine.Entities;

using JetBrains.Annotations;

using PvpMode.Services;

using UnityEngine;

namespace Engine.Manager {
    public interface IPlayerManager {
        List<Player> Players { get; }
        HeroTakeDamageInfo TakeDamageInfo { get; }
        Player GetPlayerById(HeroId id);
        Player GetPlayerBySlot(int slot);
        Player GetPlayer(int slot = 0);
        bool SpawnBomb(int slot);
        bool CheckSpawnPvpBomb(int slot);
        int CountPlantedBomb(int slot);

        public Task FirstInitPlayerPVE(List<Vector2Int> locations);

        void SpawnPvpBomb(int slot, int id, int range, Vector2Int position);
        void ExplodePvpBomb(int slot, int id, [NotNull] Dictionary<Direction, int> ranges);
        void RemoveBomb(int slot, int id);

        void SetPlayerInJail(int slot);
        void AddItemToPlayer(bool playSound, int slot, HeroItem item, int value);
        void SetShieldToPlayer(int slot);
        void JailBreak(int slot);
        void SetSkullHeadToPlayer(bool playSound, int slot, HeroEffect effect, int duration);
        void PlayKillEffectOnOther(int slot);

        void RequestEnterDoor(int slot, Door door);
        void RequestTakeItem(Item item);
        void ExitPlayer(int slot);
        void OnAfterPlayerDie();
        void SaveHeroTakeDamageInfo(bool allowRevive, bool isAds, int gemsValue, Vector2Int lastPlayerLocation);
        void Revive(int slot, bool isTesting);
        void ShowHealthBarOnPlayerNotSlot(int slot);

        Task AddPlayer(Vector2Int locations, PlayerData playerData, int slot, bool isDangerous);
        public void RemoveHeroes(HeroId[] heroIds);

        void SetPropertiesAndAbility(Player player, PlayerData playerData, bool isDangerous);
        void UpdateProcess(int slot);

        void SetThunderStrike(HeroId id, bool value);
        bool CanPlaceBomb(HeroId id);
        PlayerData GetPlayerDataRaw(int slot);
        public int GetDicPlayersSlot(HeroId id);
        public int GetActivePlayerQuantity();

        public int GetAllActivePlayerQuantity();
        public void AddPendingActiveHeroes(HeroId heroId, bool isActive);
    }
}