using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Actions;

using Analytics;

using App;

using BLPvpMode.Data;
using BLPvpMode.Engine;
using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.User;
using BLPvpMode.GameView;
using BLPvpMode.Manager;
using BLPvpMode.Manager.Api.Modules;
using BLPvpMode.UI;

using BomberLand.Bot;
using BomberLand.InGame;

using Constant;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Engine.Camera;
using Engine.Components;
using Engine.Entities;
using Engine.Manager;
using Game.Manager;
using Game.UI;

using JetBrains.Annotations;

using Notification;

using PvpMode.Manager;
using PvpMode.Services;
using PvpMode.UI;

using Reconnect;
using Reconnect.Backend;
using Reconnect.View;

using Senspark;

using Services;
using Services.IapAds;

using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.Assertions;

using Utils;

using ICommandManager = BLPvpMode.Manager.ICommandManager;

namespace Scenes.PvpModeScene.Scripts {
    /// <summary>
    /// Cái này để truyền cho các nơi khác set data vào
    /// </summary>
    public class PvpStorageData {
        public PvpDamageDealerType ReasonLose;
        public DateTime StartMatchTime;
        public DateTime EndMatchTime;
        public int ItemSkullHeadTake;
        public readonly int[] ItemsTake = new int[Enum.GetValues(typeof(ItemType)).Length];
        public int MeInPrisonCount;
    }

    public enum LevelResult {
        Win,
        Lose,
        Draw
    }

    public enum PvpDamageDealerType {
        Bomb = 0,
        HardBlock = 1,
        PrisonBreak = 2,
        Quit // Unused, FIXME.
    }

    public class BLLevelScenePvp : MonoBehaviour {
        public const float FixedDeltaTime = 1.0f / 60.0f;

        public class Participant {
            public IUser User { get; set; }
            public IBLParticipantGui Gui { get; set; }
        }

        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private Transform parent;

        [SerializeField]
        private Transform mapParent;

        [SerializeField]
        private StartCountDown startCountDown;

        [SerializeField]
        private Transform guiParent;

        [SerializeField]
        private BLGuiResource guiResource;

        private bool _pause = true;
        private ILevelViewPvp _levelView;
        private ProCamera _proCamera;

        private ILogManager _logManager;
        private ISceneManager _sceneLoader;
        private IServerManager _serverManager;
        private IStorageManager _storageManager;
        private IRankInfoManager _rankInfoManager;
        private IAnalytics _analytics;
        private IUnityAdsManager _unityAdsManager;
        private INotificationManager _notificationManager;
        private ITimeManager _timeManager;
        private ObserverHandle _handle;

        private Participant[] _participants;
        private List<IUserModule> _modules; // Modules from the previous scenes.
        private List<IUserModule> _botModules;
        private IMatchInfo _matchInfo;
        private IMatchData _matchData;
        private IMapInfo _mapInfo;
        private PvpSyncing[] _syncHeroes;

        private BoosterStatus _boosterStatus;
        private PvpStorageData _pvpStorageData;
        private BLPvpClientBridge _pvpClientBridge;

        private IBLGui _guiPvp;
        private BLGuiPvp GuiPvp => _guiPvp as BLGuiPvp;

        private int Slot => _matchInfo.Slot;

        private List<long> _timeToShowHurryUp;

        public bool IsCountDownEnabled { get; set; }

        private IReconnectStrategy _gameReconnectStrategy;
        private IReconnectStrategy _pvpReconnectStrategy;

        private bool _ready;
        
        private void Awake() {
            _unityAdsManager = ServiceLocator.Instance.Resolve<IUnityAdsManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _sceneLoader = ServiceLocator.Instance.Resolve<ISceneManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _rankInfoManager = ServiceLocator.Instance.Resolve<IRankInfoManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _notificationManager = ServiceLocator.Instance.Resolve<INotificationManager>();
            _timeManager = new EpochTimeManager();
            _handle = new ObserverHandle();

            Physics2D.simulationMode = SimulationMode2D.Script;
            Physics2D.gravity = Vector2.zero;
            _pvpStorageData = new PvpStorageData();
            _analytics.TrackScene(SceneType.PlayPvp);

            _timeToShowHurryUp = new List<long>() { 65, 35, 5 };
            _gameReconnectStrategy = new DefaultReconnectStrategy(
                _logManager,
                new MainReconnectBackend(),
                new LogReconnectView()
            );
            _gameReconnectStrategy.Start();
        }

