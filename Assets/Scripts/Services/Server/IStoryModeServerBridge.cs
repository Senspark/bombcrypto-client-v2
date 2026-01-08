using System.Collections.Generic;
using System.Threading.Tasks;
using Constant;
using Data;
using UnityEngine;

namespace App {
    public interface IStoryMapDetail {
        int Stage { get; }
        int Level { get; }
        int Row { get; }
        int Col { get; }
        IHeroDetails Hero { get; }
        Vector2Int[] Positions { get; } //danh sách vị trí các soft block
        Vector2Int Door { get; } //vị trí của door (điều kiện vị trí này là vị trí của 1 soft block)
        IEnemyDetails[] Enemies { get; } //danh sách các enemies trong level. (tham khảo levelStrategy để tạo danh sách này).
        IAdventureItem[] Items { get; }
        EquipmentData[] Equipments { get; set; }
        Dictionary<StatId, int> MaximumStats { get; set; }
    }

    public interface IStoryLevelDetail {
        int CurrentLevel { get; }
        int MaxLevel { get; }
        bool IsNew { get; }
        int HeroId { get; }
        (long Id, int RemainingSeconds)[] PlayedBombers { get; }
        (float FirstWin, float Replay)[] Rewards { get; }
    }

    public interface ILevelMapDetail {
        int Stage { get; }
        int LevelCount { get; }
    }

    public interface IAdventureLevelDetail {
        bool IsNew { get; }
        int MaxLevel { get; }
        int CurrentLevel { get; }
        int CurrentStage { get; }
        int MaxStage { get; }
        int HeroId { get; }
        List<ILevelMapDetail> LevelMaps { get; }
    }

    public interface IAdventureItem {
        int X { get; }
        int Y { get; }
        int Type { get; }
        int RewardValue { get; }
    }

    public interface IStoryExplodeResponse {
        Vector2Int[] BrokenLocation { get; }
        IEnemyDetails[] EnemiesFromDoor { get; }
        Vector2Int[] ItemsRemoved { get; }
    }

    public interface IEnemyDetails {
        int Id { get; }
        int Skin { get; }
        float Damage { get; }
        float Speed { get; }
        bool Follow { get; }
        float Hp { get; }
        float MaxHp { get; }
        bool ThroughBrick { get; }
        int BombSkin { get; }
        long Timestamp { get; }
        int GoldReceive { get; }
        int BombRange { get; }
        Vector2Int PosSpawn { get; set; } // ignore if value is  (-1, -1)
    }

    public interface IKillStoryEnemyResult {
        IEnemyDetails Enemy { get; }
    }

    public interface IWinReward {
        string RewardName { get; }
        int Value { get; }
        bool OutOfSlot { get; }
    }

    public interface IStoryModeEnterDoorResponse {
        public string RewardId { get; }
        public IWinReward[] WinRewards { get; }
        float Rewards { get; }
        bool IsStageCompleted { get; }
        float TimeCompleted { get; }
    }

    public interface ITakeItemResult {
        IAdventureItem Item { get; }
    }

    public interface IBonusRewardAdventure {
        string RewardName { get; }
        int Value { get; }
    }

    public interface IBonusRewardAdventureV2Item {
        int ItemId { get; }
        int Quantity { get; }
    }

    public interface IBonusRewardAdventureV2 {
        string RewardCode { get; }
        IBonusRewardAdventureV2Item[] Items { get; }
    }

    public interface IAdventureReviveGem {
        string GemType { get; }
        int Value { get; }
    }

    public interface IAdventureReviveHero {
        int ReviveTimes { get; }
        int Hp { get; }
        IAdventureReviveGem[] GemUsed { get; }
    }

    public interface IStoryModeServerBridge : IServerManagerDelegate {
        Task<IStoryMapDetail> StoryModeGetMapDetail(int level, HeroId heroId, StoryModeTicketType ticketType);
        Task<IStoryMapDetail> AdventureGetMapDetail(int level, int stage, int versionMap, HeroId heroId, int[] boosters, bool isPreview = false);
        Task<IStoryLevelDetail> StoryModeGetLevelDetail();
        Task<IAdventureLevelDetail> AdventureGetLevelDetail();
        Task<IStoryExplodeResponse> StoryModeStartExplode(IEnumerable<Vector2Int> brokenList);
        Task<IHeroDetails> StoryModeHeroTakeDamage(HeroId heroId, int enemyId);
        Task<IEnemyDetails> StoryModeEnemyTakeDamage(int enemyId, HeroId heroId);
        Task<IKillStoryEnemyResult> StoryModeKillEnemy(int enemyId);
        Task<IStoryModeEnterDoorResponse> StoryModeEnterDoor();
        Task<IEnemyDetails[]> StoryModeSpawnEnemies(int skin);
        Task<ITakeItemResult> RequestTakeItem(Vector2Int location);
        Task UseStoryBooster(int boosterId);
        Task<IBonusRewardAdventureV2> GetBonusRewardAdventure(string rewardId, string adsId);
        Task<IBonusRewardAdventureV2> GetBonusRewardPvp(string rewardId, string adsId);
        Task<IAdventureReviveHero> ReviveAdventureHeroByAds(string adsToken);
        Task<IAdventureReviveHero> ReviveAdventureHeroByGems();

        IEnemyDetails CreateDemoEnemy(int id, int skin, float damage, float speed, bool follow, float hp, float maxHp,
            bool throughBrick, int bombSkin, long timestamp);
    }
}