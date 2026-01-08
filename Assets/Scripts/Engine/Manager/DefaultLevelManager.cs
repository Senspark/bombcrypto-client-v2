using System;
using System.Collections.Generic;
using Engine.Entities;
using UnityEngine;
using Engine.Strategy.Provider;
using App;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;

using Cysharp.Threading.Tasks;

using DG.Tweening;
using Engine.Camera;
using PvpMode.Manager;
using Senspark;
using Services;
using Bomb = Engine.Entities.Bomb;

namespace Engine.Manager {
    public struct LevelCallback {
        public Action<EnemyType> OnAddEnemy;
        public Action<EnemyType> OnRemoveEnemy;
        public Action OnEnterDoor;
        public Action<Item> RequestTakeItem;
        public Action<BoosterType> OnUserBooster;
        public Action<int> OnPlayerInJail;
        public Action<int> OnPlayerEndInJail;
        public Action OnPlayerEndTesting;

        public Action<bool, IStoryModeEnterDoorResponse> OnLevelCompleted;
        public Action OnEnemiesCleared;
        public Action OnEnemiesSpawned;
        public Action<int, Vector3> EarnGoldFromEnemy;
        public Action<int> OnSleepBossCountDown;
        public Action OnBossWakeup;

        public Action<int, ItemType, int> OnUpdateItem;
        public Action<int, int> OnUpdateHealthUi;
    }

    public class DefaultLevelManager : ILevelManager {
        public static int LEVELS_IN_STAGE = 5;

        public IEntityManager EntityManager { get; set; }
        public int Layer { get; set; } = 0;
        public GameModeType GameMode { get; set; }

        public bool IsStoryMode =>
            GameMode == GameModeType.StoryMode;

        public bool IsPvpMode =>
            GameMode == GameModeType.PvpMode;

        public bool IsHunterMode =>
            GameMode == GameModeType.TreasureHuntV2;

        public IHeroDetails StoryModeHero { get; set; }

        public int CurrentLevel { get; }
        public int CurrentStage { get; }
        private LevelCallback LevelCallback { get; }
        public bool IsCompleted { get; private set; } = false;
        public ICamera Camera { get; set; }

        private bool HasDoorUnderBrick { get; set; } = false;

        private Door _door = null;

        private readonly IPveModeManager _pveModeManager;

        private bool _isTesting;
        
        private const string TonPath = "Assets/Scenes/TreasureModeScene/TonAddressableOnly/AccessByReference/Blocks";

        public DefaultLevelManager(IEntityManager entityManager, LevelCallback callback, int level,
            GameModeType gameMode, int stage = 0) {
            EntityManager = entityManager;
            GameMode = gameMode;
            CurrentLevel = level;
            CurrentStage = stage;
            LevelCallback = callback;
            IsCompleted = false;
            _pveModeManager = ServiceLocator.Instance.Resolve<IPveModeManager>();
        }

        public void CreateBlocksUnderBrick(IBlockInfo[] blocks) {
            var itemMappers = new Dictionary<BlockType, ItemType> {
                [BlockType.BombUp] = ItemType.BombUp,
                [BlockType.FireUp] = ItemType.FireUp,
                [BlockType.Boots] = ItemType.Boots,
                [BlockType.Shield] = ItemType.Armor,
                [BlockType.Skull] = ItemType.SkullHead,
                [BlockType.GoldX1] = ItemType.GoldX1,
                [BlockType.GoldX1] = ItemType.GoldX1,
                [BlockType.GoldX5] = ItemType.GoldX1,
                [BlockType.BronzeChest] = ItemType.BronzeChest,
                [BlockType.SilverChest] = ItemType.SilverChest,
                [BlockType.GoldChest] = ItemType.GoldChest,
                [BlockType.PlatinumChest] = ItemType.PlatinumChest,
            };
            foreach (var t in blocks) {
                if ((int) t.BlockType >= (int) BlockType.BombUp &&
                    (int) t.BlockType <= (int) BlockType.PlatinumChest) {
                    // OK.
                } else {
                    // Normal or bomb blocks.
                    continue;
                }
                var item = itemMappers[t.BlockType];
                var x = t.Position.x;
                var y = t.Position.y;
                EntityManager.MapManager.SetItemUnderBrick(item, new Vector2Int(x, y));
            }
        }