        // Đổi hàm Start thành StartPvpGame
        // Không tự động Start khi load Scene nữa
        // Mà sẽ gọi sau khi SetPvpMapDetails
        private void StartPvpGame() {
            StartGame();
            var bombable = _levelView.EntityManager.PlayerManager.GetPlayerBySlot(Slot).Bombable;
            _guiPvp.SetQuantityBomb(bombable.CountAvailableBomb());
            _ready = true;
        }

        private void InitGui(GuiType type) {
            _guiPvp = guiResource.CreateBLGui(type, guiParent);
        }

        private void OnDestroy() {
            _pvpClientBridge.Dispose();
            // foreach (var item in _modules) {
            //     item.Dispose();
            // }
            // _modules.Clear();
            foreach (var item in _botModules) {
                item.Dispose();
            }
            _botModules.Clear();
            //Comment dispose user do dispose thì ko thể chơi tiếp trong Bo3, Bo5...
            // foreach (var item in _participants) {
            //     item.User.Dispose();
            // }
            DOTween.KillAll(true);
            _handle.Dispose();
            _levelView = null;
            _gameReconnectStrategy.Dispose();
            _pvpReconnectStrategy.Dispose();
        }

        private void Update() {
            if (!_ready) {
                return;
            }
            if (_pause) {
                return;
            }
            var canProcessLogic = _matchData.Status == MatchStatus.Started;
            if (canProcessLogic) {
                _guiPvp.CheckInputKeyDown();
                // process movement from direction input
                foreach (var participant in _participants) {
                    _levelView.ProcessMovement(participant.User.MatchInfo.Slot,
                        participant.Gui.GetDirectionFromInput());
                }
            }
            // Update Entity
            var delta = Time.deltaTime;
            if (delta > FixedDeltaTime) {
                delta = FixedDeltaTime;
            }
            if (canProcessLogic) {
                foreach (var item in _botModules) {
                    item.Update(delta);
                }
            }
            _levelView.Step(delta);
            _proCamera.Process(delta);
            _pvpClientBridge.Step(delta);

            // Process Stage server
            foreach (var syncHero in _syncHeroes) {
                if (!syncHero.IsAlive) {
                    continue;
                }
                if (canProcessLogic) {
                    syncHero.CommandManager.ProcessUpdate(delta);
                }
                var slot = syncHero.Slot;
                if (syncHero.IsBot || slot == Slot) {
                    // sync pos current player or bot to server
                    syncHero.SyncMove(delta);
                } else {
                    // gen pos opponent
                    syncHero.SyncPos();
                }
            }

            var elapsed = _timeManager.Timestamp - _matchData.RoundStartTimestamp;
            var timeRemain = _mapInfo.PlayTime - elapsed;
            _guiPvp.UpdateRemainTime(timeRemain);

            var seconds = (int) (timeRemain / 1000);
            if (_timeToShowHurryUp.Contains(seconds)) {
                _guiPvp.ShowHurryUp();
                _timeToShowHurryUp.Remove(seconds);
            }

            var bombable = _levelView.EntityManager.PlayerManager.GetPlayerBySlot(Slot).Bombable;
            _guiPvp.SetQuantityBomb(bombable.CountAvailableBomb());
        }

        private void RemoveAllTimeToHurryUp() {
            _timeToShowHurryUp.Clear();
        }

