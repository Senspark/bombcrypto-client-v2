using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Analytics;

using App;

using BomberLand.DailyGift;

using Cysharp.Threading.Tasks;

using Data;

using DG.Tweening;

using Game.Dialog.BomberLand.BLGacha;
using Game.Manager;

using Notification;

using Reconnect;
using Reconnect.Backend;
using Reconnect.View;

using Scenes.StoryModeScene.Scripts;

using Senspark;

using Services;
using Services.IapAds;
using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.UI;

using AdResult = Services.IapAds.AdResult;

namespace Scenes.DailyGiftScene.Scripts {
    public class DailyGiftScene : MonoBehaviour {
        [Serializable]
        private struct RewardIcon {
            public string name;
            public Sprite sprite;
        }

        [SerializeField]
        protected Canvas canvasDialog;

        [SerializeField]
        private Sprite[] icons;

        [SerializeField]
        private BLDailyGiftItem item;

        [SerializeField]
        private Transform layout;

        [SerializeField]
        private BLGachaRes resource;

        [SerializeField]
        private RewardIcon[] rewardIcons;

        [SerializeField]
        private Image splashFade;
        
        private CancellationTokenSource _alive;
        private IAnalytics _analytics;
        private IDailyRewardManager _dailyRewardManager;
        private IServerManager _serverManager;
        private List<BLDailyGiftItem> _items;
        private IDictionary<string, Sprite> _rewardIcons;
        private ISoundManager _soundManager;
        private IReconnectStrategy _reconnectStrategy;
        private INotificationManager _notificationManager;        

        private void Awake() {
            splashFade.gameObject.SetActive(true);
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _alive = new CancellationTokenSource();
            _dailyRewardManager = ServiceLocator.Instance.Resolve<IDailyRewardManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _notificationManager = ServiceLocator.Instance.Resolve<INotificationManager>();
            _analytics.TrackScene(SceneType.VisitDailyGift);
            var iconMap = icons.ToDictionary(it => it.name);
            _rewardIcons = rewardIcons.ToDictionary(
                it => it.name,
                it => it.sprite
            );
            _items = new List<BLDailyGiftItem>();
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async (token) => {
                try {
                    await _dailyRewardManager.UpdateDataAsync();
                    var i = 0;
                    foreach (var dailyReward in _dailyRewardManager) {
                        var instant = Instantiate(item, layout);
                        instant.Initialize(Claim, GetSpriteSync, iconMap);
                        instant.UpdateData(dailyReward);
                        instant.UpdateUI();
                        i++;
                        instant.SetSttText(i);
                        _items.Add(instant);
                    }
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(canvasDialog, e.Message);    
                    } else {
                        DialogOK.ShowError(canvasDialog, e.Message);
                    }
#if UNITY_EDITOR
                    Debug.LogException(e);
#endif
                } finally {
                    waiting.End();
                    splashFade.DOFade(0.0f, 0.3f).OnComplete(() => { splashFade.gameObject.SetActive(false); });
                }
            }, _alive.Token);
            _reconnectStrategy = new DefaultReconnectStrategy(
                ServiceLocator.Instance.Resolve<ILogManager>(),
                new MainReconnectBackend(),
                LoadSceneReconnectView.ToCurrentScene(canvasDialog)
            );
            _reconnectStrategy.Start();
        }
        
        private Sprite GetSpriteSync(int itemId, string rewardType) {
            if (itemId == 0) {
                return GetRewardSprite(rewardType);
            } 
            // Handle the asynchronous call outside this method
            return GetSprite(itemId, rewardType).GetAwaiter().GetResult();
        }

        private void OnDestroy() {
            _alive.Cancel();
            _alive.Dispose();
            _reconnectStrategy.Dispose();
        }

        private void Claim() {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async (token) => {
                try {
                    _analytics.TrackAds(AdsCategory.ClaimDailyGiftLevel, TrackAdsResult.Start);
                    var items = await _dailyRewardManager.ClaimRewardAsync();
                    await _dailyRewardManager.UpdateDataAsync();
                    await _serverManager.General.GetChestReward();
                    var tuplesItems = items as (int, string, int)[] ?? items.ToArray();
                    var reward = await LuckyWheelReward.GetDialogLuckyReward(tuplesItems.Count());
                    
                    // reward.UpdateUI(tuplesItems.Select(it => {
                    //     var (itemId, rewardType, quantity) = it;
                    //     return (GetSprite(itemId, rewardType), quantity);
                    // }));

                    UniTask.Void(async () => {
                        var updatedItems = await UniTask.WhenAll(tuplesItems.Select(async it => {
                            var (itemId, rewardType, quantity) = it;
                            var sprite = await GetSprite(itemId, rewardType);
                            return (sprite, quantity);
                        }).ToArray());
                        reward.UpdateUI(updatedItems);
                    });
                    
                    reward.Show(canvasDialog);
                    _analytics.TrackAds(AdsCategory.ClaimDailyGiftLevel, TrackAdsResult.Complete);
                    UpdateUI();
                    AddDailyNotification();
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(canvasDialog, e.Message);    
                    } else {
                        DialogOK.ShowError(canvasDialog, e.Message);
                    }
                    if (e is AdException adException) {
                        var adsResult = adException.Result switch {
                            AdResult.Cancel => TrackAdsResult.Cancel,
                            _ => TrackAdsResult.Error
                        };
                        _analytics.TrackAds(AdsCategory.ClaimDailyGiftLevel, adsResult);
                    }
                } finally {
                    waiting.End();
                }
            }, _alive.Token);
        }

        private Sprite GetRewardSprite(string rewardType) {
            return !_rewardIcons.ContainsKey(rewardType) ? null : _rewardIcons[rewardType];
        }

        private async UniTask<Sprite> GetSprite(int itemId, string rewardType) {
            return itemId == 0
                ? GetRewardSprite(rewardType)
                : await resource.GetSpriteByItemId(itemId);
        }

        public void OnButtonBackClicked() {
            _soundManager.PlaySound(Audio.Tap);
            const string sceneName = "MainMenuScene";
            SceneLoader.LoadSceneAsync(sceneName).Forget();
        }

        private void UpdateUI() {
            var i = 0;
            foreach (var dailyReward in _dailyRewardManager) {
                var it = _items[i]; 
                it.UpdateData(dailyReward);
                i++;
                it.SetSttText(i);
            }
        }

        private void AddDailyNotification() {
            foreach (var dailyReward in _dailyRewardManager) {
                if (dailyReward.Status != DailyRewardData.DailyRewardStatus.None) {
                    continue;
                }
                var duration = dailyReward.ClaimTime - DateTime.UtcNow;
                _notificationManager.AddDailyGiftNotification((int)duration.TotalSeconds);
                return;
            }
        }
        
    }
}