        public void SetTestingMode(bool isTesting) {
            _isTesting = isTesting;
        }

        public void CreateItemsUnderBrick(IAdventureItem[] items) {
            if (_isTesting) return;
            foreach (var t in items) {
                var item = (ItemType) t.Type;
                var (x, y) = (t.X, t.Y);
                EntityManager.MapManager.SetItemUnderBrick(item, new Vector2Int(x, y));
            }
        }

        public void SetDoorLocation(Vector2Int location) {
            if (_isTesting) return;
            EntityManager.MapManager.SetDoorLocation(location);
            HasDoorUnderBrick = true;
        }

        private void CreateDoor() {
            if (_isTesting) return;
            var location = EntityManager.MapManager.GetDoorLocation();
            CreateEntity(EntityType.Door, location);
        }

        public Bomb CreateBomb(Vector3 position) {
            var provider = new PoolableProvider("Prefabs/Entities/Bomb");
            var bomb = provider.CreateInstance(EntityManager);
            bomb.Type = EntityType.Bomb;

            var transform = bomb.transform;
            transform.SetParent(EntityManager.View.transform, false);
            transform.localPosition = position;

            EntityManager.AddEntity(bomb);
            return bomb as Bomb;
        }

        public async UniTask<Entity> CreateEntity(EntityType entityType, Vector2Int tileLocation, bool follow = false) {
            if (entityType == EntityType.Enemy) {
                if (!EntityManager.MapManager.IsEmpty(tileLocation, false, true)) {
                    var emptyLocations = EntityManager.MapManager.GetEmptyAround(tileLocation, false, true);
                    if (emptyLocations.Count > 0) {
                        tileLocation += emptyLocations[0];
                    } else {
                        return null;
                    }
                }
            }

            var position = EntityManager.MapManager.GetTilePosition(tileLocation);

            Entity entity;
            if (entityType == EntityType.Enemy && follow) {
                var provider = new PoolableProvider("Prefabs/Enemies/FollowEnemy");
                entity = provider.CreateInstance(EntityManager);
            } else {
                entity = EntityManager.MapManager.TryCreateEntityLocation(entityType, tileLocation);
            }
            if (!entity) {
                // sử dụng lại code cũ
                var entityPath = _pveModeManager.GetEntityPath(entityType);
                PoolableProvider provider = null;
                if (AppConfig.IsWebAirdrop()) {
                    var pro = new PoolableProvider();
                    string[] parts = entityPath.Split('/');
                    var name = parts[^1];
                    provider = await pro.GetProvider($"{TonPath}/{name}.prefab");
                    entity = provider.CreateInstance(EntityManager);
                } 
                else
                {
                    provider = new PoolableProvider(entityPath);
                    entity = provider.CreateInstance(EntityManager);
                }
            }
            entity.Type = entityType;

            // update entity configure
            UpdateEntityConfigure(entity, position);

            if (entity.Type == EntityType.Door) {
                _door = (Door) entity;
                _door.SetLocation(tileLocation);
            }

            if (entity.Type == EntityType.Item) {
                var item = (Item) entity;
                var itemType = EntityManager.MapManager.GetItemType(tileLocation.x, tileLocation.y);
                item.ItemType = itemType;
                item.SetLocation(tileLocation);
                EntityManager.MapManager.SetTileType(tileLocation.x, tileLocation.y, TileType.Item);
            }

            return entity;
        }

        private void UpdateEntityConfigure(Entity entity, Vector3 position) {
            var transform = entity.transform;
            transform.SetParent(EntityManager.View.transform, false);
            transform.localPosition = position;

            EntityManager.AddEntity(entity);
        }

