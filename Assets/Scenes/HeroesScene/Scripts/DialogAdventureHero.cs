using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Analytics;

using App;

using BLPvpMode.UI;

using BomberLand.Inventory;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

using Engine.Entities;
using Engine.Input;
using Engine.MapRenderer;

using Game.Dialog;
using Game.Manager;
using Game.UI.Animation;

using PvpMode.Manager;
using PvpMode.UI;

using Scenes.AltarScene.Scripts;
using Scenes.MainMenuScene.Scripts;
using Scenes.MainMenuScene.Scripts.Controller;

using Senspark;

using Services;
using Services.IapAds;
using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

using IBoosterManager = PvpMode.Manager.IBoosterManager;

namespace Scenes.AdventureMenuScene.Scripts {
    public class DialogAdventureHero : Dialog {
        [SerializeField]
        private BLHeroInfomation hero;

        [SerializeField]
        private BLEquipment equipment;

        [SerializeField]
        private BLBoosterUI boosterUi;

        [SerializeField]
        private ButtonZoomAndFlash playButton;

        [SerializeField]
        private Text stageLevelText;

        [SerializeField]
        private PvpRanking pvpRanking;
        
        [SerializeField]
        private BLProfileCard profileCard;
        
        [SerializeField] private RectTransform container;
        
        private ISoundManager _soundManager;
        private IServerManager _serverManager;
        private ITRHeroManager _trHeroManager;
        private IPlayerStorageManager _playerStore;
        private IStoryModeManager _storyModeManager;
        private IBoosterManager _boosterManager;
        private IUnityAdsManager _unityAdsManager;
        private IAnalytics _analytics;
        private Services.IBoosterManager _serviceBoosterManager;
        private IStorageManager _storageManager;
        private IInputManager _inputManager;
        private HeroSelectorController _controller;

        private HeroId _trHeroId = HeroId.NullId;
        private List<HeroGroupData> _trHeroes;
        private int _level;
        private int _stage;

        private Dictionary<int, StatData[]> _equipSkinStats;
        private System.Action<PlayerData> _onChangeHeroCallback;

        public static UniTask<DialogAdventureHero> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogAdventureHero>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _trHeroManager = ServiceLocator.Instance.Resolve<ITRHeroManager>();
            _playerStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
            _boosterManager = ServiceLocator.Instance.Resolve<IBoosterManager>();
            _unityAdsManager = ServiceLocator.Instance.Resolve<IUnityAdsManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _serviceBoosterManager = ServiceLocator.Instance.Resolve<Services.IBoosterManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();

            hero.SetHeroClickedCallback(OnHeroClicked);
            _controller = new HeroSelectorController();
            
            if (profileCard != null) {
                profileCard.TryLoadData();
            }

            // Set container offset for mobile
            if (container != null && AppConfig.IsMobile()) {
                container.offsetMin = new Vector2(80, container.offsetMin.y);
                container.offsetMax = new Vector2(-80, container.offsetMax.y);
            }
        }

        protected override void ExtraCheck() {
            if (Input.GetKeyDown(KeyBoardInputDefine.PlayPve)
                || _inputManager.ReadButton(ControllerButtonName.X)) {
                OnPlayButtonClicked();
            }
        }

        protected override void OnYesClick() {
            // Do nothing
        }
        

