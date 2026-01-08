using System;
using System.Collections.Generic;
using System.Linq;

using Constant;

using Data;

using Engine.Entities;

using Newtonsoft.Json;

using Sfs2X.Entities.Data;

using UnityEngine;

namespace App {
    public partial class DefaultStoryModeServerBridge {
        private class StoryLevelDetail : IStoryLevelDetail {
            public int CurrentLevel { get; set; }
            public int MaxLevel { get; set; }
            public bool IsNew { get; set; }
            public int HeroId { get; set; }
            public (long Id, int RemainingSeconds)[] PlayedBombers { get; set; }
            public (float FirstWin, float Replay)[] Rewards { get; set; }
        }

        private class LevelMapDetail : ILevelMapDetail {
            public int Stage { get; set; }
            public int LevelCount { get; set; }
        }

        private class AdventureLevelDetail : IAdventureLevelDetail {
            public bool IsNew { get; set; }
            public int MaxLevel { get; set; }
            public int CurrentLevel { get; set; }
            public int CurrentStage { get; set; }
            public int MaxStage { get; set; }
            public int HeroId { get; set; }
            public List<ILevelMapDetail> LevelMaps { get; set; }
        }

        public enum BlockType {
            Rock,
            Normal,
            Jail,
            Wooden,
            Silver,
            Golden,
            Diamond,
            Legend
        }

        private class Block {
            [JsonProperty("type")]
            public BlockType BlockType { get; set; }

            [JsonProperty("hp")]
            public int Health { get; set; }

            [JsonProperty("maxHp")]
            public int MaxHealth { get; set; }

            [JsonProperty("i")]
            public int PositionX { get; set; }

            [JsonProperty("j")]
            public int PositionY { get; set; }

            public void TakeDamage(int value) => Health = Math.Max(Health - value, 0);
        }

        private class AdventureItem : IAdventureItem {
            [JsonProperty("i")]
            public int X { get; set; }

            [JsonProperty("j")]
            public int Y { get; set; }

            [JsonProperty("item")]
            public int Type { get; set; }

            [JsonProperty("rewardValue")]
            public int RewardValue { get; set; }
        }

        private class StoryMapDetail : IStoryMapDetail {
            public int Stage { get; }
            public int Level { get; }
            public int Row { get; }
            public int Col { get; }
            public IHeroDetails Hero { get; }
            public Vector2Int[] Positions { get; } //danh sách vị trí các soft block
            public Vector2Int Door { get; } //vị trí của door (điều kiện vị trí này là vị trí của 1 soft block)

            public IEnemyDetails[]
                Enemies { get; } //danh sách các enemies trong level. (tham khảo levelStrategy để tạo danh sách này).

            public IAdventureItem[] Items { get; } //danh sách các items trong level.
            public EquipmentData[] Equipments { get; set; }
            public Dictionary<StatId, int> MaximumStats { get; set; }

            public StoryMapDetail(ISFSObject data) {
                Stage = data.GetInt("stage");
                Level = data.GetInt("level");
                Row = data.GetInt("row");
                Col = data.GetInt("col");
                Positions = JsonConvert.DeserializeObject<Block[]>(data.GetUtfString("positions"))
                    .Select(it => new Vector2Int(it.PositionX, it.PositionY)).ToArray();
                // Door = JsonConvert.DeserializeObject<Vector2Int>(data.GetUtfString("door"));
                Door = new Vector2Int(data.GetInt("door_x"), data.GetInt("door_y"));
                Items = JsonConvert.DeserializeObject<AdventureItem[]>(data.GetUtfString("items"));
                var hero = data.GetSFSObject("hero");
                var playerData = ParsePlayerData(hero);
                Hero = new StoryModeHeroDetails {
                    Details = playerData.genId,
                    Id = playerData.heroId.Id,
                    AccountType = playerData.AccountType,
                    Rarity = playerData.rare,
                    Level = playerData.level,
                    Color = (int)playerData.playercolor,
                    Skin = (int)playerData.playerType,
                    Stamina = (int)playerData.stamina,
                    Speed = (int)playerData.speed,
                    BombSkin = playerData.bombSkin,
                    BombCount = playerData.bombNum,
                    BombPower = (int)playerData.bombDamage,
                    BombRange = playerData.bombRange,
                    Abilities = playerData.abilities.Select(it => (int)it).ToArray(),
                    IsActive = playerData.active,
                    // Energy = playerData.en,
                    Stage = playerData.stage,
                    StoryHp = (int)playerData.hp,
                    StoryIsPlayed = false,
                };
                var enemies = data.GetSFSArray("enemies");
                Enemies = new IEnemyDetails[enemies.Count];
                for (var i = 0; i < enemies.Count; i++) {
                    Enemies[i] = EnemyDetails.Parse(enemies.GetSFSObject(i));
                }

                MaximumStats = new Dictionary<StatId, int>();
                MaximumStats[StatId.Speed] = hero.GetInt("maxSpeed");
                MaximumStats[StatId.Range] = hero.GetInt("maxRange");
                MaximumStats[StatId.Count] = hero.GetInt("maxBomb");
            }

