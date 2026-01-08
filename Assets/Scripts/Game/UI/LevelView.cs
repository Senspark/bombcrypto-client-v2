using System;
using System.Threading.Tasks;

using App;

using Com.LuisPedroFonseca.ProCamera2D;

using CreativeSpore.SuperTilemapEditor;

using Cysharp.Threading.Tasks;

using Engine.Activity;
using Engine.Camera;
using Engine.Collision;
using Engine.Components;
using Engine.Entities;
using Engine.Manager;

using Senspark;

using Utils;

using UnityEngine;
using UnityEngine.UI;

using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Game.UI {
    public struct SceneCallback {
        public Action<bool> OnLevelCompleted;
    }

    public class LevelView : MonoBehaviour {
        [SerializeField]
        private Tileset[] tileSets;

        [SerializeField]
        private Sprite[] borderList;

        [SerializeField]
        private SpriteRenderer borderBG;

        [SerializeField]
        private TilemapGroup tmBackground;

        [SerializeField]
        private TilemapGroup tmMidground;

        [SerializeField]
        private TilemapGroup tmForeground;

        [SerializeField]
        private GameObject startLocation;

        public IEntityManager EntityManager { get; private set; }

        private IPlayerStorageManager _playerStoreManager;
        private IFeatureManager _featureManager;
        private ISoundManager _soundManager;
        private float _accumulatedTime;
        private SceneCallback _sceneCallback;
        private PlayingTimeTracker _playingTimeTracker;
        private ICamera _proCamera;

        public async UniTask Initialize(GameModeType gameMode, SceneCallback sceneCallback, 
            RectTransform bottomPanner, ProCamera2DPanAndZoom panAndZoom) {
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();

            var scene = SceneManager.GetActiveScene();
            var physicsScene = scene.GetPhysicsScene2D();

            _sceneCallback = sceneCallback;

            var manager = new DefaultEntityManager(tmForeground.gameObject, physicsScene, true);
            var items = GetComponentsInChildren<Entity>();
            foreach (var item in items) {
                if (item.IsAlive) {
                    manager.AddEntity(item);
                }
            }
            
            // In Game Manager
            manager.ExplodeEventManager = new HunterExplodeEventManager(manager);
            manager.TakeDamageEventManager = new DefaultTakeDamageEventManager(manager);

            // Level Manager
            var levelCallback = new LevelCallback {
                OnLevelCompleted = LevelCompleted
            };
            gameMode = AppConfig.IsTon() ? GameModeType.TreasureHuntV2 : gameMode;
            manager.LevelManager = new DefaultLevelManager(manager, levelCallback, 0, gameMode);

            // Map Manager
            var tileIndex = _playerStoreManager.TileSet;
            var ts = tileSets[tileIndex];
            if (tileIndex < borderList.Length) {
                var bg = borderList[tileIndex];
                borderBG.sprite = bg;
            }

            var density = 0.3f;
            var mapInfo = new MapInfo() {
                GameModeType = GameModeType.StoryMode,
                Col = 0,
                Row = 0,
            };
            // manager.MapManager = new DefaultMapManager(tileIndex, ts, manager, tmBackground, tmMidground, tmForeground, mapInfo);
            manager.MapManager = new DefaultMapManagerV2(tileIndex, ts, manager, tmBackground, tmMidground, tmForeground, mapInfo);
            await manager.MapManager.LoadMap();
            SetActiveMap(true);


            //Player Manager
            var locations = manager.MapManager.TakeEmptyLocations(_playerStoreManager.GetInMapPlayerCount());

            manager.PlayerManager = new DefaultPlayerManager(manager, startLocation.transform);
            await manager.PlayerManager.FirstInitPlayerPVE(locations);

            //add Activities
            manager.AddActivity(new DestructionActivity(manager));
            manager.AddActivity(new UpdateActivity(manager));

            //add Listener
            manager.AddListener(new DamageListener());
            manager.AddListener(new EncounterListener());
            manager.AddListener(new PlayerBombListener());
            manager.AddListener(new ItemListener());

            EntityManager = manager;
            // Ko play music ở dây nữa do refactor load scene kiểu mới ko chạy đc, scene nào cần thì tự gọi music
            //ChangeMusic();

            _playingTimeTracker = gameObject.AddComponent<PlayingTimeTracker>();
            _playingTimeTracker.Init(manager.LevelManager, manager.PlayerManager);
            _playingTimeTracker.BeginTrack();
            
            // Camera
            if (bottomPanner == null) {
                return;
            }
            panAndZoom.AutomaticInputDetection = false;
            if (Application.isMobilePlatform) {
                panAndZoom.UseMouseInput = false;
                panAndZoom.UseTouchInput = true;
            } else {
                panAndZoom.UseMouseInput = true;
                panAndZoom.UseTouchInput = false;
            }
            float maxHorizonScroll = 0;
            float minHorizonScroll = 0;
            float maxVerticalScroll = 0;
            float minVerticalScroll = 0;
            var cam = Camera.main;
            if (cam) {
                var heightCam = 2f * cam.orthographicSize;
                var widthCam = cam.aspect * heightCam;
                var w = (float) manager.MapManager.Col;
                var h = (float) manager.MapManager.Row;
                maxHorizonScroll = (w / 2) - (widthCam / 2) + 2.0f;
                minHorizonScroll = -maxHorizonScroll;
                maxVerticalScroll = (h / 2) - (heightCam / 2) + 0.5f;
                minVerticalScroll = -maxVerticalScroll;
                _proCamera = new ProCamera(ProCamera2D.Instance,
                    maxHorizonScroll, minHorizonScroll);
                var botPos = bottomPanner.TransformPoint(Vector3.zero);
                var bannerHeight = bottomPanner.TransformPoint(new Vector3(0, bottomPanner.sizeDelta.y, 0));
            }

        }

        public async Task AddNewPlayersOrRefresh(HeroId[] heroIds) {
            var locations = EntityManager.MapManager.TakeEmptyLocations(heroIds.Length);
            var playerManager = EntityManager.PlayerManager;
            for (var i = 0; i < locations.Count && i < heroIds.Length; i++) {
                var id = heroIds[i];
                var player = playerManager.GetPlayerById(id);
                var playerData = _playerStoreManager.GetPlayerDataFromId(id);
                if (player) {
                    playerManager.SetPropertiesAndAbility(player, playerData, false);
                } else {
                    if (playerManager.GetAllActivePlayerQuantity() >= 15) {
                        break;
                    }
                    var slot = playerManager.GetDicPlayersSlot(id);
                    await playerManager.AddPlayer(locations[i], playerData, slot, false);
                }
            }
            _playingTimeTracker.BeginTrack();
        }

        public async Task AddNewPlayersOrRefresh(IPveHeroDangerous data) {
            var locations = EntityManager.MapManager.TakeEmptyLocations(1);
            var playerManager = EntityManager.PlayerManager;
            var player = playerManager.GetPlayerById(data.HeroId);
            var playerData = _playerStoreManager.GetPlayerDataFromId(data.HeroId);
            var isDangerous = data.DangerousType == PveDangerousType.Danger;
            if (player) {
                playerManager.SetPropertiesAndAbility(player, playerData, isDangerous);
            } else {
                if (playerManager.GetActivePlayerQuantity() < 15) {
                    var slot = playerManager.GetDicPlayersSlot(data.HeroId);
                    await playerManager.AddPlayer(locations[0], playerData, slot, isDangerous);
                }
            }
            _playingTimeTracker.BeginTrack();
        }

        public void ShowThunder(HeroId heroId) {
            var playerManager = EntityManager.PlayerManager;

            var player = playerManager.GetPlayerById(heroId);
            var playerData = _playerStoreManager.GetPlayerDataFromId(heroId);
            var botManager = player.GetComponent<BotManager>();

            // Polygon có tính năng ko hiện nhiều effect
            var hideEffect = false;
            if (_featureManager.LimitHeroDangerousEffect) {
                if ((playerData.IsHeroS || playerData.Shield != null) && playerData.hp > 0 && playerData.hp % 10 != 0) {
                    hideEffect = true;
                }
            }

            if (hideEffect) {
                return;
            }

            var thunder = Thunder.Create(EntityManager, player.transform);
            thunder.StartAnimation(
                () => OnThunder(heroId, playerData, player),
                () => OnAfterThunder(heroId, playerData, botManager, player));
        }

        private void OnThunder(HeroId heroId, PlayerData playerData, Player player) {
            EntityManager.PlayerManager.SetThunderStrike(heroId, true);
            if ((playerData.IsHeroS || playerData.Shield != null) && playerData.hp > 0) {
                player.SetShieldEffectVisible(true);
            }
        }

        private void OnAfterThunder(HeroId heroId, PlayerData playerData, BotManager botManager, Player player) {
            EntityManager.PlayerManager.SetThunderStrike(heroId, false);
            // if (playerData.hp <= 0) {
            //     botManager.GoToSleep_SendRequest();
            // } else {
            //     botManager.ForceWork();
            // }
            if (playerData.IsHeroS || playerData.Shield != null) {
                player.SetShieldEffectVisible(false);
            }
        }

        public void Step(float delta) {
            EntityManager?.Step(delta);
            _playingTimeTracker?.Step(delta);
            _proCamera?.ProcessPanHorizontal(delta);
        }

        public void SaveMap() {
            EntityManager.MapManager.SaveMap();
        }

        private void LevelCompleted(bool win, IStoryModeEnterDoorResponse response) {
            //Hiện đổi qua dùng PVE_NEW_MAP nên bỏ new map chỗ này
            //_sceneCallback.OnLevelCompleted?.Invoke(win);
            _playingTimeTracker.StopAndSendTracking();
        }

        private void SetActiveMap(bool value) {
            if (tmMidground) {
                tmMidground.gameObject.SetActive(value);
            }
            tmForeground.gameObject.SetActive(value);
        }
        
    }
}