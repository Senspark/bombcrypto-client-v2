using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLPvpMode.Data;
using Senspark;
using Game.UI;
using GroupMainMenu;

using Scenes.AdventureMenuScene.Scripts;
using Scenes.MainMenuScene.Scripts;
using Scenes.StoryModeScene.Scripts;

using Share.Scripts.Utils;

using StoryMode.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace App {
    public class DefaultStoryModeManager : IStoryModeManager {
        private class BossSkillDetails : IBossSkillDetails {
            public float CoolDownShoot { get; }
            public float CoolDownSpawn { get; }
            public float CoolDownShootBoost { get; }
            public float SpeedBoost { get; }
            public float SpeedBoostTime { get; }
            public float PercentHealthBoost { get; }

            public BossSkillDetails(
                float coolDownShoot,
                float coolDownSpawn,
                float coolDownShootBoost,
                float speedBoost,
                float speedBoostTime,
                float percentHealthBoost) {
                CoolDownShoot = coolDownShoot;
                CoolDownSpawn = coolDownSpawn;
                CoolDownShootBoost = coolDownShootBoost;
                SpeedBoost = speedBoost;
                SpeedBoostTime = speedBoostTime;
                PercentHealthBoost = percentHealthBoost;
            }
        }

        public float PlayForFunFee => 0.1f;

        private readonly IStorageManager _storeManager;
        private readonly IPlayerStorageManager _playerStoreManager;
        private readonly ILanguageManager _languageManager;
        private readonly IServerManager _serverManager;
        private readonly IChestRewardManager _chestRewardManager;

        private StoryModeTicketType _ticketType;

        public DefaultStoryModeManager(
            IStorageManager storeManager,
            IPlayerStorageManager playerStoreManager,
            ILanguageManager languageManager,
            IServerManager serverManager,
            IChestRewardManager chestRewardManager) {
            _storeManager = storeManager;
            _playerStoreManager = playerStoreManager;
            _languageManager = languageManager;
            _serverManager = serverManager;
            _chestRewardManager = chestRewardManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public StoryModeStorageData StorageData { get; private set; }

        public void SetTicketMode(StoryModeTicketType ticketType) {
            _ticketType = ticketType;
        }

        public bool CheckValidTicket(bool chargeFee) {
            return CheckValidTicket(_ticketType, chargeFee);
        }

        public async Task<IStoryMapDetail> GetStoryMapDetail(int levelIndex, HeroId heroId) {
            return await _serverManager.StoryMode.StoryModeGetMapDetail(levelIndex, heroId, _ticketType);
        }

        public async Task<IStoryMapDetail> GetAdventureMapDetail(int level, int stage, int versionMap, HeroId heroId, int[] boosters, bool isPreview = false) {
            return await _serverManager.StoryMode.AdventureGetMapDetail(level, stage, versionMap, heroId, boosters, isPreview);
        }

        public Task<IStoryLevelDetail> GetLevelDetail() {
            return _serverManager.StoryMode.StoryModeGetLevelDetail();
        }

        public Task<IAdventureLevelDetail> GetAdventureLevelDetail() {
            return _serverManager.StoryMode.AdventureGetLevelDetail();
        }

        public Task<IStoryExplodeResponse> StartExplode(IEnumerable<Vector2Int> brokenList) {
            return _serverManager.StoryMode.StoryModeStartExplode(brokenList);
        }

        public Task<IHeroDetails> HeroTakeDamage(HeroId heroId, int enemyId) {
            return _serverManager.StoryMode.StoryModeHeroTakeDamage(heroId, enemyId);
        }

        public Task<IEnemyDetails> EnemyTakeDamage(int enemyId, HeroId heroId) {
            return _serverManager.StoryMode.StoryModeEnemyTakeDamage(enemyId, heroId);
        }

        public Task<IKillStoryEnemyResult> KillStoryEnemy(int enemyId) {
            return _serverManager.StoryMode.StoryModeKillEnemy(enemyId);
        }

        public Task<IStoryModeEnterDoorResponse> EnterDoor() {
            return _serverManager.StoryMode.StoryModeEnterDoor();
        }

        public Task<IEnemyDetails[]> SpawnEnemies(int skin) {
            return _serverManager.StoryMode.StoryModeSpawnEnemies(skin);
        }

        public async Task<ITakeItemResult> RequestTakeItem(Vector2Int location) {
            var result = await _serverManager.StoryMode.RequestTakeItem(location);
            StorageData.ItemsTake[result.Item.Type]++;
            return result;
        }

        public Task UseAdventureBooster(int boosterId) {
            return _serverManager.StoryMode.UseStoryBooster(boosterId);
        }

        public IBossSkillDetails StoryModeGetBossSkillDetails(int skin) {
            return skin switch {
                4 => new BossSkillDetails(6, 0, 0, 0, 0, 0),
                8 => new BossSkillDetails(7, 15, 5, 4, 0, 0.3f),
                12 => new BossSkillDetails(10, 0, 0, 0, 0, 0),
                16 => new BossSkillDetails(0, 30, 0, 8, 2, 0),
                20 => new BossSkillDetails(5, 5, 0, 0, 0, 0),
                24 => new BossSkillDetails(10, 15, 0, 0, 0, 0),
                28 => new BossSkillDetails(10, 10, 0, 0, 0, 0),
                32 => new BossSkillDetails(0, 5, 0, 0, 0, 0),
                37 => new BossSkillDetails(3, 0, 0, 0, 0, 0),
                _ => new BossSkillDetails(0, 0, 0, 0, 0, 0)
            };
        }

        public async Task StartPlaying(IStoryMapDetail storyMapDetail,
            IBossSkillDetails bossSkillDetails, BoosterStatus boosterStatus, bool isTesting) {
            StorageData = new StoryModeStorageData();

            void OnLoaded(GameObject obj) {
                var storyMode = obj.GetComponent<LevelSceneStoryMode>();
                storyMode.SetStoryMapDetails(storyMapDetail.Hero, storyMapDetail, bossSkillDetails, _ticketType,
                    boosterStatus, isTesting);
            }
            const string sceneName = "StoryModeScene";
            await SceneLoader.LoadSceneAsync(sceneName, OnLoaded);
        }

        public async Task<IBonusRewardAdventureV2> GetBonusRewardAdventureV2(string rewardId, string adsId) {
            var result = await _serverManager.StoryMode.GetBonusRewardAdventure(rewardId, adsId);
            return result;
        }

        public async Task<IBonusRewardAdventureV2> GetBonusRewardPvpV2(string rewardId, string adsId) {
            var result = await _serverManager.StoryMode.GetBonusRewardPvp(rewardId, adsId);
            return result;
        }

        public async Task<IAdventureReviveHero> ReviveAdventureHeroByAds(string adsToken) {
            var result = await _serverManager.StoryMode.ReviveAdventureHeroByAds(adsToken);
            return result;
        }

        public async Task<IAdventureReviveHero> ReviveAdventureHeroByGems() {
            var result = await _serverManager.StoryMode.ReviveAdventureHeroByGems();
            return result;
        }

        public async Task EnterToLevelMenu(bool isTesting) {
            if (isTesting) {
                void OnLoaded(GameObject obj) {
                    var mainMenu = obj.GetComponent<MainMenuScene>();
                    mainMenu.ShowEquip();
                }
                await SceneLoader.LoadSceneAsync("MainMenuScene", OnLoaded);
            } else {
                await SceneLoader.LoadSceneAsync("AdventureMenuScene");
            }
        }

        private bool CheckValidTicket(StoryModeTicketType ticketType, bool chargeFee) {
            if (ticketType == StoryModeTicketType.PlayToEarn) {
                var keys = _chestRewardManager.GetChestReward(BlockRewardType.Key);
                if (keys == 0) {
                    throw new Exception(_languageManager.GetValue(LocalizeKey.info_not_enough_key));
                }
                if (chargeFee) {
                    _chestRewardManager.SetChestReward(BlockRewardType.Key, Math.Max(0, keys - 1));
                }
            } else if (ticketType == StoryModeTicketType.PlayForFun) {
                var chestCoins = _chestRewardManager.GetChestReward(BlockRewardType.BCoin);
                if (chestCoins < PlayForFunFee) {
                    throw new Exception(_languageManager.GetValue(LocalizeKey.info_not_enough_resource));
                }
                if (chargeFee) {
                    _chestRewardManager.SetChestReward(BlockRewardType.BCoin, Mathf.Max(0, (float)chestCoins - PlayForFunFee));
                }
            } else if (ticketType == StoryModeTicketType.BossHunter) {
                var ticket = _chestRewardManager.GetChestReward(BlockRewardType.BossTicket);
                if (ticket == 0) {
                    throw new Exception(_languageManager.GetValue(LocalizeKey.info_not_enough_resource));
                }
                if (chargeFee) {
                    _chestRewardManager.SetChestReward(BlockRewardType.BossTicket, Math.Max(0, ticket - 1));
                }
            }
            return true;
        }
    }
}