        public void UpdateProcess() {
            if (IsCompleted == false) {
                if (GameMode != GameModeType.StoryMode &&
                    GameMode != GameModeType.PvpMode) {
                    if (EntityManager.MapManager.IsEmptyBlock) {
                        LevelCompleted(true, null);
                    }
                }
            }

            if (GameMode == GameModeType.StoryMode) {
                if (EntityManager.EnemyManager.Count <= 0) {
                    if (_isTesting) {
                        _isTesting = false;
                        _door = null;
                        HasDoorUnderBrick = true;
                        OnPlayerEndTesting();
                    } else {
                        if (_door != null) {
                            _door.SetDoorActive(true);
                        } else if (!HasDoorUnderBrick) {
                            HasDoorUnderBrick = true;
                            CreateDoor();
                        }
                    }
                } else {
                    if (_door != null) {
                        _door.SetDoorActive(false);
                    }
                }
            }
        }

        public void GenerateEnemiesFromDoor(IEnemyDetails[] enemies) {
            if (enemies != null && enemies.Length > 0) {
                // FIXME: bug 2 bomb nổ cùng lúc tại cửa, bomb 2 trả về trước (khi của chưa được tạo). 
                if (_door == null) {
                    ForceCreateDoorForEnemies();
                }
                _door.SetDoorActive(false);
                var location = EntityManager.MapManager.GetDoorLocation();

                for (var i = 0; i < enemies.Length; i++) {
                    var delay = UnityEngine.Random.Range(0, 0.5f);
                    var data = enemies[i];
                    GenerateEnemyDoor(data, location, delay);
                }
                LevelCallback.OnEnemiesSpawned?.Invoke();
            }
        }

        private void ForceCreateDoorForEnemies() {
            var mapManager = EntityManager.MapManager;
            var location = mapManager.GetDoorLocation();
            mapManager.ForceRemoveBrick(location.x, location.y);
            CreateDoor();
        }

        private void GenerateEnemyDoor(IEnemyDetails enemyData, Vector2Int location, float delay) {
            DOTween.Sequence()
                .AppendInterval(delay)
                .AppendCallback(() => {
                    UniTask.Void(async () => {
                        var enemy = await EntityManager.EnemyManager.CreateEnemy(enemyData, location);
                        enemy.SetImmortal();
                    });
                });
        }

        public void OnUpdateItem(int slot, ItemType item, int value) {
            LevelCallback.OnUpdateItem?.Invoke(slot, item, value);
        }

        public void OnUpdateHealthUi(int slot, int value) {
            LevelCallback.OnUpdateHealthUi?.Invoke(slot, value);
        }

        public void OnBombExploded(HeroId heroId, int bombId, Vector2Int tileLocation,
            List<Vector2Int> brokenList) {
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            if (AppConfig.IsSolana()) {
                serverManager.UserSolanaManager.StartExplodeSol(GameMode, heroId, bombId, tileLocation, brokenList);
            } 
            else {
                serverManager.Pve.StartExplode(GameMode, heroId, bombId, tileLocation, brokenList);
            }
        }

        public void CheckEnemiesClear() {
            if (EntityManager.EnemyManager.Count <= 0) {
                LevelCallback.OnEnemiesCleared?.Invoke();
            }
        }

        public void OnAddEnemy(EnemyType enemyType) {
            LevelCallback.OnAddEnemy(enemyType);
        }

        public void OnRemoveEnemy(EnemyType enemyType) {
            LevelCallback.OnRemoveEnemy(enemyType);
        }

        public void EarnGoldFromEnemy(int value, Vector3 from) {
            LevelCallback.EarnGoldFromEnemy(value, from);
        }

        public void OnSleepBossCountDown(int value) {
            LevelCallback.OnSleepBossCountDown(value);
        }

        public void OnBossWakeup() {
            LevelCallback.OnBossWakeup();
        }