            private static PlayerData ParsePlayerData(ISFSObject parameters) {
                var id = (int)parameters.GetLong("id");
                var type = parameters.ContainsKey(SFSDefine.SFSField.AccountType)
                    ? parameters.GetInt(SFSDefine.SFSField.AccountType)
                    : parameters.GetInt(SFSDefine.SFSField.HeroType);

                return new PlayerData {
                    playerType = (PlayerType)parameters.GetInt("playerType"),
                    playercolor = (PlayerColor)parameters.GetInt("playercolor"),
                    genId = parameters.GetUtfString("genId"),
                    AccountType = (HeroAccountType)type,
                    heroId = new HeroId(id, (HeroAccountType)type),
                    bombDamage = parameters.GetInt("bombDamage"),
                    speed = parameters.GetInt("speed"),
                    stamina = parameters.GetInt("stamina"),
                    bombNum = parameters.GetInt("bombNum"),
                    bombRange = parameters.GetInt("bombRange"),
                    bombSkin = parameters.GetInt("bombSkin"),

                    level = parameters.GetInt("level"),

                    rare = parameters.GetInt("rare"),
                    maxHp = parameters.GetInt("maxHp"),
                    hp = parameters.GetInt("hp"),

                    stage = (HeroStage)parameters.GetInt("stage"),
                    active = parameters.GetInt("active") == 1,
                    storyIsPlayed = parameters.GetBool("storyIsPlayed"),

                    abilities = parameters.GetIntArray("abilities").Select(it => (PlayerAbility)it).ToArray(),

                    timeSync = parameters.GetUtfString("timeSync")
                };
            }
        }

        private class StoryModeHeroDetails : IHeroDetails {
            public string Details { get; set; }
            public int Id { get; set; }
            public int Rarity { get; set; }
            public HeroAccountType Type { get; }
            public int Level { get; set; }
            public int LevelShield { get; }
            public int NumResetShield { get; }
            public int Color { get; set; }
            public int Skin { get; set; }
            public int Stamina { get; set; }
            public int Speed { get; set; }
            public int BombSkin { get; set; }
            public int BombCount { get; set; }
            public int BombPower { get; set; }
            public int BombRange { get; set; }
            public int[] Abilities { get; set; }
            public int RandomizeAbilityCounter { get; set; }
            public bool IsActive { get; set; }
            public int Energy { get; set; }
            public HeroStage Stage { get; set; }
            public bool IsHeroS { get; }
            public List<IHeroSAbility> HeroSAbilities { get; }
            public IHeroSAbility Shield { get; set; }
            public int StoryHp { get; set; }
            public bool StoryIsPlayed { get; set; }
            public long TimeSync { get; set; }
            public HeroAccountType AccountType { get; set; }
            public bool AllowRevive { get; set; }
            public bool AllowReviveByAds { get; set; }
            public int ReviveGemAmount { get; set; }
            public long TimeLockSince { get; }
            public int TimeLockSeconds { get; }
            public double StakeBcoin { get; set; }
            public double StakeSen { get; set; }
        }

        private class EnemyDetails : IEnemyDetails {
            public int Id { get; }
            public int Skin { get; }
            public float Damage { get; }
            public float Speed { get; }
            public bool Follow { get; }
            public float Hp { get; }
            public float MaxHp { get; }
            public bool ThroughBrick { get; }
            public int BombSkin { get; }
            public long Timestamp { get; }
            public int GoldReceive { get; }
            public int BombRange { get; }
            public Vector2Int PosSpawn { get; set; }

            public static IEnemyDetails Parse(ISFSObject data) {
                var id = data.GetInt("id");
                var skin = data.GetInt("skin");
                var damage = data.GetInt("damage");
                var speed = data.GetFloat("speed");
                var follow = data.GetBool("follow");
                var hp = data.GetFloat("hp");
                var maxHp = data.GetFloat("maxHp");
                var throughBrick = data.GetBool("throughBrick");
                var bombSkin = data.GetInt(("bombSkin"));
                var timestamp = 0L;
                if (data.ContainsKey("timestamp")) {
                    timestamp = data.GetLong("timestamp");
                }
                var goldReceive = data.GetInt("gold_receive");
                var bombRange = data.GetInt("bomb_range");
                var posSpawn = new Vector2Int(0, 0);
                if (data.ContainsKey("spawn")) {
                    var spawn = data.GetSFSObject("spawn");
                    posSpawn = new Vector2Int(spawn.GetInt("x"), spawn.GetInt("y"));
                }
                return new EnemyDetails(id, skin, damage, speed, follow, hp, maxHp, throughBrick, bombSkin,
                    timestamp, goldReceive, bombRange, posSpawn);
            }

