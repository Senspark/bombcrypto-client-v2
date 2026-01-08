using System;
using System.Linq;
using System.Threading;

using Analytics;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Dialog.BomberLand.BLLuckyWheel;
using Game.Manager;

using Senspark;

using Services;
using Services.IapAds;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;

using AdResult = Services.IapAds.AdResult;

namespace Scenes.StoryModeScene.Scripts {
    public enum TypeBLLuckyWheel {
        AddGem,
        AddBomb,
        Nothing,
        AddSpeed,
        AddGold,
        AddRange,
    };

    public class BLDialogLuckyWheel : Dialog {
        
        public static UniTask<BLDialogLuckyWheel> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogLuckyWheel>();
        }
        
        [SerializeField]
        protected BLFrameLuckyWheel frameLuckyWheel;

        private ILuckyWheelManager _luckyWheelManager;
        private IUnityAdsManager _unityAdsManager;
        private IAnalytics _analytics;
        private CancellationTokenSource _alive;
        private IStoryModeManager _storyModeManager;
        private IInventoryManager _inventoryManager;
        private IProductItemManager _productItemManager;

        private Action _onCompleted;
        
        protected override void Awake() {
            base.Awake();
            _alive = new CancellationTokenSource();
            IgnoreOutsideClick = true;
            _luckyWheelManager = ServiceLocator.Instance.Resolve<ILuckyWheelManager>();
            _unityAdsManager = ServiceLocator.Instance.Resolve<IUnityAdsManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            frameLuckyWheel.gameObject.SetActive(false);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _alive.Cancel();
            _alive.Dispose();
        }

        public void SetData(Canvas canvasDialog,
            GameObject adsButtons,
            string rewardId,
            bool isPvpMode,
            string matchResult,
            Action onCompleted,
            int level = 0,
            int stage = 0
        ) {
            _onCompleted = onCompleted;
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async (token) => {
                var category = isPvpMode ? "lucky_wheel_pvp" : $"lucky_wheel_pve_level_{level}_stage_{stage}";
                var conversionType =
                    isPvpMode ? ConversionType.LuckyWheelPvpWatchAds : ConversionType.LuckyWheelPveWatchAds;

                try {
                    _analytics.TrackAds(category, TrackAdsResult.Start);
                    await _luckyWheelManager.InitializeAsync();
                    var reward = _luckyWheelManager.GetRewards();
                    var adsId = string.Empty;
                    if (Application.isMobilePlatform) {
                        adsId = await _unityAdsManager.ShowRewarded();
                    }
                    var result = isPvpMode
                        ? await _storyModeManager.GetBonusRewardPvpV2(rewardId, adsId)
                        : await _storyModeManager.GetBonusRewardAdventureV2(rewardId, adsId);

                    adsButtons.SetActive(false);
                    frameLuckyWheel.gameObject.SetActive(true);
                    frameLuckyWheel.SetDataSpin(reward, result);
                    frameLuckyWheel.SetOnSpinFinish(() => {
                        Hide();
                        LuckyWheelReward.GetDialogLuckyReward(result.Items.Length).ContinueWith(
                            (dialogReward) => {
                                dialogReward.UpdateUI(result.Items);
                                dialogReward.OnDidHide(_onCompleted);
                                dialogReward.Show(DialogCanvas);
                            }
                        );
                        ClearCacheDataInventory();
                    });

                    _analytics.TrackAds(category, TrackAdsResult.Complete);
                    _analytics.TrackConversion(conversionType);
                    TrackLuckyWheel(isPvpMode, matchResult, level, stage, result);
                } catch (Exception e) {
                    if (e is AdException adException) {
                        var adsResult = adException.Result switch {
                            AdResult.Cancel => TrackAdsResult.Cancel,
                            _ => TrackAdsResult.Error
                        };
                        _analytics.TrackAds(category, adsResult);
                    }
                    Hide();
                    DialogOK.ShowError(canvasDialog, e.Message, _onCompleted);
                } finally {
                    waiting.End();
                }
            }, _alive.Token);
        }

        private void TrackLuckyWheel(bool isPvpMode, string matchResult, int level, int stage,
            IBonusRewardAdventureV2 result) {
            var type = frameLuckyWheel.IsGetMysteryBox ? "mystery_box" : "normal";
            var mode = isPvpMode ? "pvp" : "pve";
            var names = result.Items.Select(e =>
                _productItemManager.GetItem(e.ItemId).Name.ToString()).ToArray();
            var values = result.Items.Select(e => e.Quantity.ToString()).ToArray();
            _analytics.TrackLuckyWheel(type, mode, names, values, matchResult, level, stage);
        }

        private void ClearCacheDataInventory() {
            _inventoryManager.Clear();
        }

        protected override void OnYesClick() {
            // Do nothing
        }

        protected override void OnNoClick() {
            // Do nothing
        }
    }
}