using System.Collections.Generic;
using System.Linq;

using App;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.Queue;

using BomberLand.Manager;

using CreativeSpore.SuperTilemapEditor;

using Engine.Activity;
using Engine.Collision;
using Engine.Entities;
using Engine.Manager;
using Engine.MapRenderer;
using Engine.Utils;

using PvpMode.Entities;
using PvpMode.Services;

using Senspark;

using SuperTiled2Unity;

using UnityEngine;

using Utils;

using DefaultEntityManager = Engine.Manager.DefaultEntityManager;
using IEntityManager = Engine.Manager.IEntityManager;
using IMapManager = Engine.Manager.IMapManager;
using MapInfo = Engine.Manager.MapInfo;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace BLPvpMode.GameView {
    public struct PvpModeCallback {
        public System.Action OnKick;
        public System.Action<int> OnPlayerInJail;
        public System.Action<int> OnPlayerEndInJail;
        public System.Action<int, ItemType, int> OnUpdateItem;
        public System.Action<string> OnErrorMessage;
    }

    public class BLevelViewPvp : MonoBehaviour, ILevelViewPvp {
        public static BLevelViewPvp Create(Transform mapParent) {
            var path = "Prefabs/Levels/BLlevelPvpMode";
            var prefab = Resources.Load<BLevelViewPvp>(path);
            return Instantiate(prefab, mapParent);
        }

        [SerializeField]
        private Tileset[] tileSets;

        [SerializeField]
        private TilemapGroup tilemapBackground;

        [SerializeField]
        private TilemapGroup tileMapGroup;

        [SerializeField]
        private GameObject startLocation;

        public GameObject StartLocation => startLocation;

        private IEntityManager _entityManager;
        private PvpModeCallback _pvpModeCallback;
        private DirectionInputProcess _directionInputProcess;
        private FallingBlocksQueue _fallingBlocksQueue;
        private IMatchData _matchData;

        public TilemapGroup TilemapBackground => tilemapBackground;
        public TilemapGroup TileMapGroup => tileMapGroup;

        protected void Awake() {
            _directionInputProcess = new DirectionInputProcess();
        }

        public void Initialize(
            IMatchHeroInfo[] heroes,
            IMapInfo mapDetail,
            IMatchData matchData,
            PvpModeCallback pvpModeCallback,
            int layer,
            SuperMapData mapData
        ) {
            var scene = SceneManager.GetActiveScene();
            var physicsScene = scene.GetPhysicsScene2D();
            _pvpModeCallback = pvpModeCallback;
            gameObject.SetLayer(layer);

            _matchData = matchData;

            var manager = new DefaultEntityManager(tileMapGroup.gameObject, physicsScene, true, layer);
            var items = GetComponentsInChildren<Entity>();
            foreach (var item in items) {
                if (item.IsAlive) {
                    manager.AddEntity(item);
                }
            }

            // In Game Manager
            manager.ExplodeEventManager = new PvpExplodeEventManager(manager);
            manager.TakeDamageEventManager = new DefaultTakeDamageEventManager(manager);

            // Level Manager
            var levelCallback = new LevelCallback {
                OnAddEnemy = _ => { },
                OnRemoveEnemy = _ => { },
                OnPlayerInJail = OnPlayerInJail,
                OnPlayerEndInJail = OnPlayerEndInJail,
                OnUpdateItem = OnUpdateItem,
            };
            manager.LevelManager = new DefaultLevelManager(manager, levelCallback, 0, GameModeType.PvpMode);

            // Map Manager
            var col = mapDetail.Width;
            var row = mapDetail.Height;
            var tileIndex = mapDetail.Tileset;

            var tileSet = tileSets[tileIndex];
            var mapInfo = new MapInfo() {
                GameModeType = GameModeType.PvpMode,
                Col = col,
                Row = row,
                ExtendData = mapData != null ? new ExtendMapData(mapData) : null
            };
            manager.MapManager = new DefaultMapManagerV2(tileIndex, tileSet, manager, tilemapBackground, null,
                tileMapGroup, mapInfo);
            manager.MapManager.LoadMapV2(MapHelperV2.GetMapDataFromPvpMapDetail(mapDetail, mapInfo));

            //Player Manager
            manager.PlayerManager = new DefaultPlayerManager(manager, startLocation.transform, mapDetail, heroes);

            // Not call LevelManager.CreateItems, because Item information will be sent when brick destroyed.
            // manager.LevelManager.CreateItems(pvpMapDetail);

            if (mapInfo.ExtendData != null) {
                //Enemy Manager
                manager.EnemyManager = new DefaultEnemyManager(manager, mapInfo.ExtendData.BossSkillDetails);
                var enemies = mapInfo.ExtendData.Data.enemies;
                for (var idx = 0; idx < enemies.Count; idx++) {
                    var enemy = enemies[idx];
                    manager.EnemyManager.CreateEnemy(
                        EnemyDetailsV2.Create(idx, ((int) EnemyType.BabyLog) + 1,
                            new Vector2Int(enemy.rect_spawn.x, enemy.rect_spawn.y)), Vector2Int.zero);
                }
            }

            //add Activities
            manager.AddActivity(new DestructionActivity(manager));
            manager.AddActivity(new UpdateActivity(manager));

            //add Listener
            manager.AddListener(new DamageListener());
            manager.AddListener(new EncounterListener());
            manager.AddListener(new PlayerBombListener());
            manager.AddListener(new ItemListener());

            _entityManager = manager;
            _fallingBlocksQueue = new FallingBlocksQueue(_entityManager);
        }

        public void UpdateState(int slot) {
            //Cập nhât chỉ số item
            var levelManager = _entityManager.LevelManager;
            var playerManager = _entityManager.PlayerManager;
            var player = playerManager.Players[slot];
            levelManager.OnUpdateItem(slot, ItemType.BombUp, player.Bombable.MaxBombNumber);
            levelManager.OnUpdateItem(slot, ItemType.FireUp, player.Bombable.ExplosionLength);
            levelManager.OnUpdateItem(slot, ItemType.Boots, (int) player.Movable.Speed);
        }

        public void ShowHealthBarOnPlayerNotSlot(int slot) {
            _entityManager.PlayerManager.ShowHealthBarOnPlayerNotSlot(slot);
        }

        public void PlayKillEffectOnOther(int slot) {
            _entityManager.PlayerManager.PlayKillEffectOnOther(slot);
        }

        public void UpdateHealthBar(int slot, int value) {
            var hero = GetPvpHeroBySlot(slot);
            hero.Health.SetCurrentHealth(value);
        }

        public void SetImmortal(int slot) {
            var hero = GetPvpHeroBySlot(slot);
            hero.SetImmortal();
        }

        public List<PlayerPvp> GetPvpHeroes() {
            var heroes = _entityManager.PlayerManager.Players;
            return heroes.Cast<PlayerPvp>().ToList();
        }

        public PlayerPvp GetPvpHeroBySlot(int slot) {
            return _entityManager.PlayerManager.GetPlayerBySlot(slot) as PlayerPvp;
        }

        public void KillHero(int slot, HeroDamageSource source) {
            var hero = GetPvpHeroBySlot(slot);
            hero.KillHero(slot);
            switch (source) {
                case HeroDamageSource.PrisonBreak:
                    ShakeCamera();
                    break;
                case HeroDamageSource.HardBlock:
                    OnWallDropTakeDamage(hero.GetTileLocation());
                    break;
            }
        }

        public void HeroShieldBroken(int slot) {
            var hero = GetPvpHeroBySlot(slot);
            hero.ShieldBreak();
        }

        public void ExplodeBomb(int slot, int id, Dictionary<Direction, int> ranges) {
            _entityManager.PlayerManager.ExplodePvpBomb(slot, id, ranges);
            // FIXME: Force process explosion event immediately.
            _entityManager.ExplodeEventManager.UpdateProcess(1);
        }

        public async void CreateItem(Vector2Int tileLocation, ItemType itemType) {
            if (EntityManager.MapManager.IsItem(tileLocation.x, tileLocation.y)) {
#if UNITY_EDITOR
                Debug.LogWarning("This tile had item");
#endif
                return;
            }
            if (EntityManager.MapManager.IsBrick(tileLocation.x, tileLocation.y)) {
                EntityManager.MapManager.SetItemUnderBrick(itemType, tileLocation);
                return;
            }
            if (!EntityManager.MapManager.IsEmpty(tileLocation.x, tileLocation.y)) {
#if UNITY_EDITOR
                Debug.LogWarning("Can't create item on tile not empty");
#endif
                return;
            }
            var entity = await _entityManager.LevelManager.CreateEntity(EntityType.Item, tileLocation);
            var item = (Item) entity;
            item.ItemType = itemType;
            item.SetLocation(tileLocation);
            EntityManager.MapManager.SetTileType(tileLocation.x, tileLocation.y, TileType.Item);
        }

        public bool RemoveBlock(Vector2Int location) {
            return _entityManager.MapManager.BreakBrick(location.x, location.y);
        }

        public void RemoveBomb(int slot, int id) {
            _entityManager.PlayerManager.RemoveBomb(slot, id);
        }

        private void OnWallDropTakeDamage(Vector2Int location) {
            _entityManager.MapManager.WallDropTakeDamage(location.x, location.y);
        }

        public void SetShieldToPlayer(int slot) {
            _entityManager.PlayerManager.SetShieldToPlayer(slot);
        }

        public void JailBreak(int slot) {
            _entityManager.PlayerManager.JailBreak(slot);
        }

        public void AddItemToPlayer(bool playSound, int slot, HeroItem item, int value) {
            _entityManager.PlayerManager.AddItemToPlayer(playSound, slot, item, value);
        }

        public void SetSkullHeadToPlayer(bool playSound, int slot, HeroEffect effect, int duration) {
            _entityManager.PlayerManager.SetSkullHeadToPlayer(playSound, slot, effect, duration);
        }

        public void RemoveItem(Vector2Int location) {
            _entityManager.MapManager.RemoveItem(location.x, location.y);
        }

        public void SpawnBomb(int slot, int id, int range, Vector2Int position) {
            _entityManager.PlayerManager.SpawnPvpBomb(slot, id, range, position);
        }

        public void DropWall() {
            var dropList = _entityManager.MapManager.CreateDropListFullMap();
            _fallingBlocksQueue.PushEventFalling(dropList);
        }

        public void AddFallingWall(IFallingBlockInfo[] blocks) {
            _fallingBlocksQueue.PushEventFalling(blocks);
        }

        public TileType GetTileType(int x, int y) {
            return _entityManager.MapManager.GetTileTypeMap()[x, y];
        }

        public ItemType GetItemType(int x, int y) {
            return _entityManager.MapManager.GetItemType(x, y);
        }

        public Vector3 GetTilePosition(int x, int y) {
            return _entityManager.MapManager.GetTilePosition(x, y);
        }

        public void Step(float delta) {
            _entityManager.Step(delta);
            _fallingBlocksQueue.UpdateProcess(delta, _matchData.RoundStartTimestamp);
        }

        public void ProcessMovement(int slot, Vector2 direction) {
            _directionInputProcess.ProcessMovement(GetPvpHeroBySlot(slot), direction);
        }

        public void ProcessUpdatePlayer(int slot) {
            _entityManager.PlayerManager.UpdateProcess(slot);
        }

        public void SetPlayerInJail(int slot) {
            GetPvpHeroBySlot(slot).SetPlayerInJail();
        }

        public bool PlayerIsInJail(int slot) {
            return GetPvpHeroBySlot(slot).IsInJail;
        }

        private void OnKick() {
            _pvpModeCallback.OnKick?.Invoke();
        }

        private void OnPlayerInJail(int slot) {
            _pvpModeCallback.OnPlayerInJail?.Invoke(slot);
        }

        private void OnPlayerEndInJail(int slot) {
            _pvpModeCallback.OnPlayerEndInJail?.Invoke(slot);
        }

        private void OnUpdateItem(int slot, ItemType item, int value) {
            _pvpModeCallback.OnUpdateItem?.Invoke(slot, item, value);
        }

        public bool CheckSpawnPvpBomb(int slot) {
            return _entityManager.PlayerManager.CheckSpawnPvpBomb(slot);
        }

        public void ShakeCamera() {
            _entityManager.LevelManager.Camera?.Shaking(0.1f, 1);
        }

        public int[,] GetMapGrid(bool throughBrick, bool throughBomb, bool throughWall) {
            return _entityManager.MapManager.GetMapGrid(throughBrick, throughBomb, throughWall);
        }

        public IEntityManager EntityManager => _entityManager;
        public IMapManager MapManager => _entityManager.MapManager;
    }
}