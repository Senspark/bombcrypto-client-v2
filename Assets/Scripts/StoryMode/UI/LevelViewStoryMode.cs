using System.Collections.Generic;

using Analytics;

using App;

using BLPvpMode.Engine;
using BLPvpMode.Engine.Entity;

using Com.LuisPedroFonseca.ProCamera2D;

using CreativeSpore.SuperTilemapEditor;

using Engine.Activity;
using Engine.Camera;
using Engine.Collision;
using Engine.Entities;
using Engine.Manager;
using Engine.MapRenderer;
using Engine.Utils;

using UnityEngine;

using PvpMode.Manager;

using Senspark;

using SuperTiled2Unity;

using Utils;

using DefaultEntityManager = Engine.Manager.DefaultEntityManager;
using DefaultMapManager = Engine.Manager.DefaultMapManager;
using IEntityManager = Engine.Manager.IEntityManager;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace StoryMode.UI {
    public class LevelViewStoryMode : MonoBehaviour {
        [SerializeField]
        private Tileset[] tileSets;

        [SerializeField]
        private TilemapGroup tilemapBackground;

        [SerializeField]
        private TilemapGroup tileMapGroup;

        [SerializeField]
        private GameObject startLocation;

        private IEntityManager _entityManager;
        private IAnalytics _analytics;
        private IStoryMapDetail _storyMapDetail;
        private ISoundManager _soundManager;
        private Audio _currentMusic;

        private ICamera _proCamera;

        private DirectionInputProcess _directionInputProcess;

        private int _idxMainPlayer = 0;

        public Player MainPlayer { private set; get; }

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _directionInputProcess = new DirectionInputProcess();
        }

        public void Initialize(
            IHeroDetails hero,
            IStoryMapDetail storyMapDetail,
            IBossSkillDetails bossSkillDetails,
            IAnalytics analytics,
            float leftOffset,
            LevelCallback storyModeCallback,
            bool isTesting
        ) {
            MapInfo mapInfo;
            var isUseMapPveV2 = MapHelperV2.IsUseMapPveV2(storyMapDetail.Stage, storyMapDetail.Level);
            if (isUseMapPveV2) {
                var mapParent = gameObject.transform;
                var path = $"BLMap/forest/forest_{storyMapDetail.Level}";
                var prefabMap = Resources.Load<GameObject>(path);
                var map = Instantiate(prefabMap, mapParent, true);
                map.SetLayer(mapParent.gameObject.layer, true);
                var superMap = map.GetComponent<SuperMap>();
                mapInfo = new MapInfo() {
                    GameModeType = GameModeType.StoryMode,
                    Col = superMap.m_mapData.width,
                    Row = superMap.m_mapData.height,
                    ExtendData = new ExtendMapData(superMap.m_mapData)
                };
                superMap.SetUpPosAndFixSize(transform.position);
                superMap.SetUpSortingOrder();
                storyMapDetail = new StoryMapDetailV2(storyMapDetail, mapInfo);
            } else {
                mapInfo = new MapInfo() {
                    GameModeType = GameModeType.StoryMode, Col = storyMapDetail.Col, Row = storyMapDetail.Row,
                };
            }
            _analytics = analytics;
            _storyMapDetail = storyMapDetail;
            var (stage, level) = (storyMapDetail.Stage - 1, storyMapDetail.Level - 1); // stage, level theo index zero.

            var scene = SceneManager.GetActiveScene();
            var physicsScene = scene.GetPhysicsScene2D();

            var manager = new DefaultEntityManager(tileMapGroup.gameObject, physicsScene, true);
            var items = GetComponentsInChildren<Entity>();
            foreach (var item in items) {
                if (item.IsAlive) {
                    manager.AddEntity(item);
                }
            }

            // In Game Manager
            manager.ExplodeEventManager = new StoryExplodeEventManager(manager);
            manager.TakeDamageEventManager = new DefaultTakeDamageEventManager(manager);

            // Level Manager
            manager.LevelManager =
                new DefaultLevelManager(manager, storyModeCallback, level, GameModeType.StoryMode, stage);
            manager.LevelManager.StoryModeHero = hero;

            var col = storyMapDetail.Col;
            var row = storyMapDetail.Row;

            // Camera
            float maxHorizonScroll = 0;
            float minHorizonScroll = 0;
            float maxVerticalScroll = 0;
            float minVerticalScroll = 0;
            var cam = Camera.main;
            if (cam != null) {
                if (isUseMapPveV2) {
                    cam.orthographicSize = 5.4f;
                    var w = (float) col;
                    var h = (float) row;
                    var heightCam = 2f * cam.orthographicSize;
                    var widthCam = cam.aspect * heightCam;
                    maxHorizonScroll = (w / 2) - (widthCam / 2) + 1.0f;
                    minHorizonScroll = -maxHorizonScroll - leftOffset;
                    var offsetVertical = (h / 2) - (heightCam / 2) + 1;
                    maxVerticalScroll = offsetVertical;
                    minVerticalScroll = -offsetVertical + 2;
                } else {
                    var heightCam = 2f * cam.orthographicSize;
                    var widthCam = cam.aspect * heightCam;
                    var w = (float) col;
                    var h = (float) row;
                    maxHorizonScroll = (w / 2) - (widthCam / 2) + 5.0f;
                    maxVerticalScroll = (h / 2) - (heightCam / 2) + 2.0f;
                    minHorizonScroll = -maxHorizonScroll - leftOffset;
                    minVerticalScroll = -maxVerticalScroll;
                }
            }
            _proCamera = new ProCamera(ProCamera2D.Instance,
                maxHorizonScroll, minHorizonScroll,
                maxVerticalScroll, minVerticalScroll);
            manager.LevelManager.Camera = _proCamera;

            // Map Manager
            manager.MapManager = new DefaultMapManagerV2(stage, tileSets[stage], manager, tilemapBackground, null,
                tileMapGroup, mapInfo);
            manager.MapManager.LoadMapV2(MapHelperV2.GetMapDataFromStoryMapDetail(storyMapDetail));
            var playerSpawn = isUseMapPveV2 ? mapInfo.ExtendData.Data.player_spawn : new Vector2Int(0, row - 1);
            //Add Player Manager
            var locations = new List<Vector2Int>() {new(playerSpawn.x, playerSpawn.y)};
            manager.PlayerManager = new DefaultPlayerManager(manager, startLocation.transform,
                storyMapDetail.Equipments, storyMapDetail.MaximumStats);
            manager.PlayerManager.FirstInitPlayerPVE(locations);
            //Set MainPlayer 
            _idxMainPlayer = 0;
            MainPlayer = manager.PlayerManager.Players[_idxMainPlayer];
            //
            _proCamera.SetTarget(MainPlayer);

            //Enemy Manager
            manager.EnemyManager = new DefaultEnemyManager(manager, bossSkillDetails);
            manager.EnemyManager.CreateEnemies(storyMapDetail.Enemies);
            manager.LevelManager.SetTestingMode(isTesting);
            manager.LevelManager.SetDoorLocation(storyMapDetail.Door);
            if (isUseMapPveV2) {
                manager.LevelManager.CreateEntity(EntityType.Door,
                    new Vector2Int(storyMapDetail.Door.x, storyMapDetail.Door.y));
            }

            //Create Items
            manager.LevelManager.CreateItemsUnderBrick(storyMapDetail.Items);

            //add Activities
            manager.AddActivity(new DestructionActivity(manager));
            manager.AddActivity(new UpdateActivity(manager));

            //add Listener
            manager.AddListener(new DamageListener());
            manager.AddListener(new EncounterListener());
            manager.AddListener(new PlayerBombListener());
            manager.AddListener(new ItemListener());

            _entityManager = manager;

            //Cập nhât chỉ số item
            var player = MainPlayer;
            var levelManager = manager.LevelManager;
            levelManager.OnUpdateItem(_idxMainPlayer, ItemType.BombUp, player.Bombable.MaxBombNumber);
            levelManager.OnUpdateItem(_idxMainPlayer, ItemType.FireUp, player.Bombable.ExplosionLength);
            levelManager.OnUpdateItem(_idxMainPlayer, ItemType.Boots, (int) player.Movable.Speed);

            ChangeMusic(stage);
        }

        private void ChangeMusic(int stage) {
            _currentMusic = stage switch {
                0 => Audio.StoryMusic1,
                1 => Audio.StoryMusic2,
                2 => Audio.StoryMusic3,
                3 => Audio.StoryMusic4,
                4 => Audio.StoryMusic5,
                5 => Audio.StoryMusic6,
                6 => Audio.StoryMusic7,
                7 => Audio.StoryMusic8,
                8 => Audio.StoryMusic9,
                _ => Audio.StoryMusic1
            };
            PlayMusic(_currentMusic);
            _soundManager.ChangeMusic(_currentMusic);
        }

        public void PlayMusic(Audio sound) {
            _currentMusic = sound;
            _soundManager.ChangeMusic(sound);
        }

        public void ReplayMusic() {
            _soundManager.ChangeMusic(_currentMusic);
        }

        public void OnBombButtonClicked() {
            _entityManager.PlayerManager.SpawnBomb(0);
        }

        public void Step(float delta) {
            _entityManager.Step(delta);
            _proCamera.Process(delta);
        }

        public void ProcessMovement(Vector2 direction) {
            _directionInputProcess.ProcessMovement(MainPlayer, direction);
        }

        public void ProcessUpdatePlayer() {
            _entityManager.PlayerManager.UpdateProcess(_idxMainPlayer);
        }

        public void SaveMap() {
            _entityManager.MapManager.SaveMap();
        }

        public void ShakeCamera() {
            _entityManager.LevelManager.Camera?.Shaking(0.1f, 1);
        }

        public void ExitPlayer() {
            _entityManager.PlayerManager.ExitPlayer(0);
        }

        public void TakeItem(int x, int y, ItemType itemType, int rewardValue) {
            _entityManager.MapManager.RemoveItem(x, y);
            if (!PlayerIsAlive()) {
                return;
            }
            switch (itemType) {
                case ItemType.SkullHead:
                    var effects = new[] {
                        HeroEffect.SpeedTo1, HeroEffect.SpeedTo10, HeroEffect.ReverseDirection,
                        HeroEffect.PlantBombRepeatedly,
                    };
                    var index = UnityEngine.Random.Range(0, effects.Length);
                    MainPlayer.StartSkullHeadEffect(effects[index], 10000);
                    break;
                default:
                    MainPlayer.SetItem(itemType, rewardValue);
                    break;
            }
        }

        public void TakeItem(IAdventureItem item) {
            TakeItem(item.X, item.Y, (ItemType) item.Type, item.RewardValue);
        }

        public void DeActiveItem(Item item) {
            item.SetActive(false);
        }

        public void UseBooster(BoosterType type) {
            switch (type) {
                case BoosterType.Shield:
                    MainPlayer.SetItem(ItemType.Armor);
                    break;
                case BoosterType.Key:
                    MainPlayer.JailBreak();
                    break;
            }
        }

        public bool PlayerIsInJail() {
            return MainPlayer.IsInJail;
        }

        public bool PlayerIsAlive() {
            return MainPlayer.IsAlive;
        }

        public bool PlayerIsTakeDamage() {
            return MainPlayer.IsTakeDamage;
        }

        public Vector2 GetPlayerPosition() {
            return MainPlayer.transform.position;
        }

        public void ShowFlyingText(int value) {
            MainPlayer.ShowFlyingText(value);
        }

        public HeroTakeDamageInfo GetHeroTakeDamageInfo() {
            return _entityManager.PlayerManager.TakeDamageInfo;
        }

        public void RevivePlayer(bool isTesting) {
            _entityManager.PlayerManager.Revive(_idxMainPlayer, isTesting);
            MainPlayer = _entityManager.PlayerManager.Players[_idxMainPlayer];
            _entityManager.LevelManager.Camera.SetTarget(MainPlayer);
        }
    }
}