        public void SetPvpMapDetails(
            IUser[] users,
            List<IUserModule> modules,
            IMatchData matchData,
            IMapInfo mapInfo,
            BoosterStatus boosterStatus
        ) {
            var ratio = ScreenUtils.GetScreenRatio();
            if (AppConfig.IsTon() || !ScreenUtils.IsIPadScreen()) {
                InitGui(GuiType.PvpPlay);
            } else {
                InitGui(GuiType.PadPvpPlay);
            }
            Assert.IsTrue(users.Length > 0);
            var currentUser = users.FirstOrDefault(user => !user.IsBot && user.IsParticipant) ?? users.First();
            var matchInfo = currentUser.MatchInfo;
            _matchInfo = matchInfo;
            _pvpClientBridge = new BLPvpClientBridge(_logManager, currentUser, mapInfo, _pvpStorageData);
            _matchData = new ClientMatchData(matchData, _pvpClientBridge, matchInfo.Slot);
            _mapInfo = mapInfo;
            _boosterStatus = boosterStatus;

            _participants = users.Select(item => new Participant {
                User = item,
                Gui = item.IsBot
                    ? new BLBotBridge(item.MatchInfo.Slot)
                    : _guiPvp.GetParticipantGui(), // Interactive participant.
            }).ToArray();
            _modules = modules;
            _botModules = new List<IUserModule>();
            var interactiveParticipant = _participants.FirstOrDefault(item => !item.User.IsBot);
            if (interactiveParticipant == null) {
                // All users are bot.
            } else {
                var interactiveUser = interactiveParticipant.User;
                void ShowQuitDialog(Action callback) =>
                    _guiPvp.ShowDialogQuit(canvasDialog, callback, () => _pause = false);
                if (interactiveUser.IsParticipant) {
                    var slot = currentUser.MatchInfo.Slot;
                    var info = _matchInfo.Info[slot];
                    var hero = info.Hero;
                    _handle.AddObserver(interactiveParticipant.Gui, new BLGuiObserver {
                            SpawnBomb = () => {
                                UniTask.Void(async () => {
                                    try {
                                        if (_levelView.CheckSpawnPvpBomb(slot)) {
                                            await _syncHeroes[slot].PlantBomb();
                                        } else {
#if ENABLE_THROW_BOMB
                                            await interactiveUser.ThrowBomb();
#endif
                                        }
                                    } catch (Exception e) {
                                        _logManager.Log($"plant bomb error: {e.Message}");
                                    }
                                });
                            },
                            UseBooster = item => {
                                UniTask.Void(async () => {
                                    try {
                                        if (!_syncHeroes.All(entry => entry.IsAlive)) {
                                            // Pre-check.
                                            // FIXME: only works for 1v1.
                                            throw new Exception("Invalid status");
                                        }
                                        await interactiveUser.UseBooster(
                                            (Booster) DefaultBoosterManager.ConvertFromEnum(item));
                                    } catch {
                                        _guiPvp.UpdateButtonBoosterFailToUse();
                                    }
                                });
                            },
                            UseEmoji = itemId => interactiveUser.UseEmoji(itemId),
                            RequestQuit = () => {
                                if (interactiveUser.IsParticipant && _syncHeroes.Count(entry => entry.IsAlive) == 1) {
                                    // Pre-check.
                                    // FIXME: only works for 1v1.
                                    return;
                                }
                                ShowQuitDialog(RequestQuitPvp);
                            },
                        });
                    // 
                    _guiPvp.UpdateDamageUi(hero.Damage);

                    //show emoji buttons theo skinChest with type emoji;
                    var skinChest = hero.SkinChests;
                    const int itemType = (int) InventoryItemType.Emoji;
                    if (skinChest.ContainsKey(itemType)) {
                        var itemIds = skinChest[itemType].ToArray();
                        _guiPvp.InitEmojiButtons(itemIds);
                    }
                } else {
                    // Observer.
                    _handle.AddObserver(interactiveParticipant.Gui, new BLGuiObserver {
                            RequestQuit = () => ShowQuitDialog(() => {
                                foreach (var item in _participants) {
                                    item.User.Disconnect();
                                }
                                GoToPvpMenu();
                            }),
                        });
                }
            }
            var callback = new PvpManagerCallback {
                OnResponseStartReady = ShowReadyDialog,
                OnResponseFinishRound = OnFinishRound,
                OnResponseLatency = OnUpdateLatency,
                OnResponseFallingBlock = AddFallingWall,
                OnResponseFinishMatch = FinishMatch,
                OnResponseUseEmoji = UseEmoji,
                OnUpdateHealth = OnUpdateHealth,
                OnKillHero = KillHero,
                SetShielded = SetShielded,
                OnChangeStateImprisoned = SetImprisoned,
                OnExplodeBomb = ExplodeBomb,
                AddItemToPlayer = AddItemToPlayer,
                SetSkullHeadToPlayer = SetSkullHeadToPlayer,
                AddItem = AddItem,
                RemoveBlock = RemoveBlock,
                RemoveBomb = RemoveBomb,
                RemoveItem = RemoveItem,
                SpawnBomb = SpawnBomb,
                SyncPosPlayer = SyncPosPlayer,
            };
            _pvpClientBridge.PvpCallback = callback;
            _guiPvp.UpdatePvpInfo(matchInfo.Id, matchInfo.ServerId, matchInfo.Slot, matchInfo.Info, boosterStatus);
            _pvpReconnectStrategy = new MultiReconnectStrategy(users.Select(it =>
                (IReconnectStrategy) new DefaultReconnectStrategy(
                    _logManager,
                    new PvpReconnectBackend(it),
                    it.IsBot
                        ? new LogReconnectView()
                        : new WaitingReconnectView(canvasDialog)
                )).ToArray());
            _pvpReconnectStrategy.Start();
            
            StartPvpGame();
        }

