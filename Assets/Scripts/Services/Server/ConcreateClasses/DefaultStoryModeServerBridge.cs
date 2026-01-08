using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics;
using BLPvpMode.Manager.Api;
using Constant;
using CustomSmartFox.SolCommands;
using Senspark;
using Sfs2X.Entities.Data;
using UnityEngine;
using Utils;

namespace App {
    public partial class DefaultStoryModeServerBridge : IStoryModeServerBridge {
        private readonly Lazy<IAnalytics> _analytics = new(ServiceLocator.Instance.Resolve<IAnalytics>);
        private readonly IServerDispatcher _serverDispatcher;
        private readonly IServerManager _serverManager;

        public DefaultStoryModeServerBridge(IServerDispatcher serverDispatcher, IServerManager serverManager) {
            _serverDispatcher = serverDispatcher;
            _serverManager = serverManager;
        }

        public async Task<IStoryMapDetail> StoryModeGetMapDetail(int level, HeroId heroId,
            StoryModeTicketType ticketType) {
            var data = new SFSObject().Apply(it => {
                var s = SFSDefine.GetSlogans(EntryPoint.GetStoryMap);
                it.PutInt("level", level);
                it.PutLong("hero_id", heroId.Id);
                it.PutInt("ticket_type", (int) ticketType);
                it.PutUtfString(SFSDefine.SFSField.SLOGAN, s);
            });

            var response = await _serverDispatcher.SendCmd(new CmdGetStoryMap(data));
            return OnAdventureGetMapDetail(response);
        }

        private IStoryMapDetail OnAdventureGetMapDetail(ISFSObject data) {
            var result = new StoryMapDetail(data);
            return result;
        }
        
        public async Task<IStoryMapDetail> AdventureGetMapDetail(int level, int stage, int versionMap, HeroId heroId,
            int[] boosters, bool isPreview = false) {
            var data = new SFSObject().Apply(it => {
                var s = SFSDefine.GetSlogans(EntryPoint.GetStoryMap);
                it.PutIntArray("boosters", boosters);
                it.PutInt("level", level);
                it.PutInt("stage", stage);
                it.PutInt("version", versionMap);
                it.PutInt("hero_id", heroId.Id);
                it.PutBool("is_preview", isPreview);
                it.PutUtfString(SFSDefine.SFSField.SLOGAN, s);
            });
            var response = await _serverDispatcher.SendCmd(new CmdGetAdventureMap(data));
            
            foreach (var booster in boosters.Where(it =>
                         (GachaChestProductId) it is not (GachaChestProductId.Shield or GachaChestProductId.Key))) {
                _analytics.Value.TrackConversion(ConversionConvert.ConvertUsedBoosterPve(booster));
            }
            return OnStoryModeGetMapDetail(response);
        }
        
        private IStoryMapDetail OnStoryModeGetMapDetail(ISFSObject data) {
            var result = new StoryMapDetail(data);
            return result;
        }
        