            //Create 1 enemy for Demo..
            public static IEnemyDetails CreateDemo(int id, int skin, float damage, float speed, bool follow, float hp,
                float maxHp, bool throughBrick, int bombSkin, long timestamp, int goldReceive, int bombRange) {
                return new EnemyDetails(id, skin, damage, speed, follow, hp, maxHp, throughBrick, bombSkin,
                    timestamp, goldReceive, bombRange, new Vector2Int(-1, -1));
            }

            private EnemyDetails(
                int id,
                int skin,
                float damage,
                float speed,
                bool follow,
                float hp,
                float maxHp,
                bool throughBrick,
                int bombSkin,
                long timestamp,
                int goldReceive,
                int bombRange,
                Vector2Int posSpawn
            ) {
                Id = id;
                Skin = skin;
                Damage = damage;
                Speed = speed;
                Follow = follow;
                Hp = hp;
                MaxHp = maxHp;
                ThroughBrick = throughBrick;
                BombSkin = bombSkin;
                Timestamp = timestamp;
                GoldReceive = goldReceive;
                BombRange = bombRange;
                PosSpawn = posSpawn;
            }
        }

        private class StoryExplodeResponse : IStoryExplodeResponse {
            public Vector2Int[] BrokenLocation { get; set; }
            public IEnemyDetails[] EnemiesFromDoor { get; set; }
            public Vector2Int[] ItemsRemoved { get; set; }
        }

        private class KillStoryEnemyResult : IKillStoryEnemyResult {
            public IEnemyDetails Enemy { get; }

            public KillStoryEnemyResult(ISFSObject parameters) {
                Enemy = EnemyDetails.Parse(parameters);
            }
        }

        private class WinReward : IWinReward {
            [JsonProperty("rewardType")]
            public string RewardName { get; set; }

            [JsonProperty("value")]
            public int Value { get; set; }

            [JsonProperty("outOfSlot")]
            public bool OutOfSlot { get; set; }
        }

        private class StoryModeEnterDoorResponse : IStoryModeEnterDoorResponse {
            public string RewardId { get; }
            public IWinReward[] WinRewards { get; }
            public float Rewards { get; }
            public bool IsStageCompleted { get; }
            public float TimeCompleted { get; }

            public StoryModeEnterDoorResponse(ISFSObject data) {
                RewardId = data.GetUtfString("reward_id");

                if (data.ContainsKey("rewards")) {
                    WinRewards = JsonConvert.DeserializeObject<WinReward[]>(data.GetSFSArray("rewards").ToJson());
                } else {
                    WinRewards = Array.Empty<IWinReward>();
                }
                IsStageCompleted = data.GetInt("is_complete") > 0;
                TimeCompleted = data.GetInt("time_complete");
            }
        }

        private class TakeItemResult : ITakeItemResult {
            public IAdventureItem Item { get; }

            public TakeItemResult(ISFSObject parameters) {
                Item = JsonConvert.DeserializeObject<AdventureItem>(parameters.ToJson());
            }
        }

        private class BonusRewardAdventure : IBonusRewardAdventure {
            public string RewardName { get; }
            public int Value { get; }

            public BonusRewardAdventure(ISFSObject data) {
                RewardName = data.GetUtfString("reward_type");
                Value = data.GetInt("value");
            }
        }

        private class BonusRewardAdventureV2ItemData : IBonusRewardAdventureV2Item {
            public int ItemId { get; }
            public int Quantity { get; }

            public BonusRewardAdventureV2ItemData(ISFSObject data) {
                ItemId = data.GetInt("item_id");
                Quantity = data.GetInt("quantity");
            }
        }

        private class BonusRewardAdventureV2 : IBonusRewardAdventureV2 {
            public string RewardCode { get; }
            public IBonusRewardAdventureV2Item[] Items { get; }

            public BonusRewardAdventureV2(ISFSObject data) {
                RewardCode = data.GetUtfString("reward_code");
                var items = data.GetSFSArray("items");
                Items = new IBonusRewardAdventureV2Item[items.Count];
                for (var i = 0; i < items.Count; i++) {
                    Items[i] = new BonusRewardAdventureV2ItemData(items.GetSFSObject(i));
                }
            }
        }

        private class AdventureReviveGem : IAdventureReviveGem {
            public string GemType { get; }
            public int Value { get; }

            public AdventureReviveGem(ISFSObject data) {
                GemType = data.GetUtfString("type");
                Value = (int)data.GetFloat("value");
            }
        }

        private class AdventureReviveHero : IAdventureReviveHero {
            public int ReviveTimes { get; }
            public int Hp { get; }
            public IAdventureReviveGem[] GemUsed { get; }

            public AdventureReviveHero(ISFSObject data) {
                ReviveTimes = data.GetInt("revive_count");
                Hp = data.GetInt("hp");
                var gems = data.GetSFSArray("gem_used");
                GemUsed = new IAdventureReviveGem[gems.Count];
                for (var i = 0; i < gems.Count; i++) {
                    GemUsed[i] = new AdventureReviveGem(gems.GetSFSObject(i));
                }
            }
        }
    }
}