        private void StartGame() {
            _notificationManager.CancelSeasonAlreadyNotification();
            LoadLevel();
            CreateSyncHeroes();
            if (IsCountDownEnabled) {
                _pause = true;
                startCountDown.StartCount(() => {
                    _pause = false;
                    _pvpStorageData.StartMatchTime = DateTime.Now;
                });
            } else {
                startCountDown.gameObject.SetActive(false);
                _pvpStorageData.StartMatchTime = DateTime.Now;
            }
            foreach (var item in _participants) {
                var user = item.User;
                var slot = user.MatchInfo.Slot;
                if (user.IsBot) {
                    _botModules.Add(new BotModule(
                        _logManager,
                        user,
                        (BLBotBridge) item.Gui,
                        _syncHeroes[slot].CommandManager,
                        _levelView
                    ));
                }
            }
            for (var slot = 0; slot < _matchInfo.Info.Length; slot++) {
                var info = _matchInfo.Info[slot];
                _guiPvp.InitHealthUi(slot, info.Hero.Health);
            }
        }

        private ICommandManager CreateAllyCommandManager(IUser user, int slot, Vector2 posStart) {
            // Participant.
            var participantMoveManager = new ParticipantMoveManager(
                user,
                _timeManager,
                posStart,
                (PvpMovable) _levelView.GetPvpHeroBySlot(slot).Movable
            );
            // FIXME.
            // if (_botManager.HasBot()) {
            //     // set step time sync move is 1.0s
            //     participantMoveManager.TimeToSendBundleMove = 1000;
            // }
            var participantPlantBombManager = new ParticipantPlantBombManager(
                user,
                _timeManager,
                slot,
                (bombId, position) => {
                    if (_levelView == null) {
                        return;
                    }
                    var viewPosition = new Vector2Int(position.x, position.y);
                    _levelView.SpawnBomb(slot, bombId, 0, viewPosition);
                }
            );
            return new CommandManager(
                participantMoveManager,
                participantPlantBombManager
            );
        }

        private ICommandManager CreateObserverCommandManager(int slot, Vector2 posStart) {
#if USE_OBSERVER_TEST
            var moveManager = new ObserverMoveManagerTest(_timeManager, 1500, posStart);
#else
            var moveManager = new ObserverMoveManager(_timeManager, 1500, posStart);
#endif
            var plantBombManager = new ObserverPlantBombManager(
                _timeManager,
                slot,
                (bombId, position) => {
                    if (_levelView == null) {
                        return;
                    }
                    var viewPosition = new Vector2Int(position.x, position.y);
                    _levelView.SpawnBomb(slot, bombId, 0, viewPosition);
                }
            );
            return new CommandManager(moveManager, plantBombManager);
        }

        private void CreateSyncHeroes() {
            var pvpHeroes = _levelView.GetPvpHeroes();
            _syncHeroes = new PvpSyncing[pvpHeroes.Count];
            for (var idx = 0; idx < pvpHeroes.Count; idx++) {
                var slot = idx;
                var player = pvpHeroes[slot];
                FactionType faction;
                ICommandManager commandManager;
                var posStart = new Vector2(
                    _mapInfo.StartingPositions[slot].x + 0.5f,
                    _mapInfo.StartingPositions[slot].y + 0.5f
                );
                var participant = _participants.FirstOrDefault(item => item.User.MatchInfo.Slot == slot);
                if (participant?.User.IsParticipant ?? false) {
                    faction = FactionType.Ally;
                    commandManager = CreateAllyCommandManager(participant.User, slot, posStart);
                } else {
                    faction = FactionType.Enemy;
                    commandManager = CreateObserverCommandManager(slot, posStart);
                }
                player.SetSlotAndFaction(slot, faction);
                player.ShowMe(slot == Slot);
                player.ForceToPosition(posStart);
                _syncHeroes[slot] = new PvpSyncing(player, commandManager, slot, participant?.User?.IsBot ?? false);
            }
        }

