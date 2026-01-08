using System;
using System.Collections.Generic;

using Analytics;

using App;

using BLPvpMode.Data;
using BLPvpMode.Engine.Entity;
using BLPvpMode.UI;

using BomberLand.Component;
using BomberLand.DirectionInput;
using BomberLand.InGame;

using Constant;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Engine.Camera;
using Engine.Entities;
using Engine.Manager;

using Game.Dialog;
using Game.Manager;
using Game.UI;

using PvpMode.Manager;

using Reconnect;
using Reconnect.Backend;
using Reconnect.View;

using Scenes.PvpModeScene.Scripts;

using Senspark;

using Services.IapAds;

using Share.Scripts.Dialog;

using StoryMode.UI;

using UnityEngine;

using Utils;

namespace Scenes.StoryModeScene.Scripts {
    public class LevelSceneStoryMode : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private Transform parent;

        [SerializeField]
        private Transform guiParent;

        [SerializeField]
        private BLGuiResource guiResource;

        private bool Pause { get; set; } = true;
        private bool AutoNextLevel => _displayPlayingTime;

        private IServerManager _serverManager;
        private IStoryModeManager _storyModeManager;
        private LevelViewStoryMode _levelView;

        private IAnalytics _analytics;
        private ISoundManager _soundManager;
        private ILogManager _logManager;

        private StoryModeTicketType _ticketType;
        private JoyStickDirectionInput _joystickDirectionInput;
        private JoyStickDirectionInput _joyPadDirectionInput;

        private ICamera _camera;

        private IStoryMapDetail _storyMapDetails;
        private IHeroDetails _storyModeHero;
        private IBossSkillDetails _bossSkillDetails;
        private IStoryModeEnterDoorResponse _enterDoorResponse;
        private IUnityAdsManager _unityAdsManager;

        // Playing Time Track
        private bool _countingPlayingTime;
        private float _totalPlayingTime;
        private float _levelPlayingTime;
        private bool _displayPlayingTime;

        private BoosterStatus _boosterStatus;
        private ObserverHandle _handle;

        private IBLGui _guiPve;
        private int _idxMainPlayer = 0;
        private IReconnectStrategy _reconnectStrategy;
        private bool _isTesting;

        private void Awake() {
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _unityAdsManager = ServiceLocator.Instance.Resolve<IUnityAdsManager>();

            Physics2D.simulationMode = SimulationMode2D.Script;
            Physics2D.gravity = Vector2.zero;

            _analytics.TrackScene(SceneType.PlayPve);
            _handle = new ObserverHandle();
            _reconnectStrategy = new DefaultReconnectStrategy(
                _logManager,
                new MainReconnectBackend(),
                LoadSceneReconnectView.ToOtherScene(canvasDialog, nameof(AdventureMenuScene.Scripts.AdventureMenuScene))
            );
            _reconnectStrategy.Start();
        }

        
        // Đổi hàm Start thành StartGame
        // Không tự động Start khi load Scene nữa
        // Mà sẽ gọi sau khi SetStoryMapDetails
        private void StartGame() {
            LoadLevel();
            TrackUseBoosterAtLevelStart();
            Pause = false;
        }

        private void InitGui() {
            if (_isTesting) {
                _guiPve = guiResource.CreateBLGui(GuiType.Testing, guiParent);
                _handle.AddObserver(_guiPve.GetParticipantGui(), new BLGuiObserver() {
                    SpawnBomb = OnBombButtonClicked, UseBooster = null, RequestQuit = ShowDialogQuit
                });
            } else {
                _guiPve = guiResource.CreateBLGui(GuiType.Pve, guiParent);
                _handle.AddObserver(_guiPve.GetParticipantGui(), new BLGuiObserver() {
                    SpawnBomb = OnBombButtonClicked, UseBooster = OnBoosterButtonClicked, RequestQuit = ShowDialogQuit
                });
            }
        }

        private void OnDestroy() {
            _soundManager.StopMusic();
            DOTween.KillAll(true);
            _handle.Dispose();
            _reconnectStrategy.Dispose();
        }

