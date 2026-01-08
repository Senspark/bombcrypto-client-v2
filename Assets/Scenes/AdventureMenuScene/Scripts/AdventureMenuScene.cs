using System;
using System.Collections.Generic;
using System.Linq;

using Analytics;

using App;

using BLPvpMode.UI;

using BomberLand.Component;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

using Engine.Entities;
using Engine.Input;
using Engine.Manager;

using Game.Dialog;
using Game.Manager;
using Game.UI.Animation;

using Reconnect;
using Reconnect.Backend;
using Reconnect.View;

using Senspark;

using Services;

using Share.Scripts.Dialog;
using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.AdventureMenuScene.Scripts {
    public class AdventureMenuScene : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private BLAdventureMap[] maps;

        [SerializeField]
        private Text stageName;
        
        [SerializeField]
        private Text levelText;

        [SerializeField]
        private Text stageLevelText;
        
        [SerializeField]
        private Button previousButton;

        [SerializeField]
        private Button nextButton;

        [SerializeField]
        private ButtonZoomAndFlash playButton;
        
        private ISoundManager _soundManager;
        private IServerManager _serverManager;
        private ITRHeroManager _trHeroManager;
        private IStoryModeManager _storyModeManager;
        private IAnalytics _analytics;
        private IStorageManager _storageManager;
        private IReconnectStrategy _reconnectStrategy;
        private IInputManager _inputManager;

        private int _trHeroId;
        private int _maxStage;
        private int _maxLevel;

        private int _chooseLevel;
        private int _chooseStage;
        
        private int _centerStage;

        private PlayerData _adventureHero = null;
        private const HeroAccountType DEFAULT_TYPE = HeroAccountType.Tr;

        private Vector2 _firstPressPos;
        private Vector2 _secondPressPos;
        private Vector2 _currentSwipe;
        
        
        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _trHeroManager = ServiceLocator.Instance.Resolve<ITRHeroManager>();
            _storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _analytics.TrackScene(SceneType.VisitAdventure);
            _reconnectStrategy = new DefaultReconnectStrategy(
                ServiceLocator.Instance.Resolve<ILogManager>(),
                new MainReconnectBackend(),
                LoadSceneReconnectView.ToCurrentScene(canvasDialog)
            );
            _reconnectStrategy.Start();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();

        }

        private void OnDestroy() {
            // _soundManager.StopMusic();
            _reconnectStrategy.Dispose();
        }

        private void Update() {
            if (canvasDialog.transform.childCount > 0) {
                return;
            }
#if UNITY_IOS || UNITY_ANDROID            
            TouchSwipe();
#else
            MouseSwipe();
#endif            
            if (Input.GetKeyDown(KeyBoardInputDefine.PlayPve) ||
                _inputManager.ReadButton(_inputManager.InputConfig.Enter)) {
                OnPlayClicked();
            }
            if (_inputManager.ReadButton(_inputManager.InputConfig.Back)) {
                OnBackButtonClicked();
            }
            if (_inputManager.ReadButton(ControllerButtonName.LB) || Input.GetKeyDown(KeyCode.A)) {
                OnPreviousStageClicked();
            }
            if (_inputManager.ReadButton(ControllerButtonName.RB) || Input.GetKeyDown(KeyCode.D)) {
                OnNextStageClicked();
            }
        }

        private void Start() {
            var dialog = new WaitingUiManager(canvasDialog);
            dialog.Begin();
            UniTask.Void(async () => {
                try {
                    await _serverManager.WaitForUserInitialized();
                    var adventureLevelDetail = await _storyModeManager.GetAdventureLevelDetail();
                    _chooseLevel = adventureLevelDetail.CurrentLevel;
                    _maxLevel = adventureLevelDetail.MaxLevel;

                    _centerStage = adventureLevelDetail.CurrentStage;
                    if (_centerStage == 0) {
                        _centerStage = 1;
                    }
                    _maxStage = adventureLevelDetail.MaxStage;
                    if (_maxStage == 0) {
                        _maxStage = 1;
                    }

                    // select level at start is max level + 1;
                    var levelCount = adventureLevelDetail.LevelMaps[_centerStage - 1].LevelCount;
                    if (_maxLevel + 1 > levelCount) {
                        if (_centerStage >= maps.Length) {
                            _chooseLevel = _maxLevel;
                        } else {
                            _maxLevel = 0;
                            _maxStage += 1;
                            _chooseLevel = 1;
                            _centerStage += 1;
                        }
                    } else {
                        _chooseLevel = _maxLevel + 1;
                    }

                    var result = await _trHeroManager.GetHeroesAsync("HERO");
                    var trHeroes = result as TRHeroData[] ?? result.ToArray();
                    UpdateSelectedHeroKey(trHeroes);
                    
                    var id = _storageManager.SelectedHeroKey;
                    if (id > 0) {
                        _adventureHero = GetTrHero(id, trHeroes);
                    } else {
                        _adventureHero = GetTrHero(adventureLevelDetail.HeroId, trHeroes);
                    }
                    
                    // Update Gold
                    await _serverManager.General.GetChestReward();
                    Initialize();
                    dialog.End();
                    
                } catch (Exception e) {
                    DialogOK.ShowError(canvasDialog, e.Message);
                }
            });
        }

        private void UpdateSelectedHeroKey(TRHeroData[] trHeroes) {
            foreach (var iter in trHeroes) {
                if (iter.IsActive) {
                    _storageManager.SelectedHeroKey = iter.InstanceId;
                    break;
                }
            }
        }

        private PlayerData GetTrHero(int heroId, TRHeroData[] trHeroes) {
            if (trHeroes.Length == 0) {
                return null;
            }
            
            foreach (var iter in trHeroes) {
                if (iter.InstanceId == heroId) {
                    return ConvertFrom(iter);
                }
            }
            
            // not found heroId in List => the first one
            var data = trHeroes[0];
            _storageManager.SelectedHeroKey = data.InstanceId;
            return ConvertFrom(data);
        }

        private PlayerData ConvertFrom(TRHeroData data) {
            var player =  new PlayerData() {
                itemId = data.ItemId,
                heroId = new HeroId(data.InstanceId, HeroAccountType.Tr),
                playerType = UIHeroData.ConvertFromHeroId(data.ItemId),
                playercolor = PlayerColor.HeroTr
            };
            var upgradeStats = new Dictionary<StatId, int>();
            upgradeStats[StatId.Range] = data.UpgradedRange;
            upgradeStats[StatId.Speed] = data.UpgradedSpeed;
            upgradeStats[StatId.Count] = data.UpgradedBomb;
            upgradeStats[StatId.Health] = data.UpgradedHp;
            upgradeStats[StatId.Damage] = data.UpgradedDmg;
            player.UpgradeStats = upgradeStats;
            return player;

        }
        
        private void Initialize() {
            _chooseStage = _centerStage;
            foreach (var iter in maps) {
                iter.OnLevelButtonCallback = OnLevelButtonClicked;
                iter.Initialize(_maxStage, _maxLevel, _centerStage, _chooseLevel, _adventureHero);
            }
            ScrollToStage();
            UpdateUILevel();
        }

        private void OnLevelButtonClicked(int stage, int level) {
            _soundManager.PlaySound(Audio.Tap);
            if (_chooseStage == stage && _chooseLevel == level) {
                OnPlayClicked();
                return;
            }
            _chooseStage = stage;
            _chooseLevel = level;
            UpdateUILevel();

            _centerStage = stage;
            ScrollToStage();
        }

        private void UpdateUILevel() {
            levelText.text = $"STAGE {_chooseStage} - LEVEL {_chooseLevel}";
            stageLevelText.text = $"STAGE {_chooseStage}-{_chooseLevel}";
            UpdatePositionHero(_chooseStage, _chooseLevel);
        }

        public void OnPlayClicked() {
            _soundManager.PlaySound(Audio.Tap);
            playButton.SetActive(false);
            DialogAdventureHero.Create().ContinueWith(dialog => {
                dialog.Show(canvasDialog);
                dialog.OnDidHide(() => {
                    playButton.SetActive(true);
                });
                dialog.SetInfo(_chooseLevel, _chooseStage, _adventureHero, OnChangeHero);
            });
        }

        private void OnChangeHero(PlayerData trHero) {
            _adventureHero = trHero;
            foreach (var iter in maps) {
                iter.ChangeHero(trHero);
            }
        }

        private void StageClickedChange(float value) {
            if (value > 0) {
                OnNextStageClicked();
                return;
            }
            if (value < 0) {
                OnPreviousStageClicked();
            }
        }
        
        public void OnPreviousStageClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (previousButton.interactable == false || nextButton.interactable == false) {
                return;
            }
            previousButton.interactable = false;
            nextButton.interactable = false;

            if (_centerStage <= 1) {
                previousButton.interactable = true;
                nextButton.interactable = true;
                return;
            }
            _centerStage -= 1;
            ScrollToStage();
        }

        public void OnNextStageClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (previousButton.interactable == false || nextButton.interactable == false) {
                return;
            }
            previousButton.interactable = false;
            nextButton.interactable = false;

            if (_centerStage >= maps.Length) {
                previousButton.interactable = true;
                nextButton.interactable = true;
                return;
            }
            _centerStage += 1;
            ScrollToStage();
        }

        private void UpdatePositionHero(int stage, int level) {
            foreach (var map in maps) {
                map.SetHeroPosition(stage, level);
            }
        }
        
        private void ScrollToStage() {
            foreach (var map in maps) {
                map.MoveTo(_centerStage, _chooseLevel, 0.5f, OnAfterChangeStage);
            }
        }
        
        private void OnAfterChangeStage() {
            previousButton.interactable = true;
            nextButton.interactable = true;
            stageName.text = GetStageName(_centerStage);
        }

        public void OnBackButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            const string sceneName = "MainMenuScene";
            SceneLoader.LoadSceneAsync(sceneName).Forget();
        }

        private void TouchSwipe() {
            if (Input.touches.Length <= 0) {
                return;
            }
            var t = Input.GetTouch(0);
            switch (t.phase) {
                case TouchPhase.Began:
                    _firstPressPos = new Vector2(t.position.x, t.position.y);
                    break;
                case TouchPhase.Ended: {
                    _secondPressPos = new Vector2(t.position.x, t.position.y);
                    _currentSwipe = new Vector3(_secondPressPos.x - _firstPressPos.x,
                        _secondPressPos.y - _firstPressPos.y);
                    if (_currentSwipe.x < -40) {
                        OnNextStageClicked();
                    }
                    if (_currentSwipe.x > 40) {
                        OnPreviousStageClicked();
                    }
                    break;
                }
            }
        }

        private void MouseSwipe() {
            if (Input.GetMouseButtonDown(0)) {
                _firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            }

            if (!Input.GetMouseButtonUp(0)) {
                return;
            }
            _secondPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            _currentSwipe = new Vector2(_secondPressPos.x - _firstPressPos.x, _secondPressPos.y - _firstPressPos.y);

            if (_currentSwipe.x < -40) {
                OnNextStageClicked();
            }
            if (_currentSwipe.x > 40) {
                OnPreviousStageClicked();
            }
        }
        
        private static string GetStageName(int stage) {
            return stage switch {
                1 => "TOY",
                2 => "CANDY",
                3 => "FOREST",
                4 => "SAND",
                5 => "MACHINE",
                6 => "PIRATES",
                7 => "CHINESE RESTAURANT",
                8 => "FARM",
                9 => "PARK",
                10 => "CEMETERY"
            };
        }
    }
}