        public async Task<IStoryLevelDetail> StoryModeGetLevelDetail() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetStoryLevelDetail(data));
            return OnStoryModeGetLevelDetail(response);
        }

        private IStoryLevelDetail OnStoryModeGetLevelDetail(ISFSObject data) {
            var playedBomberList = new List<(long Id, int remainingSeconds)>();
            var playedBombers = data.GetSFSArray("played_bombers");
            for (var i = 0; i < playedBombers.Count; i++) {
                var playedBomber = playedBombers.GetSFSObject(i);
                playedBomberList.Add(
                    (playedBomber.GetLong("id"), (int) (playedBomber.GetLong("remaining_time")) / 1000));
            }
            var rewardList = new List<(float FirstWin, float Replay)>();
            var rewards = data.GetSFSArray("level_rewards");
            for (var i = 0; i < rewards.Count; i++) {
                var reward = rewards.GetSFSObject(i);
                rewardList.Add((reward.GetFloat("first_win"), reward.GetFloat("replay")));
            }
            var result = new StoryLevelDetail {
                CurrentLevel = data.GetInt("current_level"),
                MaxLevel = data.GetInt("max_level"),
                IsNew = data.GetBool("is_new"),
                HeroId = (int) data.GetLong("hero_id"),
                PlayedBombers = playedBomberList.ToArray(),
                Rewards = rewardList.ToArray()
            };
            return result;
        }
        
        public async Task<IAdventureLevelDetail> AdventureGetLevelDetail() {
            var data = new SFSObject();
            var response = await _serverDispatcher.SendCmd(new CmdGetAdventureLevelDetail(data));
            return OnAdventureGetLevelDetail(response);
        }
        
        private IAdventureLevelDetail OnAdventureGetLevelDetail(ISFSObject data) {
            var levelMapsArray = data.GetSFSArray("level_map");
            var levelMaps = new List<ILevelMapDetail>();
            for (var i = 0; i < levelMapsArray.Count; i++) {
                var lm = levelMapsArray.GetSFSObject(i);
                levelMaps.Add(new LevelMapDetail() {
                    Stage = lm.GetInt("stage"),
                    LevelCount = lm.GetInt("level_count")
                });
            }
            var result = new AdventureLevelDetail() {
                IsNew = data.GetBool("is_new"),
                MaxLevel = data.GetInt("max_level"),
                CurrentLevel = data.GetInt("current_level"),
                CurrentStage = data.GetInt("current_stage"),
                MaxStage = data.GetInt("max_stage"),
                HeroId = data.GetInt("hero_id"),
                LevelMaps = levelMaps
            };
            return result;
        }
        
        public async Task<IStoryExplodeResponse> StoryModeStartExplode(IEnumerable<Vector2Int> brokenList) {
            var data = new SFSObject().Apply(it => {
                var bombId = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                ISFSArray blocks = new SFSArray();
                foreach (var vector in brokenList) {
                    ISFSObject block = new SFSObject();
                    block.PutInt("i", vector.x);
                    block.PutInt("j", vector.y);
                    blocks.AddSFSObject(block);
                }
                it.PutLong("bombId", bombId);
                it.PutSFSArray(SFSDefine.SFSField.BLocks, blocks);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdStartStoryExplode(data));
            return OnStoryModeStartExplode(response);
        }

        private IStoryExplodeResponse OnStoryModeStartExplode(ISFSObject data) {
            var bombId = data.GetLong("bombId");
            var blocksData = data.GetSFSArray(SFSDefine.SFSField.BLocks);
            var blocks = Array.Empty<Vector2Int>();
            if (blocksData != null) {
                blocks = new Vector2Int[blocksData.Count];
                for (var i = 0; i < blocksData.Count; i++) {
                    var blockData = blocksData.GetSFSObject(i);
                    blocks[i] = new Vector2Int(blockData.GetInt("i"), blockData.GetInt("j"));
                }
            }
            var enemiesData = data.GetSFSArray("enemies");
            IEnemyDetails[] enemies = null;
            if (enemiesData != null) {
                enemies = new IEnemyDetails[enemiesData.Count];
                for (var i = 0; i < enemiesData.Count; i++) {
                    enemies[i] = EnemyDetails.Parse(enemiesData.GetSFSObject(i));
                }
            }

            var itemsRemovedData = data.GetSFSArray("itemsRemoved");
            var itemsRemoved = Array.Empty<Vector2Int>();
            if (itemsRemovedData != null) {
                itemsRemoved = new Vector2Int[itemsRemovedData.Count];
                for (var i = 0; i < itemsRemovedData.Count; i++) {
                    var blockData = itemsRemovedData.GetSFSObject(i);
                    itemsRemoved[i] = new Vector2Int(blockData.GetInt("i"), blockData.GetInt("j"));
                }
            }
            var result = new StoryExplodeResponse {
                BrokenLocation = blocks,
                EnemiesFromDoor = enemies,
                ItemsRemoved = itemsRemoved
            };
            return result;
        }
        
        public async Task<IHeroDetails> StoryModeHeroTakeDamage(HeroId heroId, int enemyId) {
            var data = new SFSObject().Apply(it => {
                var timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                it.PutLong("timestamp", timestamp);
                it.PutLong("hero_id", heroId.Id);
                it.PutInt("enemy_id", enemyId);
            });

            var response = await _serverDispatcher.SendCmd(new CmdHeroTakeDamage(data));
            return OnStoryModeHeroTakeDamage(response);
        }
        
        private IHeroDetails OnStoryModeHeroTakeDamage(ISFSObject data) {
            var timestamp = data.GetLong("timestamp");
            var result = new StoryModeHeroDetails {
                Id = (int) data.GetLong("hero_id"),
                StoryHp = data.GetInt("hp"),
                TimeSync = timestamp,
                AllowRevive = data.GetBool("allow_revive"),
                AllowReviveByAds = data.GetBool("allow_revive_by_ads"),
                ReviveGemAmount = data.GetInt("revive_gem_amount")
            };
            return result;
        }
        
        public async Task<IEnemyDetails> StoryModeEnemyTakeDamage(int enemyId, HeroId heroId) {
            var data = new SFSObject().Apply(it => {
                var timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                timestamp = (timestamp * 1000) + enemyId;
                it.PutLong("timestamp", timestamp);
                it.PutLong("hero_id", heroId.Id);
                it.PutInt("enemy_id", enemyId);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdEnemyTakeDamage(data));
            return OnStoryModeEnemyTakeDamage(response);
        }
        
        private IEnemyDetails OnStoryModeEnemyTakeDamage(ISFSObject data) {
            var result = EnemyDetails.Parse(data);
            return result;
        }
        
        public async Task<IKillStoryEnemyResult> StoryModeKillEnemy(int enemyId) {
            var data = new SFSObject().Apply(it => {
                var timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                it.PutLong("timestamp", timestamp);
                it.PutInt("enemy_id", enemyId);
            });

            var response = await _serverDispatcher.SendCmd(new CmdKillStoryEnemy(data));
            return OnStoryModeKillEnemy(response);
        }
        
        private IKillStoryEnemyResult OnStoryModeKillEnemy(ISFSObject data) {
            var result = new KillStoryEnemyResult(data);
            return result;
        }
        
        public async Task<IStoryModeEnterDoorResponse> StoryModeEnterDoor() {
            var data = new SFSObject().Apply(it => {
                var s = SFSDefine.GetSlogans(EntryPoint.EnterStoryDoor);
                it.PutUtfString(SFSDefine.SFSField.SLOGAN, s);
            });
            var response = await _serverDispatcher.SendCmd(new CmdEnterAdventureDoor(data));
            return OnStoryModeEnterDoor(response);
        }
        
        private IStoryModeEnterDoorResponse OnStoryModeEnterDoor(ISFSObject data) {
            var result = new StoryModeEnterDoorResponse(data);
            return result;
        }
        
        public async Task<IEnemyDetails[]> StoryModeSpawnEnemies(int skin) {
            var data = new SFSObject().Apply(it => {
                var timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                it.PutInt("skin", skin);
                it.PutLong("timestamp", timestamp);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdSpawnEnemy(data));
            return OnStoryModeSpawnEnemies(response);
        }
        
        private IEnemyDetails[] OnStoryModeSpawnEnemies(ISFSObject data) {
            var enemiesData = data.GetSFSArray("enemies");
            var enemies = new IEnemyDetails[enemiesData.Count];
            for (var i = 0; i < enemiesData.Count; i++) {
                enemies[i] = EnemyDetails.Parse(enemiesData.GetSFSObject(i));
            }
            return enemies;
        }
        
        public async Task<ITakeItemResult> RequestTakeItem(Vector2Int location) {
            var data = new SFSObject().Apply(it => {
                var timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                it.PutInt("i", location.x);
                it.PutInt("j", location.y);
                it.PutLong("timestamp", timestamp);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdTakeAdventureItem(data));
            return OnStoryModeTakeItem(response);
        }

        private ITakeItemResult OnStoryModeTakeItem(ISFSObject data) {
            var result = new TakeItemResult(data);
            return result;
        }
        
        public async Task UseStoryBooster(int boosterId) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("booster", boosterId);
            });
            
            await _serverDispatcher.SendCmd(new CmdUseAdventureBooster(data));
        }

        public async Task<IBonusRewardAdventureV2> GetBonusRewardAdventure(string rewardId, string adsId) {
            var data = new SFSObject().Apply(it => {
                var deviceType = string.IsNullOrWhiteSpace(adsId) ? "WEB" : "MOBILE";
                it.PutUtfString("reward_id", rewardId);
                it.PutUtfString("ads_id", adsId);
                it.PutUtfString("device_type", deviceType);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdGetBonusRewardAdventure(data));
            return OnGetBonusReward(response);
        }
        
        public async Task<IBonusRewardAdventureV2> GetBonusRewardPvp(string rewardId, string adsId) {
            var data = new SFSObject().Apply(it => {
                var deviceType = string.IsNullOrWhiteSpace(adsId) ? "WEB" : "MOBILE";
                it.PutUtfString("reward_id", rewardId);
                it.PutUtfString("ads_id", adsId);
                it.PutUtfString("device_type", deviceType);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdGetBonusRewardPvp(data));
            return OnGetBonusReward(response);
        }
        
        private IBonusRewardAdventureV2 OnGetBonusReward(ISFSObject data) {
            var result = new BonusRewardAdventureV2(data);
            return result;
        }
        
        public async Task<IAdventureReviveHero> ReviveAdventureHeroByAds(string adsToken) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("ads_token", adsToken);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdAdventureReviveHero(data));
            return OnReviveAdventureHero(response);
        }

        public async Task<IAdventureReviveHero> ReviveAdventureHeroByGems() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdAdventureReviveHero(data));
            return OnReviveAdventureHero(response);
        }

        private IAdventureReviveHero OnReviveAdventureHero(ISFSObject data) {
            if (ServerUtils.HasError(data)) {
                throw ServerUtils.ParseErrorMessage(data);
            }
            var result = new AdventureReviveHero(data);
            return result;
        }
        
        public IEnemyDetails CreateDemoEnemy(int id, int skin, float damage, float speed, bool follow, float hp,
            float maxHp,
            bool throughBrick, int bombSkin, long timestamp) {
            return EnemyDetails.CreateDemo(id, skin, damage, speed, follow, hp, maxHp, throughBrick, bombSkin,
                timestamp, 0, 4);
        }
    }
}