        public void SetInfo(int level, int stage, PlayerData trHero, System.Action<PlayerData> callback) {
            _level = level;
            _stage = stage;
            stageLevelText.text = $"STAGE {stage}-{level}";

            _onChangeHeroCallback = callback;
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                // Update Rank
                await _controller.Initialized();
                pvpRanking.SetCurrentInfo(_controller.CurrentRank);
                
                // Get User Booster
                //var boosterResult = await _serviceBoosterManager.GetPvEBoostersAsync();
                var boosterResult = await _serviceBoosterManager.GetPvPBoostersAsync();
                _boosterManager.SetBoosterData(boosterResult);
                _boosterManager.EventBoosterChanged += OnBoosterChanged;
                boosterUi.Initialize(DialogCanvas, (_, _) => Task.FromResult(0));

                // Get Tr Heroes
                var result = await _trHeroManager.GetHeroesAsync("HERO");
                LoadHeroesFrom(result.ToArray());
                UpdateUiWithTrHero(trHero);

                waiting.End();

                await equipment.InitializeAsync(DialogCanvas, OnReloadEquip, OnShowHideEquip, OnWingEquipped);
            });
        }

        private void OnWingEquipped(int itemId) {
            hero.ShowWing(itemId);
        }
        
        private void OnShowHideEquip(bool isShow) {
            playButton.Interactable = !isShow;
        }

        private void OnReloadEquip(Dictionary<int, StatData[]> skinStats) {
            _equipSkinStats = skinStats;
            hero.UpdateStatsFromSkinStats(_equipSkinStats);
            playButton.Interactable = true;
        }

        private void LoadHeroesFrom(TRHeroData[] trHeroes) {
            _trHeroes = new List<HeroGroupData>();
            foreach (var iter in trHeroes) {
                var player = new PlayerData() {
                    itemId = iter.ItemId,
                    heroId = new HeroId(iter.InstanceId, HeroAccountType.Tr),
                    playerType = UIHeroData.ConvertFromHeroId(iter.ItemId),
                    playercolor = PlayerColor.HeroTr
                };
                var inventoryHero = new HeroGroupData() {PlayerData = player, Quantity = iter.Quantity};
                var upgradeStats = new Dictionary<StatId, int>();
                upgradeStats[StatId.Range] = iter.UpgradedRange;
                upgradeStats[StatId.Speed] = iter.UpgradedSpeed;
                upgradeStats[StatId.Count] = iter.UpgradedBomb;
                upgradeStats[StatId.Health] = iter.UpgradedHp;
                upgradeStats[StatId.Damage] = iter.UpgradedDmg;
                inventoryHero.PlayerData.UpgradeStats = upgradeStats;
                _trHeroes.Add(inventoryHero);

                if (iter.IsActive) {
                    _storageManager.SelectedHeroKey = iter.InstanceId;
                }
            }
        }

        private void OnBoosterChanged(IBooster[] boosters) {
            boosterUi.UpdateQuantity();
        }

        public void OnPlayButtonClicked() {
            if (!playButton.Interactable) {
                return;
            }
            playButton.Interactable = false;
            _soundManager.PlaySound(Audio.Tap);
            StartAdventureMode();
        }

        private void OnHeroClicked() {
            DialogHeroSelection.Create().ContinueWith(dialog => {
                dialog.SetInfo(_trHeroId, _trHeroes, _equipSkinStats, OnHeroSelected);
                dialog.Show(DialogCanvas);
            });
        }

        private void OnHeroSelected(HeroId pvpHeroId) {
            var trHero = GetTrHero(pvpHeroId.Id);
            UpdateUiWithTrHero(trHero);
            _onChangeHeroCallback(trHero);
        }

        private PlayerData GetTrHero(int heroId) {
            foreach (var iter in _trHeroes) {
                if (iter.PlayerData.heroId.Id == heroId) {
                    return iter.PlayerData;
                }
            }
            if (_trHeroes.Count > 0) {
                return _trHeroes[0].PlayerData;
            }
            return null;
        }

        private void StartAdventureMode() {
            _soundManager.PlaySound(Audio.Tap);
            _analytics.TrackClickPlayPve(_trHeroId.Id, _level, _stage);

            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var isUseMapPveV2 = MapHelperV2.IsUseMapPveV2(_stage, _level);
                    var storyMapDetail = await _storyModeManager.GetAdventureMapDetail(_level, _stage,
                        isUseMapPveV2 ? 2 : 1, _trHeroId, boosterUi.GetSelectedBoosterIds(), false);
                    var equipmentResult = await _serverManager.Pvp.GetEquipment();
                    storyMapDetail.Equipments = equipmentResult.Equipments;
                    var bossSkillDetails =
                        _storyModeManager.StoryModeGetBossSkillDetails(storyMapDetail.Enemies[0].Skin);
                    await _storyModeManager.StartPlaying(storyMapDetail, bossSkillDetails, boosterUi.BoosterStatus);
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e.Message);    
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
                    }
                    playButton.Interactable = true;
                } finally {
                    waiting.End();
                }
            });
        }

        private void UpdateUiWithTrHero(PlayerData trHero) {
            if (trHero == null) {
                _trHeroId = HeroId.NullId;
            } else {
                _trHeroId = trHero.heroId;
            }
            hero.UpdateHero(trHero);
            hero.UpdateStatsFromSkinStats(_equipSkinStats);
            UpdatePlayButton();
        }

        private void UpdatePlayButton() {
            playButton.Interactable = !_trHeroId.IsInvalid();
        }
    }
}