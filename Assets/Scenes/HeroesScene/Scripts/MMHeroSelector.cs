using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using BLPvpMode.Data;
using BLPvpMode.UI;
using BomberLand.Inventory;
using Cysharp.Threading.Tasks;
using Data;

using Engine.Input;
using Engine.MapRenderer;
using Game.Dialog;
using Game.Manager;
using PvpMode.Manager;
using PvpMode.UI;
using Scenes.MainMenuScene.Scripts.Controller;
using Senspark;
using Services.Server.Exceptions;
using Share.Scripts.Dialog;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GroupMainMenu {
    public class MMHeroSelector : MonoBehaviour {
        [SerializeField]
        private Image dimBackground;
        
        [SerializeField]
        private PvpRanking pvpRanking;

        [SerializeField]
        private BLBoosterUI boosterChoose;

        [SerializeField]
        private BLEquipment equipment;

        [SerializeField]
        private BLHeroInfomation hero;

        [SerializeField]
        private GameObject statsContainer;

        [SerializeField]
        private GameObject boosterContainer;

        [SerializeField]
        private BLProfileCard profileCard;

        [SerializeField]
        private RectTransform container;
        
        private HeroSelectorController _controller;

        private HeroId _pvpHeroId = HeroId.NullId;
        private Dictionary<int, StatData[]> _equipSkinStats;

        private System.Action<int> _onHeroSelected;
        private System.Action<int> _onWingEquippedCallback;
        private System.Action<int> _onBombEquippedCallback;
        private System.Action<HeroId, List<HeroGroupData>, Dictionary<int, StatData[]>> _onHeroClickedCallback;
        private System.Action<BoosterType, Action> _onNotificationToMarket;
        private System.Action _onAcceptClickedCallback;

        private Canvas _canvasDialog;
        private ISoundManager _soundManager;
        private IServerManager _serverManager;
        private IStoryModeManager _storyModeManager;
        private IInputManager _inputManager;
        private IDialogManager _dialogManager;
        private bool _isClicked;

        private void Awake() {
            _controller = new HeroSelectorController();
            
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            _dialogManager = ServiceLocator.Instance.Resolve<IDialogManager>();

            //Mobile cần giảm size lại cho phù hợp
            if(container != null && AppConfig.IsMobile()) {
                //Left and right
                container.offsetMin = new Vector2(120, container.offsetMin.y);
                container.offsetMax = new Vector2(-120, container.offsetMax.y);
            }
        }

        private void Update() {
            if(_dialogManager.IsAnyDialogOpened()) {
                return;
            }
            if (_inputManager.ReadJoystick(ControllerButtonName.X)
                || Input.GetKeyDown(KeyCode.Space)) {
                OnAcceptBtn();
            }
        }

        public void Initialized(
            Canvas canvasDialog,
            System.Action<int> onHeroSelected,
            System.Action<int> onWingEquippedCallback,
            System.Action<int> onBombEquippedCallback,
            System.Action<HeroId, List<HeroGroupData>, Dictionary<int, StatData[]>> onHeroClickedCallback,
            System.Action<BoosterType, Action> onNotificationToMarket,
            System.Action onAcceptClickedCallback
        ) {
            UniTask.Void(async () => {
                _canvasDialog = canvasDialog;
                await _controller.Initialized();
                await equipment.InitializeAsync(_canvasDialog, OnReloadEquip, null, OnWingEquipped, OnBombEquipped);
                boosterChoose.Initialize((_, _) => Task.FromResult(0), OnNotificationToMarket);

                _onHeroSelected = onHeroSelected;
                _onWingEquippedCallback = onWingEquippedCallback;
                _onBombEquippedCallback = onBombEquippedCallback;
                _onHeroClickedCallback = onHeroClickedCallback;
                _onNotificationToMarket = onNotificationToMarket;
                _onAcceptClickedCallback = onAcceptClickedCallback;

                UpdateHeroChoose(_controller.GetCurrentPvpHero());
                UpdateRank();
                UpdateBooster();

                hero.SetHeroClickedCallback(OnHeroClicked);
                
                if (profileCard != null) {
                    profileCard.TryLoadData();
                }

                if (AppConfig.IsTournament()) {
                    statsContainer.SetActive(false);
                    boosterContainer.SetActive(false);
                }
                
            });
        }

        public void UpdateHeroChoose(int heroId) {
            var pvpHero = _controller.GetPvpHero(heroId);
            UpdateHeroChoose(pvpHero);
            _onHeroSelected?.Invoke(pvpHero.itemId);
        }

        public BoosterStatus GetBoosterStatus() {
            return boosterChoose.BoosterStatus;
        }
        
        public void UpdateListPvpHeros() {
            UniTask.Void(async () => { await _controller.GetHeroTrFromServer(); });
        }
        
        private void UpdateRank() {
            pvpRanking.SetCurrentInfo(_controller.CurrentRank);
        }

        private void UpdateHeroChoose(PlayerData pvpHero) {
            if (pvpHero == null) {
                _pvpHeroId = HeroId.NullId;
            } else {
                _pvpHeroId = pvpHero.heroId;
            }
            hero.UpdateHero(pvpHero);
            hero.UpdateStatsFromSkinStats(_equipSkinStats);
        }

        private void UpdateBooster() {
            var idBoosters = HeroSelectorController.GetSelectedBooster();
            boosterChoose.SetSelectedBooster(idBoosters);
        }

        private void OnReloadEquip(Dictionary<int, StatData[]> skinStats) {
            _equipSkinStats = skinStats;
            hero.UpdateStatsFromSkinStats(_equipSkinStats);
        }

        private void OnWingEquipped(int wingId) {
            hero.ShowWing(wingId);
            _onWingEquippedCallback?.Invoke(wingId);
        }

        private void OnBombEquipped(int bombId) {
            _onBombEquippedCallback?.Invoke(bombId);
        }

        private void OnHeroClicked() {
            _onHeroClickedCallback?.Invoke(_pvpHeroId, _controller.PvpHeroes, _equipSkinStats);
        }

        private void OnNotificationToMarket(BoosterType boosterType, Action callback) {
            _onNotificationToMarket?.Invoke(boosterType, callback);
        }

        public void OnAcceptBtn() {
            var selectedBoosters = boosterChoose.GetSelectedBoosterIds();
            HeroSelectorController.SaveSelectedBooster(selectedBoosters);
            _onAcceptClickedCallback?.Invoke();
        }
        
        public void OnHide() {
            _onAcceptClickedCallback?.Invoke();
        }

        public void OnChoosePreview() {
            if(_isClicked)
                return;
            _isClicked = true;
            OnPreviewBtn();
        }

        public void OnPreviewBtn() {
            _soundManager.PlaySound(Audio.Tap);
            const int _stage = 1;
            const int _level = 1;

            var waiting = new WaitingUiManager(_canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var isUseMapPveV2 = MapHelperV2.IsUseMapPveV2(_stage, _level);
                    await _storyModeManager.GetAdventureLevelDetail();
                    var storyMapDetail = await _storyModeManager.GetAdventureMapDetail(_level, _stage,
                        isUseMapPveV2 ? 2 : 1, _pvpHeroId, Array.Empty<int>(), true);
                    var equipmentResult = await _serverManager.Pvp.GetEquipment();
                    storyMapDetail.Equipments = equipmentResult.Equipments;
                    var bossSkillDetails =
                        _storyModeManager.StoryModeGetBossSkillDetails(storyMapDetail.Enemies[0].Skin);
                    await _storyModeManager.StartPlaying(storyMapDetail, bossSkillDetails, boosterChoose.BoosterStatus, true);
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        await DialogError.ShowError(_canvasDialog, e.Message, () => { _isClicked = false;});    
                    } else {
                        DialogOK.ShowError(_canvasDialog, e.Message,() => { _isClicked = false;});
                    }
                } finally {
                    waiting.End();
                    _isClicked = false;
                }
            });
        }
    }
}