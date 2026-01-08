using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLPvpMode.Data;
using Engine.Entities;

using Scenes.StoryModeScene.Scripts;

using Senspark;
using StoryMode.UI;
using UnityEngine;

namespace App {
    public interface IBossSkillDetails {
        float CoolDownShoot { get; }
        float CoolDownSpawn { get; }
        float CoolDownShootBoost { get; }
        float SpeedBoost { get; }
        float SpeedBoostTime { get; }
        float PercentHealthBoost { get; }
    }

    public class StoryModeStorageData {
        public readonly int[] ItemsTake = new int[Enum.GetValues(typeof(ItemType)).Length];
    }

    [Service(nameof(IStoryModeManager))]
    public interface IStoryModeManager : IService {
        StoryModeStorageData StorageData { get; }
        void SetTicketMode(StoryModeTicketType ticketType);
        float PlayForFunFee { get; }
        bool CheckValidTicket(bool chargeFee);
        Task<IStoryMapDetail> GetStoryMapDetail(int levelIndex, HeroId heroId);
        Task<IStoryMapDetail> GetAdventureMapDetail(int level, int stage, int versionMap, HeroId herId, int[] boosters, bool isPreview = false);
        Task<IStoryLevelDetail> GetLevelDetail();
        Task<IAdventureLevelDetail> GetAdventureLevelDetail();
        Task<IStoryExplodeResponse> StartExplode(IEnumerable<Vector2Int> brokenList);
        Task<IHeroDetails> HeroTakeDamage(HeroId heroId, int enemyId);
        Task<IEnemyDetails> EnemyTakeDamage(int enemyId, HeroId heroId);
        Task<IKillStoryEnemyResult> KillStoryEnemy(int enemyId);
        Task<IStoryModeEnterDoorResponse> EnterDoor();
        Task<ITakeItemResult> RequestTakeItem(Vector2Int location);
        Task UseAdventureBooster(int boosterId);
        Task<IEnemyDetails[]> SpawnEnemies(int skin);
        IBossSkillDetails StoryModeGetBossSkillDetails(int skin);
        Task StartPlaying(IStoryMapDetail storyMapDetail, IBossSkillDetails bossSkillDetails, BoosterStatus boosterStatus, bool isTesting = false);
        Task<IBonusRewardAdventureV2> GetBonusRewardAdventureV2(string rewardId, string adsId);
        Task<IBonusRewardAdventureV2> GetBonusRewardPvpV2(string rewardId, string adsId);
        Task<IAdventureReviveHero> ReviveAdventureHeroByAds(string adsToken);
        Task<IAdventureReviveHero> ReviveAdventureHeroByGems();

        Task EnterToLevelMenu(bool isTesting = false);
    }
}