        private void LoadLevel() {
            _levelView = BLevelViewPvp.Create(mapParent);
            var pvpModeCallback = new PvpModeCallback {
                OnKick = OnKick,
                OnPlayerInJail = OnPlayerInJail,
                OnPlayerEndInJail = OnPlayerEndInJail,
                OnUpdateItem = OnUpdateItem,
            };
            _levelView.Initialize(
                _matchInfo.Info.Select(item => item.Hero).ToArray(),
                _mapInfo,
                _matchData,
                pvpModeCallback,
                mapParent.gameObject.layer,
                null
            );
            if (_matchInfo.IsParticipant()) {
                _levelView.UpdateState(Slot);
            }

            _proCamera = GuiPvp.SetUpCamera(_mapInfo.Width, _mapInfo.Height);
            _proCamera.SetTarget(_levelView.EntityManager.PlayerManager.GetPlayer(Slot));

            _levelView.ShowHealthBarOnPlayerNotSlot(Slot);
        }

        #region Pvp Bridge callback

        private void KillHero(int slot, HeroDamageSource dealer) {
            _levelView.KillHero(slot, dealer);
        }

        private void ExplodeBomb(int slot, int id, [NotNull] Dictionary<Direction, int> ranges) {
            _levelView.ExplodeBomb(slot, id, ranges);
        }

        private void FinishMatch(IPvpResultInfo info) {
            RemoveAllTimeToHurryUp();
            _pvpStorageData.EndMatchTime = DateTime.Now;
            var userInfo = info.Info[Slot];
            UniTask.Void(async () => {
                await WebGLTaskDelay.Instance.Delay(2000);
                // Wait for server data.
                // Attempt 3 times.
                IPvpClaimMatchRewardResult reward = null;
                for (var i = 0; i < 3; ++i) {
                    _logManager.Log($"FinishMatch: attempt={i + 1}");
                    try {
                        reward = await _serverManager.Pvp.ClaimMatchReward();
                        break;
                    } catch (Exception ex) {
                        Debug.LogException(ex);
                    }
                }
                var rewardId = reward?.RewardId ?? "";
                var isOutOfChest = reward?.IsOutOfChest ?? false;
                var result = ParseLevelResult(info, Slot);
                TrackBoosters(userInfo);
                TrackPassiveBoosters(result);
                TrackPrison();
                if (result != LevelResult.Draw) {
                    TrackCollectItems(result);
                    TrackMatchResult(info, result, Slot);
                    TrackFullInventory(isOutOfChest);
                }
                
                await _rankInfoManager.ReloadData();
                ShowResultPopup(info, rewardId, isOutOfChest);
                
                // Disable reconnection when match is finished.
                _pvpReconnectStrategy.Dispose();
            });
        }

        private void AddItemToPlayer(int slot, HeroItem item, int value) {
            _levelView.AddItemToPlayer(Slot == slot, slot, item, value);
            if (Slot == slot) {
                var position = _levelView.GetPvpHeroBySlot(Slot).transform.position;
                _guiPvp.FlyItemReward(parent, position, item);
            }
        }

        private void SetSkullHeadToPlayer(int slot, HeroEffect effect, int duration) {
            _levelView.SetSkullHeadToPlayer(Slot == slot, slot, effect, duration);
        }

        private void UseEmoji(int slot, int itemId) {
            _guiPvp.ShowEmojiUi(slot, itemId);
        }

        private void AddItem(Vector2Int position, ItemType type) {
            _levelView.CreateItem(position, type);
        }

        private void RemoveBlock(Vector2Int position) {
            _levelView.RemoveBlock(position);
        }

        private void RemoveBomb(int slot, int id) {
            _logManager.Log($"RemoveBomb slot:{slot} id:{slot}");
            _levelView.RemoveBomb(slot, id);
        }

        private void RemoveItem(Vector2Int location) {
            _levelView.RemoveItem(location);
        }

        private void SpawnBomb(int timestamp, int slot, Vector2Int location, int bombId, BombReason reason) {
            // Observer plant bomb events.
            _syncHeroes[slot].CommandManager.ReceivePacket(new ObserverPlantBombPacket {
                Timestamp = timestamp, // 
                Slot = slot,
                BombId = bombId,
                Position = location,
                Reason = reason,
            });
        }

        private void AddFallingWall(IFallingBlockInfo[] blocks) {
            _levelView.AddFallingWall(blocks);
        }

        private void SetShielded(int slot, bool active, HeroEffectReason reason) {
            if (active) {
                _levelView.SetShieldToPlayer(slot);
                if (reason == HeroEffectReason.UseBooster && Slot == slot) {
                    OnUseBooster(BoosterType.Shield);
                }
            } else {
                if (reason == HeroEffectReason.Damaged) {
                    _levelView.HeroShieldBroken(slot);
                }
            }
        }

