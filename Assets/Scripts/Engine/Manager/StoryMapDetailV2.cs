using System.Collections.Generic;

using UnityEngine;

using Engine.Entities;

using App;

using Constant;

using Data;

using SuperTiled2Unity;

namespace Engine.Manager {
    public class BossSkillDetailsV2 : IBossSkillDetails {
        public float CoolDownShoot { get; }
        public float CoolDownSpawn { get; }
        public float CoolDownShootBoost { get; }
        public float SpeedBoost { get; }
        public float SpeedBoostTime { get; }
        public float PercentHealthBoost { get; }

        public BossSkillDetailsV2(
            float coolDownShoot = 0,
            float coolDownSpawn = 0,
            float coolDownShootBoost = 0,
            float speedBoost = 0,
            float speedBoostTime = 0,
            float percentHealthBoost = 0) {
            CoolDownShoot = coolDownShoot;
            CoolDownSpawn = coolDownSpawn;
            CoolDownShootBoost = coolDownShootBoost;
            SpeedBoost = speedBoost;
            SpeedBoostTime = speedBoostTime;
            PercentHealthBoost = percentHealthBoost;
        }
    }

    public class EnemyDetailsV2 : IEnemyDetails {
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

        public static IEnemyDetails Create(
            int id,
            int skin,
            Vector2Int posSpawn) {
            float damage = 1;
            float speed = 1;
            var follow = false;
            float hp = 1;
            float maxHp = 1;
            var throughBrick = false;
            var bombSkin = 0;
            long timestamp = 0;
            var goldReceive = 0;
            var bombRange = 1;
            return new EnemyDetailsV2(id, skin, damage, speed, follow, hp, maxHp, throughBrick, bombSkin,
                timestamp, goldReceive, bombRange, posSpawn);
        }

        public static IEnemyDetails Create(IEnemyDetails raw, Vector2Int posSpawn) {
            return new EnemyDetailsV2(raw.Id, raw.Skin, raw.Damage, raw.Speed, raw.Follow, raw.Hp, raw.MaxHp, raw.ThroughBrick, raw.BombSkin,
                raw.Timestamp, raw.GoldReceive, raw.BombRange, posSpawn);
        }

        private EnemyDetailsV2(
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

    public class AdventureItemV2 : IAdventureItem {
        public int X { get; set; }
        public int Y { get; set; }
        public int Type { get; set; }
        public int RewardValue { get; set; }
    }

    public class StoryMapDetailV2 : IStoryMapDetail {
        private IStoryMapDetail _raw;
        private MapInfo _mapInfo;
        public int Stage => _raw.Stage;
        public int Level => _raw.Level;
        public int Row => _mapInfo.Row;
        public int Col => _mapInfo.Col;
        public IHeroDetails Hero => _raw.Hero;
        public Vector2Int[] Positions { get; } //danh sách vị trí các soft block
        public Vector2Int Door { get; } //vị trí của door (điều kiện vị trí này là vị trí của 1 soft block)
        public IEnemyDetails[] Enemies { get; } //danh sách các enemies trong level. (tham khảo levelStrategy để tạo danh sách này).

        public IAdventureItem[] Items { get; } //danh sách các items trong level.
        public EquipmentData[] Equipments { get; set; }

        public Dictionary<StatId, int> MaximumStats {
            get {
                return _raw.MaximumStats;
            }
            set {
                _raw.MaximumStats = value;
            }
        }

        public StoryMapDetailV2(IStoryMapDetail raw, MapInfo mapInfo) {
            _raw = raw;
            _mapInfo = mapInfo;
            Equipments = raw.Equipments;
            Door = mapInfo.ExtendData.Data.door;
            // Add soft block
            var blocks = mapInfo.ExtendData.Data.blocks;
            var positions = new List<Vector2Int>() { };
            foreach (var block in blocks) {
                if (block.type == SuperBlockType.Soft) {
                    positions.Add(new Vector2Int(block.x, block.y));
                }
            }
            Positions = positions.ToArray();
#if RANDOM_ENEMIES
            var enemiesData = mapInfo.ExtendData.Data.enemies;
            var enemies = new List<IEnemyDetails>();
            for (var idx = 0; idx < enemiesData.Count; idx++) {
                var data = enemiesData[idx];
                enemies.Add(EnemyDetailsV2.Create(idx, ((int)EnemyType.BabyLog) + 1, new Vector2Int(data.rect_spawn.x + data.rect_spawn.width - 1 + 2, data.rect_spawn.y + data.rect_spawn.height - 1 + 1)));
            }
            Enemies = enemies.ToArray();
#else
            var enemies = new List<IEnemyDetails>();
            var enemiesData = mapInfo.ExtendData.Data.enemies;
            for (var index = 0; index < raw.Enemies.Length; index++) {
                var enemyRaw = raw.Enemies[index];
                var enemyType = (EnemyType)(enemyRaw.Skin - 1);
                var offsetY = DefaultEnemyManager.IsBoss(enemyType) ? 2 : 0;
                if (index < enemiesData.Count) {
                    // Use PosSpawn in local file ( tilemap)
                    var rectSpawn = enemiesData[index].rect_spawn;
                    enemyRaw.PosSpawn = new Vector2Int(rectSpawn.x, rectSpawn.y);
                }
                enemies.Add(EnemyDetailsV2.Create(enemyRaw, new Vector2Int(enemyRaw.PosSpawn.x, enemyRaw.PosSpawn.y + offsetY)));
            }
            Enemies = enemies.ToArray();
#endif

#if RANDOM_ITEMS
            var items = new List<IAdventureItem>();
            var softAll = mapInfo.ExtendData.Data.blocks.FindAll(it => it.type == SuperBlockType.Soft);
            foreach (var itemRaw in raw.Items) {
                if (softAll.Count <= 0) break;
                var soft = softAll[Random.Range(0, softAll.Count)];
                items.Add(new AdventureItemV2() {
                    X = soft.x,
                    Y = soft.y,
                    Type = itemRaw.Type,
                    RewardValue = itemRaw.RewardValue
                });
            }
            Items = items.ToArray();
#else
            Items = raw.Items;
#endif
        }
    }

    public class ExtendMapData {
        public SuperMapData Data { get; }

        public IBossSkillDetails BossSkillDetails { get; }

        public ExtendMapData(SuperMapData data) {
            Data = data;
            BossSkillDetails = new BossSkillDetailsV2();
        }
    }
}