        public void LevelCompleted(bool win, IStoryModeEnterDoorResponse reward) {
            IsCompleted = true;
            LevelCallback.OnLevelCompleted?.Invoke(win, reward);
        }

        public void EnterDoor() {
            LevelCallback.OnEnterDoor?.Invoke();
        }

        public void OnUseBooster(BoosterType type) {
            LevelCallback.OnUserBooster?.Invoke(type);
        }

        public void OnPlayerInJail(int slot) {
            LevelCallback.OnPlayerInJail?.Invoke(slot);
        }

        public void OnPlayerEndInJail(int slot) {
            LevelCallback.OnPlayerEndInJail?.Invoke(slot);
        }

        public void RequestTakeItem(Item item) {
            LevelCallback.RequestTakeItem?.Invoke(item);
        }

        private void OnPlayerEndTesting() {
            LevelCallback.OnPlayerEndTesting?.Invoke();
        }

        //-----------------------------------------------------------------

        #region StoryMode

        public static (int, int) GetStageLevel(int levelIndex) {
            if (levelIndex > 0) {
                var stage = (levelIndex) / LEVELS_IN_STAGE;
                var level = (levelIndex) - (stage * LEVELS_IN_STAGE);
                return (stage, level);
            } else {
                return (0, 0);
            }
        }

        public static int GetLevelIndexFromStageLevel(int stage, int level) {
            return level + (stage * LEVELS_IN_STAGE);
        }

        public static IEnemyDetails[] LoadEnemyForMenu(int stage) {
            var storyModeBridge = ServiceLocator.Instance.Resolve<IServerManager>().StoryMode;
            var enemies = new IEnemyDetails[4];
            if (stage >= 7) {
                // vì stage 7, 8 có 4 quái + 1 boss, nhưng chỉ 3 quái trong level nên tắch riêng bắt đầu từ skin thứ 28 và 33
                var startSkin = stage == 7 ? 28 : 33;
                for (var i = 0; i < 4; i++) {
                    enemies[i] = storyModeBridge.CreateDemoEnemy(0, i + startSkin, 0, 0, false, 0, 0, false, 0, 0);
                }
            } else { // các stage trước đó có 3 quái + 1 boss nên lấy bắt đầu từ skin thứ (stage * 4).
                for (var i = 0; i < 4; i++) {
                    enemies[i] = storyModeBridge.CreateDemoEnemy(0, i + (stage * 4), 0, 0, false, 0, 0, false, 0, 0);
                }
            }
            return enemies;
        }

        public static string GetStageName(int stage) {
            var dictionary = new Dictionary<int, string>() {
                { 0, "Toy" },
                { 1, "Candy" },
                { 2, "Forest" },
                { 3, "Sand" },
                { 4, "Machine" },
                { 5, "Pirates" },
                { 6, "Chinese Restaurant" },
                { 7, "Farm" },
                { 8, "Park" },
                { 9, "Cemetery" }
            };
            return dictionary[stage];
        }

        public static string GetBossName(int stage) {
            return stage switch {
                0 => "Big Tank",
                1 => "Candy King",
                2 => "Big Rocky Lord",
                3 => "Beetles King",
                4 => "Deceptions Headquarter",
                5 => "Lord Pirates",
                6 => "Dumplings Master",
                7 => "Pumpkin Lord",
                8 => "Jester King",
                9 => "Baby Demon King",
                _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
            };
        }

        public static string GetStageNameKey(int stage) {
            var dictionary = new Dictionary<int, string>() {
                { 0, "ui_toy" },
                { 1, "ui_candy" },
                { 2, "ui_forest" },
                { 3, "ui_sand" },
                { 4, "ui_machine" },
                { 5, "ui_pirates" },
                { 6, "ui_chinese_restaurant" },
                { 7, "ui_farm" },
                { 8, "ui_park" },
                { 9, "ui_cemetery" },
            };
            return dictionary[stage];
        }

        #endregion
    }
}