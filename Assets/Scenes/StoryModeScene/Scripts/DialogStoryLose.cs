using System;

using Analytics;

using App;

using BomberLand.Button;

using Cysharp.Threading.Tasks;

using Engine.Manager;

using Game.Dialog;
using Game.Manager;

using Senspark;

using Services.IapAds;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

using AdResult = Services.IapAds.AdResult;

namespace Scenes.StoryModeScene.Scripts
{
    public struct StoryLoseCallback {
        public Action OnNextClicked;
        public Action<int> OnRevive;
    }
    
    public class DialogStoryLose : Dialog {
        [SerializeField]
        private GameObject reviveButtons;

        [SerializeField]
        private Button buttonNext;
        
        [SerializeField]
        private ReviveButtonAds buttonAds;

        [SerializeField]
        private ReviveButtonGems buttonGems;

        [SerializeField]
        private DelayButton delayButton;

        private IAnalytics _analytics;
        private IUnityAdsManager _unityAdsManager;
        private IStoryModeManager _storyModeManager;
        private IChestRewardManager _chestRewardManager;
        private ILanguageManager _languageManager;
        private IInputManager _inputManager;

        private StoryLoseCallback _storyLoseCallback;
        private int _stage;
        private int _level;
        private int _revivePrice;
        private bool _isClickAds, _isClickGems;

        public static UniTask<DialogStoryLose> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogStoryLose>();
        }
        
        protected override void Awake() {
            base.Awake();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _unityAdsManager = ServiceLocator.Instance.Resolve<IUnityAdsManager>();
            _storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
        }

        public void SetInfo(int stage, int level, HeroTakeDamageInfo info, StoryLoseCallback callback) {
            if (!Application.isMobilePlatform) {
                info.IsReviveAds = false;
            }
            _stage = stage;
            _level = level;
            _revivePrice = info.ReviveGemValue;
            _storyLoseCallback = callback;
            if (info.AllowRevive) {
                reviveButtons.SetActive(true);
                buttonNext.gameObject.SetActive(false);
                buttonGems.SetInfo(_revivePrice);
                if (info.IsReviveAds) {
                    ShowAdsButton();
                } else {
                    ShowGemsButton();
                }
            } else {
                reviveButtons.SetActive(false);
                buttonNext.gameObject.SetActive(true);
            }
        }

        private void ShowAdsButton() {
            buttonAds.gameObject.SetActive(true);
            buttonGems.gameObject.SetActive(false);
            delayButton.gameObject.SetActive(false);
        }

        private void ShowGemsButton() {
            buttonAds.gameObject.SetActive(false);
            buttonGems.gameObject.SetActive(true);
            delayButton.gameObject.SetActive(true);

        }
        
        public void OnAdsButtonClicked() {
            buttonAds.StopCountDown();
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            var category = $"revive_pvp_level_{_level}_stage_{_stage}";
            UniTask.Void(async () => {
                try {
                    _analytics.TrackAds(category, TrackAdsResult.Start);
                    var adsToken = await _unityAdsManager.ShowRewarded();
                    var result = await _storyModeManager.ReviveAdventureHeroByAds(adsToken);

                    _analytics.Adventure_TrackSoftCurrencyReviveByAds(_level, _stage, result.ReviveTimes);
                    _analytics.TrackAds(category, TrackAdsResult.Complete);
                    _analytics.TrackConversion(ConversionType.RevivePveByAds);
                    
                    _storyLoseCallback.OnRevive?.Invoke(result.Hp);
                    Hide();
                } catch (Exception e) {
                    if (e is AdException adException) {
                        var adsResult = adException.Result switch {
                            AdResult.Cancel => TrackAdsResult.Cancel,
                            _ => TrackAdsResult.Error
                        };
                        _analytics.TrackAds(category, adsResult);
                    }
                    DialogOK.ShowError(DialogCanvas, e.Message);
                    ShowGemsButton();
                } finally {
                    waiting.End();
                }
            });
        }

        public void OnGemsButtonClicked() {
            _isClickGems = true;
            var gemUnlock = _chestRewardManager.GetChestReward(BlockRewardType.Gem);
            var gemLock = _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
            var bLGem = gemLock + gemUnlock;
            if (bLGem < _revivePrice) {
                DialogOK.ShowInfo(DialogCanvas, _languageManager.GetValue(LocalizeKey.ui_not_enough_gem), () => {
                    _isClickGems = false;
                });
                return;
            }
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var result = await _storyModeManager.ReviveAdventureHeroByGems();
                    var valueLock = 0;
                    var valueUnlock = 0;
                    foreach (var iter in result.GemUsed) {
                        if (iter.Value == 0) {
                            continue;
                        }
                        if (iter.GemType == "GEM_LOCKED") {
                            valueLock = iter.Value;
                        } else {
                            valueUnlock = iter.Value;
                        }
                    }
                    _analytics.Adventure_TrackSoftCurrencyReviveByGem(_level, _stage, valueLock, valueUnlock, result.ReviveTimes);
                    _analytics.TrackConversion(ConversionType.RevivePveByGem);
                    
                    _storyLoseCallback.OnRevive?.Invoke(result.Hp);
                    Hide();
                } catch (Exception e) {
                    DialogOK.ShowError(DialogCanvas, e.Message,() => {
                        _isClickGems = false;
                    });
                } finally {
                    waiting.End();
                }
            });
        }
        
        public void OnNextClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            _storyLoseCallback.OnNextClicked?.Invoke();
            Hide();
        }

        protected override void OnYesClick() {
            if (_inputManager.ReadButton(_inputManager.InputConfig.Enter)) {
                    if(_isClickGems || !buttonGems.gameObject.activeInHierarchy)
                        return;
                    OnGemsButtonClicked();
            }
        }

        protected override void OnNoClick() {
            if (!delayButton.isCountdown) {
                OnNextClicked();
            }
        }
    }
}