        private void SetImprisoned(int slot, bool active, HeroEffectReason reason) {
            if (active) {
                _levelView.SetPlayerInJail(slot);

                // Track trường hợp player bị sập tù (ko track đối thủ)
                if (slot == Slot) {
                    _pvpStorageData.MeInPrisonCount++;
                }
            } else {
                _levelView.JailBreak(slot);
                switch (reason) {
                    case HeroEffectReason.UseBooster when Slot == slot:
                        OnUseBooster(BoosterType.Key);
                        return;
                    case HeroEffectReason.Damaged:
                        PlayKillEffectOnOther(slot);
                        break;
                }
            }
        }

        private void PlayKillEffectOnOther(int slot) {
            _levelView.PlayKillEffectOnOther(slot);
        }

        private void SyncPosPlayer(long timestamp, int slot, Vector2 position, Direction direction) {
            if (slot == Slot) {
                // Ignore own.
                return;
            }
            var syncHero = _syncHeroes.FirstOrDefault(item => item.Slot == slot);
            if (syncHero == null) {
                return;
            }
            if (!syncHero.IsAlive || syncHero.IsBot) {
                return;
            }
            syncHero.UpdateLocalPosition(timestamp, position, direction);
        }

        #endregion

        #region Gui callback

        private void RequestQuitPvp() {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await Task.WhenAll(_participants
                        .Where(it => !it.User.IsBot)
                        .Select(item => item.User.Quit()).Cast<Task>().ToArray());
                    _pvpStorageData.ReasonLose = PvpDamageDealerType.Quit;
                } catch (Exception e) {
                    _guiPvp.ShowDialogOk(canvasDialog, e.Message);
                } finally {
                    waiting.End();
                }
            });
        }

        #endregion

        private void GoToPvpMenu() {
            if (_matchInfo.Rule.IsTournament) {
                GotoPvpSchedule();
            } else {
                UniTask.Void(async () => {
                    await _serverManager.General.GetChestReward();
                    const string sceneName = "MainMenuScene";
                    SceneLoader.LoadSceneAsync(sceneName).Forget();
                });
            }
        }

        private void GotoPvpSchedule() {
            UniTask.Void(async () => {
                const string sceneName = "MainMenuScene";
                void OnLoaded(GameObject obj) {
                    var mainMenu = obj.GetComponent<MainMenuScene.Scripts.MainMenuScene>();
                    mainMenu.ShowDialogPvpSchedule();
                }
                await SceneLoader.LoadSceneAsync(sceneName, OnLoaded);
            });
        }

        private void OnKick() {
            ServiceLocator.Instance.Resolve<IServerManager>().Disconnect();
            var reason = "The account is having a data conflict with the Battle mode server";
            _guiPvp.ShowErrorAndKick(canvasDialog, reason);
        }

        private void OnUseBooster(BoosterType type) {
            _guiPvp.UpdateButtonBoosterUsed(type, _boosterStatus, () => _levelView.PlayerIsInJail(Slot));
            _analytics.Pvp_TrackActiveBooster(type.ToString());
        }

        private void OnPlayerInJail(int slot) {
            if (slot == Slot) {
                _guiPvp.UpdateButtonInJail(_boosterStatus);
            }
        }

        private void OnPlayerEndInJail(int slot) {
            if (slot == Slot) {
                _guiPvp.UpdateButtonEndInJail(_boosterStatus);
            }
        }

        private void OnUpdateItem(int slot, ItemType item, int value) {
            if (slot == Slot) {
                _guiPvp.UpdateItemUi(item, value);
            }
        }

        private void HideAllDialog() {
            _guiPvp.HideAllDialog(canvasDialog);
        }

        private void ShowResultPopup(IPvpResultInfo info, string rewardId, bool isOutOfChest) {
            ServiceLocator.Instance.Resolve<IBLTutorialManager>().IncreaseTimePlayPvp();
            HideAllDialog();
            var slot = Slot;
            var result = ParseLevelResult(info, slot);
            var isTournament = _matchInfo.Rule.IsTournament;
            var boosters = _matchInfo.Info[Slot].Boosters;
            if (result == LevelResult.Win) {
                _guiPvp.ShowDialogPvpVictory(canvasDialog, info, slot, rewardId, isOutOfChest, OnClaim, isTournament,
                    boosters);
                return;
            }
            if (result == LevelResult.Lose) {
                _guiPvp.ShowDialogPvpDefeat(
                    canvasDialog, result, info, slot, rewardId, isOutOfChest, OnClaim, isTournament, boosters);
                return;
            }
            _guiPvp.ShowDialogPvpDrawOrLose(
                canvasDialog, result, info, slot, rewardId, isOutOfChest, OnClaim, isTournament);
        }

        private static LevelResult ParseLevelResult(IPvpResultInfo info, int slot) {
            if (info.IsDraw) {
                return LevelResult.Draw;
            }
            return info.WinningTeam != info.Info[slot].TeamId ? LevelResult.Lose : LevelResult.Win;
        }

        private void OnClaim() {
            GoToPvpMenu();
        }

        private void ShowReadyDialog() {
            void OnLoaded(GameObject obj) {
                var pvpReady = obj.GetComponent<PvpReadyScene.Scripts.PvpReadyScene>();
                var scores = Enumerable.Repeat(0, _matchInfo.Info.Length).ToArray();
                foreach (var iter in _matchData.Results) {
                    for (var i = 0; i < iter.Scores.Length; i++) {
                        scores[i] += iter.Scores[i];
                    }
                }
                var strRound = $"{_matchData.Round}/{_matchInfo.Rule.Round}";
                var strScore = $"{scores[0]}-{scores[1]}";
                var users = _participants.Select(item => item.User).ToArray();
                pvpReady.SetInfo(users, _modules, strRound, strScore);
            }
            const string sceneName = "PvpReadyScene";
            SceneLoader.LoadSceneAsync(sceneName, OnLoaded).Forget();
        }

        private void OnFinishRound(IMatchData matchData) {
            _matchData = matchData;
            _handle.Dispose();
            //Tạm đưa về FinishMatch để fix lỗi pvp bị đứng
            // Disable reconnection when match is finished.
            //_pvpReconnectStrategy.Dispose();
        }

        private void OnUpdateLatency(int[] latencies) {
            _guiPvp.SetMainLatency(latencies[Slot]);
            for (var i = 0; i < _matchInfo.Info.Length && i < 2; ++i) {
                _guiPvp.SetLatency(i, latencies[i]);
            }
            // FIXME: _syncHeroes may be null when update latency is called too soon.
            if (_syncHeroes == null) {
                return;
            }
            for (var i = 0; i < _syncHeroes.Length; ++i) {
                var syncHeroes = _syncHeroes[i];
                syncHeroes.UpdateMovementLatency(latencies[i] + latencies[Slot]);
            }
        }

        private void OnUpdateHealth(int slot, int value) {
            _guiPvp.UpdateHealthUi(slot, value);
            if (Slot == slot) {
                _levelView.ShakeCamera();
                EventManager.Dispatcher(PlayerEvent.OnDamage);
            } else {
                _levelView.UpdateHealthBar(slot, value);
            }
            _levelView.SetImmortal(slot);
        }

        #region ANALYTICS

        private void TrackFullInventory(bool isOutOfChestSlot) {
            if (!isOutOfChestSlot) {
                return;
            }
            var data = new Dictionary<TrackPvpRejectChest, int> {
                [TrackPvpRejectChest.BronzeChest] = _pvpStorageData.ItemsTake[(int) ItemType.BronzeChest],
                [TrackPvpRejectChest.SilverChest] = _pvpStorageData.ItemsTake[(int) ItemType.SilverChest],
            };
            _analytics.Pvp_TrackFullInventory(data);
        }

        private void TrackBoosters(IPvpResultUserInfo info) {
            var boosters = info.UsedBoosters;
            foreach (var (id, used) in boosters) {
                var boosterId = (GachaChestProductId) id;
                switch (boosterId) {
                    case GachaChestProductId.Key:
                    case GachaChestProductId.Shield:
                    case GachaChestProductId.RankGuardian:
                    case GachaChestProductId.FullRankGuardian:
                    case GachaChestProductId.ConquestCard:
                    case GachaChestProductId.FullConquestCard:
                    case GachaChestProductId.BombAdd1:
                    case GachaChestProductId.SpeedAdd1:
                    case GachaChestProductId.RangeAdd1:
                        _analytics.TrackConversion(ConversionConvert.ConvertUseBoosterPvp(id));
                        break;
                    default:
                        _logManager.Log($"Could not find booster id: {boosterId}");
                        break;
                }
            }
        }

        private void TrackPrison() {
            _analytics.Pvp_TrackInPrison(_pvpStorageData.MeInPrisonCount);
        }

        private void TrackCollectItems(LevelResult result) {
            var matchResult = ConvertMatchResultForAnalytics(result);
            var data = new Dictionary<TrackPvpCollectItemType, int>() {
                [TrackPvpCollectItemType.SkullHead] = _pvpStorageData.ItemSkullHeadTake,
                [TrackPvpCollectItemType.Armor] = _pvpStorageData.ItemsTake[(int) ItemType.Armor],
                [TrackPvpCollectItemType.Boots] = _pvpStorageData.ItemsTake[(int) ItemType.Boots],
                [TrackPvpCollectItemType.Gold1] = _pvpStorageData.ItemsTake[(int) ItemType.GoldX1],
                [TrackPvpCollectItemType.Gold5] = _pvpStorageData.ItemsTake[(int) ItemType.GoldX5],
                [TrackPvpCollectItemType.BombUp] = _pvpStorageData.ItemsTake[(int) ItemType.BombUp],
                [TrackPvpCollectItemType.BronzeChest] = _pvpStorageData.ItemsTake[(int) ItemType.BronzeChest],
                [TrackPvpCollectItemType.FireUp] = _pvpStorageData.ItemsTake[(int) ItemType.FireUp],
                [TrackPvpCollectItemType.SilverChest] = _pvpStorageData.ItemsTake[(int) ItemType.SilverChest],
                [TrackPvpCollectItemType.SkullHead] = _pvpStorageData.ItemsTake[(int) ItemType.SkullHead],
            };
            _analytics.Pvp_TrackCollectItems(data, matchResult);
        }

        private static TrackPvpMatchResult ConvertMatchResultForAnalytics(LevelResult result) {
            return result switch {
                LevelResult.Win => TrackPvpMatchResult.Win,
                LevelResult.Lose => TrackPvpMatchResult.Lose,
                LevelResult.Draw => TrackPvpMatchResult.Draw,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
        }

        private void TrackMatchResult(IPvpResultInfo info, LevelResult result, int slot) {
            // reward.Losers là danh sách số slot bị thua, ko phải id của user bị thua
            // Phải lấy số slot để tìm ra đó là user nào.
            // FIXME: handle winningTeam.
            var loserUserId =
                info.IsDraw ? -1 : info.Info.Where((_, index) => index != info.WinningTeam).First().UserId;
            var winnerUserId = info.IsDraw ? -1 : info.Info[info.WinningTeam].UserId;
            var matchResult = ConvertMatchResultForAnalytics(result);
            var loseReason = _pvpStorageData.ReasonLose switch {
                PvpDamageDealerType.Bomb => TrackPvpLoseReason.Prison,
                PvpDamageDealerType.HardBlock => TrackPvpLoseReason.BlockDrop,
                PvpDamageDealerType.PrisonBreak => TrackPvpLoseReason.Prison,
                PvpDamageDealerType.Quit => TrackPvpLoseReason.Quit,
                _ => throw new ArgumentOutOfRangeException()
            };
            var matchTime = (int) (_pvpStorageData.EndMatchTime - _pvpStorageData.StartMatchTime).TotalSeconds;
            _analytics.Pvp_TrackPlay(winnerUserId, loserUserId, matchTime, matchResult,
                loseReason);

            var userInfo = info.Info[slot];
            _analytics.TrackConversionPvpPlay(userInfo.MatchCount, userInfo.WinMatchCount);
        }

        private void TrackPassiveBoosters(LevelResult matchResult) {
            BoosterType[] trackThese = null;
            if (matchResult == LevelResult.Win) {
                trackThese = new[] { BoosterType.CupBonus, BoosterType.FullCupBonus };
            } else if (matchResult == LevelResult.Lose) {
                trackThese = new[] { BoosterType.RankGuardian, BoosterType.FullRankGuardian, };
            }
            if (trackThese != null) {
                foreach (var b in trackThese) {
                    if (_boosterStatus.IsChooseBooster(b)) {
                        _analytics.Pvp_TrackActiveBooster(b.ToString());
                    }
                }
            }
        }

        #endregion

        [Button]
        public void TestDisServerMain() {
            _serverManager.Disconnect();
        }

        [Button]
        public void TestDisServerPvp() {
            foreach (var item in _participants) {
                item.User.Disconnect();
            }
        }

        [Button]
        public void KillConnection() {
            _participants[0].User.KillConnection();
        }

        [Button]
        public void TestQuit() {
            _participants[0].User.Quit();
        }
        
    }
}