        private void Update() {
            if (Pause) {
                return;
            }

            _guiPve.CheckInputKeyDown();

            // process movement from direction input
            _levelView.ProcessMovement(_guiPve.GetParticipantGui().GetDirectionFromInput());

            var delta = Time.deltaTime;
            if (delta > BLLevelScenePvp.FixedDeltaTime) {
                delta = BLLevelScenePvp.FixedDeltaTime;
            }
            _levelView.Step(delta);

            // update BombLoseControl
            _levelView.ProcessUpdatePlayer();

            DisplayPlayingTime(delta);
            
            var bombable = _levelView.MainPlayer.Bombable; 
            _guiPve.SetQuantityBomb(bombable.CountAvailableBomb());
        }

        private void OnBombButtonClicked() {
            _levelView.OnBombButtonClicked();
        }

        private void OnBoosterButtonClicked(BoosterType type) {
            if (!_levelView.PlayerIsAlive()) {
                return;
            }
            if (_levelView.PlayerIsTakeDamage()) {
                return;
            }
            RequestUseBooster(type);
        }

        public void SetStoryMapDetails(IHeroDetails hero, IStoryMapDetail storyMapDetail,
            IBossSkillDetails bossSkillDetails, StoryModeTicketType ticketType,
            BoosterStatus boosterStatus, bool isTesting) {
            _isTesting = isTesting;
            InitGui();
            _storyModeHero = hero;
            _storyMapDetails = storyMapDetail;
            _bossSkillDetails = bossSkillDetails;
            _ticketType = ticketType;
            _displayPlayingTime = ticketType is StoryModeTicketType.Tournament or StoryModeTicketType.BossHunter;
            _countingPlayingTime = _displayPlayingTime;
            _boosterStatus = boosterStatus;
            var stageLevel = $"STAGE {_storyMapDetails.Stage} - {_storyMapDetails.Level}";
            _guiPve.UpdatePveInfo(_boosterStatus, _displayPlayingTime, stageLevel, _isTesting);
            _guiPve.InitHealthUi(_idxMainPlayer, hero.StoryHp);
            _guiPve.UpdateDamageUi(hero.BombPower);
            
            StartGame();
        }

        public void SetTotalPlayingTime(float totalPlayingTime) {
            _totalPlayingTime = totalPlayingTime;
        }

        private void LoadLevel() {
            var path = $"Prefabs/Levels/levelStoryMode";
            var prefab = Resources.Load<LevelViewStoryMode>(path);
            _levelView = Instantiate(prefab, transform);

            var storyModeCallback = new LevelCallback() {
                OnAddEnemy = OnAddEnemy,
                OnRemoveEnemy = OnRemoveEnemy,
                OnEnterDoor = OnEnterDoor,
                OnPlayerInJail = OnPlayerInJail,
                OnPlayerEndInJail = OnPlayerEndInJail,
                OnUpdateItem = OnUpdateItem,
                OnUpdateHealthUi = OnUpdateHealthUi,
                RequestTakeItem = RequestTakeItem,
                OnLevelCompleted = OnLevelCompleted,
                OnEnemiesCleared = OnEnemiesCleared,
                OnEnemiesSpawned = OnEnemiesSpawned,
                EarnGoldFromEnemy = EarnGoldFromEnemy,
                OnSleepBossCountDown = OnSleepBossCountDown,
                OnBossWakeup = OnBossWakeup,
                OnPlayerEndTesting = OnPlayerEndTesting,
            };

            //TODO:
            var offset = 0; //GuiPlayer.SetSafeArea();
            _levelView.Initialize(_storyModeHero, _storyMapDetails, _bossSkillDetails, _analytics,
                offset,
                storyModeCallback,
                _isTesting);
        }

        #region EVENTS

        private void RequestUseBooster(BoosterType boosterType) {
            // Tạm thời active booster trước khi nhận response.Code từ Server.
            // Nếu response.Code != 0 thì phía server xử lý.
            ActiveBooster(boosterType);

            UniTask.Void(async () => {
                var boosterId = DefaultBoosterManager.ConvertFromEnum(boosterType);
                await _storyModeManager.UseAdventureBooster(boosterId);
            });
        }

        private void ActiveBooster(BoosterType boosterType) {
            _levelView.UseBooster(boosterType);
            _guiPve.UpdateButtonBoosterUsed(boosterType, _boosterStatus, () => _levelView.PlayerIsInJail());

            var lv = _storyMapDetails.Level;
            var stage = _storyMapDetails.Stage;
            _analytics.Adventure_TrackUseBooster(lv, stage, boosterType.ToString());
        }

        private void OnAddEnemy(EnemyType enemyType) {
            _guiPve.AddEnemy(enemyType);
        }

        private void OnRemoveEnemy(EnemyType enemyType) {
            _guiPve.RemoveEnemy(enemyType);
        }

        private void OnEnterDoor() {
            Pause = true;
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var response = await _storyModeManager.EnterDoor();
                    _levelView.ExitPlayer();

                    // Delay 2 giây waiting.. khi server trả kết quả nhanh quá thì hiện ngay bản kết quả thì không đẹp mắt. 
                    await WebGLTaskDelay.Instance.Delay(2000);
                    OnLevelCompleted(true, response);
                } catch (Exception e) {
                    await _guiPve.ShowDialogError(canvasDialog, e.Message);
                    GoToLevelMenu();
                }
                waiting.End();
            });
        }

        private void OnPlayerInJail(int slot) {
            _guiPve.UpdateButtonInJail(_boosterStatus);
        }

        private void OnPlayerEndInJail(int slot) {
            _guiPve.UpdateButtonEndInJail(_boosterStatus);
        }

        public void UpdateDamage(int value) {
            _guiPve.UpdateDamageUi(value);
        }

        private void OnUpdateItem(int slot, ItemType item, int value) {
            _guiPve.UpdateItemUi(item, value);
        }

        private void OnUpdateHealthUi(int slot, int value) {
            EventManager.Dispatcher(PlayerEvent.OnDamage);
            _guiPve.UpdateHealthUi(slot, value);
            _levelView.ShakeCamera();
        }

        private void RequestTakeItem(Item item) {
            //Nếu không phải vàng thì take item.. sau đó request không chờ response
            if (item.ItemType != ItemType.GoldX1 &&
                item.ItemType != ItemType.GoldX5) {
                TakeItemThenRequest(item);
                return;
            }
            
            // còn lại thì request và chờ response để take item.
            RequestThenTakeItem(item);
        }

        private void RequestThenTakeItem(Item item) {
            if (!GameConstant.AdventureRequestServer) {
                _levelView.DeActiveItem(item);
            }
            UniTask.Void(async () => {
                try {
                    var response = await _storyModeManager.RequestTakeItem(item.Location);
                    _levelView.TakeItem(response.Item);
                    var itemType = (ItemType) response.Item.Type;
                    FlyingItemReward(itemType);
                } catch (Exception e) {
                    _logManager.Log(e.Message);
                }
            });
        }
        
        private void TakeItemThenRequest(Item item) {
            _levelView.TakeItem(item.Location.x, item.Location.y, item.ItemType, 0);
            FlyingItemReward(item.ItemType);
            UniTask.Void(async () => {
                try {
                    await _storyModeManager.RequestTakeItem(item.Location);
                } catch (Exception e) {
                    _logManager.Log(e.Message);
                }
            });
        }
        
        private void FlyingItemReward(ItemType itemType) {
            var heroItem = itemType switch {
                ItemType.GoldX1 => HeroItem.Gold,
                ItemType.GoldX5 => HeroItem.Gold,
                ItemType.BronzeChest => HeroItem.BronzeChest,
                ItemType.SilverChest => HeroItem.SilverChest,
                ItemType.GoldChest => HeroItem.GoldChest,
                ItemType.PlatinumChest => HeroItem.PlatinumChest,
                _ => HeroItem.Boots // tạm lấy 1 loại item không có fying effect.
            };
            _guiPve.FlyItemReward(parent, _levelView.GetPlayerPosition(), heroItem);
        }

        private void OnLevelCompleted(bool win, IStoryModeEnterDoorResponse response) {
            Pause = true;
            HideAllDialog();
            _enterDoorResponse = response;
            TrackPlay(win);
            if (win) {
                ShowInterstitialsBeforeWin(response.RewardId, response.WinRewards);
            } else {
                if (_isTesting) {
                    OnRevive(_storyModeHero.StoryHp);
                } else {
                    ShowInterstitialsBeforeLose();
                }
            }
        }

        private void ShowInterstitials(Action callback) {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                await AdsHelper.ShowInterstitial(_unityAdsManager, _serverManager, _analytics, AdsCategory.QuitMatchPve,
                    InterstitialCategory.PveEnd);
                waiting.End();
                callback?.Invoke();
            });
        }

        private void ShowInterstitialsBeforeWin(string rewardId, IWinReward[] rewards) {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                await AdsHelper.ShowInterstitial(_unityAdsManager, _serverManager, _analytics,
                    AdsCategory.CompleteMatchPve, InterstitialCategory.PveEnd);
                waiting.End();
                ShowDialogWin(rewardId, rewards);
            });
        }

        private void ShowInterstitialsBeforeLose() {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                await AdsHelper.ShowInterstitial(_unityAdsManager, _serverManager, _analytics,
                    AdsCategory.CompleteMatchPve, InterstitialCategory.PveEnd);
                waiting.End();
                ShowDialogLose();
            });
        }

        private void ShowDialogWin(string rewardId, IWinReward[] rewards) {
            _soundManager.PlaySound(Audio.PopupWin);
            TrackStoryModeWin();
            TrackFullInventory(rewards);
            _guiPve.ShowDialogPveWin(canvasDialog, _storyMapDetails.Stage, _storyMapDetails.Level, rewardId, rewards,
                () => {
#if UNITY_WEBGL
                    GoToLevelMenu();
#else
                    if (_storyMapDetails.Stage == 1 && _storyMapDetails.Level == 5 &&
                        PlayerPrefs.GetInt(DialogRating.Status, DialogRating.Default) != DialogRating.Showed) {
                        PlayerPrefs.SetInt(DialogRating.Status, DialogRating.Showed);
                        _analytics.TrackScene(SceneType.Rating);
                        _guiPve.ShowDialogRate(canvasDialog, GoToLevelMenu);
                    } else {
                        GoToLevelMenu();
                    }
#endif
                });
        }

        private void ShowDialogLose() {
            _soundManager.PlaySound(Audio.PopupDefeated);
            var callback = new StoryLoseCallback() { OnNextClicked = GoToLevelMenu, OnRevive = OnRevive };
            var takeDamageInfo = _levelView.GetHeroTakeDamageInfo();
            _guiPve.ShowDialogPveLose(canvasDialog, _storyMapDetails.Stage, _storyMapDetails.Level, takeDamageInfo,
                callback);
        }

        private void OnRevive(int hp) {
            _levelView.RevivePlayer(_isTesting);
            _guiPve.ResetBoosterButton(_boosterStatus);
            _guiPve.UpdateHealthUi(_idxMainPlayer, hp);
            _levelView.ReplayMusic();
            Pause = false;
        }

        private void TrackFullInventory(IWinReward[] rewards) {
            var data = new Dictionary<TrackPvpRejectChest, int>() {
                [TrackPvpRejectChest.BronzeChest] = 0, [TrackPvpRejectChest.SilverChest] = 0
            };
            var numFull = 0;
            foreach (var iter in rewards) {
                var rewardType = RewardResource.ConvertStringToEnum(iter.RewardName);
                switch (rewardType) {
                    case RewardSourceType.BronzeChest:
                        if (iter.OutOfSlot) {
                            numFull += 1;
                            data[TrackPvpRejectChest.BronzeChest] = iter.Value;
                        }
                        break;
                    case RewardSourceType.SilverChest:
                        if (iter.OutOfSlot) {
                            numFull += 1;
                            data[TrackPvpRejectChest.SilverChest] = iter.Value;
                        }
                        break;
                }
            }
            if (numFull > 0) {
                _analytics.Pve_TrackFullInventory(data);
            }
        }

        private void HideAllDialog() {
            _guiPve.HideAllDialog(canvasDialog);
        }

        private void TrackUseBoosterAtLevelStart() {
            var trackTheseBoosters = new[] {
                BoosterType.RangeAddOne, BoosterType.BombAddOne, BoosterType.SpeedAddOne,
            };
            foreach (var b in trackTheseBoosters) {
                if (_boosterStatus.IsChooseBooster(b)) {
                    var lv = _storyMapDetails.Level;
                    var stage = _storyMapDetails.Stage;
                    _analytics.Adventure_TrackUseBooster(lv, stage, b.ToString());
                }
            }
            _analytics.TrackConversion(ConversionType.PlayAdventure);
        }

        private void TrackPlay(bool win) {
            var trackResult = win ? TrackPvpMatchResult.Win : TrackPvpMatchResult.Lose;
            _analytics.Adventure_TrackPlay(_storyModeHero.Id, _storyMapDetails.Level, _storyMapDetails.Stage,
                _levelPlayingTime, trackResult);
        }

        private void TrackStoryModeWin() {
            var collected = _storyModeManager.StorageData;
            var data = new Dictionary<TrackPvpCollectItemType, int>() {
                [TrackPvpCollectItemType.SkullHead] = collected.ItemsTake[(int) ItemType.SkullHead],
                [TrackPvpCollectItemType.Armor] = collected.ItemsTake[(int) ItemType.Armor],
                [TrackPvpCollectItemType.Boots] = collected.ItemsTake[(int) ItemType.Boots],
                [TrackPvpCollectItemType.Gold1] = collected.ItemsTake[(int) ItemType.GoldX1],
                [TrackPvpCollectItemType.Gold5] = collected.ItemsTake[(int) ItemType.GoldX5],
                [TrackPvpCollectItemType.BombUp] = collected.ItemsTake[(int) ItemType.BombUp],
                [TrackPvpCollectItemType.BronzeChest] = collected.ItemsTake[(int) ItemType.BronzeChest],
                [TrackPvpCollectItemType.FireUp] = collected.ItemsTake[(int) ItemType.FireUp],
                [TrackPvpCollectItemType.SilverChest] = collected.ItemsTake[(int) ItemType.SilverChest],
                [TrackPvpCollectItemType.SkullHead] = collected.ItemsTake[(int) ItemType.SkullHead],
            };
            var lv = _storyMapDetails.Level;
            var stage = _storyMapDetails.Stage;
            _analytics.Adventure_TrackCollectItems(lv, stage, data, TrackPvpMatchResult.Win);
            _analytics.TrackConversionAdventurePlay(lv, stage);
        }

        private void OnEnemiesCleared() {
            _countingPlayingTime = false;
        }

        private void OnEnemiesSpawned() {
            _countingPlayingTime = true;
        }

        private void EarnGoldFromEnemy(int value, Vector3 enemyPosition) {
            _guiPve.FlyItemReward(parent, enemyPosition, HeroItem.Gold);
        }

        private void OnSleepBossCountDown(int value) {
            _guiPve.UpdateSleepBossTime(value);
        }

        private void OnBossWakeup() {
            _levelView.PlayMusic(Audio.PvpBossMusic);
        }

        private void OnPlayerEndTesting() {
            DialogOK.ShowInfo(canvasDialog, "Exit Preview", ShowDialogQuit);
            Pause = true;
        }

        #endregion

        private void GoToLevelMenu() {
            _storyModeManager.EnterToLevelMenu(_isTesting);
        }

        private void ShowDialogQuit() {
            if (!_levelView.PlayerIsAlive() && !_isTesting) {
                return;
            }
            Pause = true;
            if (_isTesting) {
                var waiting = new WaitingUiManager(canvasDialog);
                waiting.Begin();
                UniTask.Void(async () => {
                    try {
                        await _storyModeManager.GetAdventureLevelDetail();
                        Pause = false;
                        GoToLevelMenu();
                    } catch (Exception e) {
                        DialogOK.ShowError(canvasDialog, e.Message);
                    }
                    waiting.End();
                });
            } else {
                _guiPve.ShowDialogQuit(canvasDialog, RequestQuitPve, () => Pause = false);
            }
        }

        private void RequestQuitPve() {
            ShowInterstitials(GoToLevelMenu);
        }

        private void DisplayPlayingTime(float delta) {
            // if (!_displayPlayingTime || !_countingPlayingTime) {
            //     return;
            // }
            _levelPlayingTime += delta;
            // var currentTimeSpan = TimeSpan.FromSeconds(_levelPlayingTime);
            // var totalTimeSpan = TimeSpan.FromSeconds(_totalPlayingTime + _levelPlayingTime);
            // var (stage, level) = DefaultLevelManager.GetStageLevel(_storyMapDetails.Level - 1);
            // playingTimeLbl.text =
            //     $@"Lv {level + 1} Time: {currentTimeSpan:mm\:ss}{Environment.NewLine}Total: {totalTimeSpan:mm\:ss}";
        }
    }
}