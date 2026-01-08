using System;
using System.Threading;

using Analytics;

using App;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Engine.Utils;

using Game.Dialog;

using PvpMode.Dialogs;
using PvpMode.Services;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.MainMenuScene.Scripts {
    public class DialogLeaderboard : Dialog {
        [SerializeField]
        private GameObject itemPrefab;

        [SerializeField]
        private LeaderBoardItem[] rankMedals;

        [SerializeField]
        private Text endSeason;
        
        [SerializeField]
        private Transform itemContain;
        
        [SerializeField]
        private LeaderBoardItem currentUser;
        
        [SerializeField]
        private GameObject waiting;
        
        [SerializeField] 
        private RectTransform waitingRun;

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private Slider slider;
        
        private IServerManager _serverManager;
        private ISoundManager _soundManager;
        private ILanguageManager _languageManager;
        private ILogManager _logManager;
        private IAnalytics _analytics;
        private CancellationTokenSource _cancellationTokenSource;

        public static UniTask<DialogLeaderboard> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogLeaderboard>();
        }
        
        protected override void Awake() {
            base.Awake();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _cancellationTokenSource = new CancellationTokenSource();
            OnWillHide(() => _cancellationTokenSource.Cancel());
       }

        private void Start() {
            ShowWaiting();
            GetLeaderBoardList();
            _analytics.TrackScene(SceneType.VisitRanking);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _cancellationTokenSource.Dispose();
        }

        private void ShowWaiting() {
            waiting.SetActive(true);
            DOTween.defaultTimeScaleIndependent = true;
            waitingRun.DORotate(new Vector3(0, 0, 360), 0.5f).SetRelative(true).SetEase(Ease.Linear).SetLoops(-1);
        }

        private void HideWaiting() {
            DOTween.Kill(waitingRun, true);
            waiting.SetActive(false);
        }

        private void GetLeaderBoardList() {
            UniTask.Void(async (token) => {
                try {
                    var result = await _serverManager.Pvp.GetPvpRanking();
                    SetCurrentInfo(result.CurrentRank);
                    SetRankListInfo(result.RankList);
                    if (result.RemainTime == 0) {
                        SetEndSeason(string.Empty);
                    } else {
                        SetEndSeason(result.RemainTime < 0
                            ? $"Please wait for new season to start: {Epoch.GetTimeStringDayHourMinute(-result.RemainTime)}"
                            : _languageManager.GetValue(LocalizeKey.end_season) + ": " +
                              Epoch.GetTimeStringDayHourMinute(result.RemainTime));
                    }
                } catch (Exception e) {
                    DialogError.ShowError(DialogCanvas, e.Message, OnCloseButtonClicked);
                }
                HideWaiting();
            }, _cancellationTokenSource.Token);
        }

        private void SetEndSeason(int minutes) {
            endSeason.text = _languageManager.GetValue(LocalizeKey.end_season) + ": " +  Epoch.GetTimeStringDayHourMinute(minutes);
        }

        private void SetEndSeason(string text) {
            endSeason.text = text;
        }

        private void SetCurrentInfo(IPvpRankingItemResult item) {
            currentUser.SetCurrentInfo(item);
            currentUser.SetButtonClicked(ShowDialogProfile);
        }

        private void SetRankListInfo(IPvpRankingItemResult[] list) {
            _logManager.Log();
            foreach (var rank in list) {
                if (rank.RankNumber <= 3) {
                    var item = rankMedals[rank.RankNumber - 1];
                    item.gameObject.SetActive(true);
                    item.SetInfo(rank);
                    item.SetButtonClicked(ShowDialogProfile);
                } else {
                    var obj = Instantiate(itemPrefab, itemContain, false);
                    var item = obj.GetComponent<LeaderBoardItem>();
                    item.SetInfo(rank);
                    item.SetButtonClicked(ShowDialogProfile);
                }
            }
        }

        private void ShowDialogProfile(int userId, string userName, IPvpRankingItemResult rankItem) {
            DialogProfile.Create().ContinueWith(dialog => {
                dialog.InitOtherHero(userId, userName, rankItem);
                dialog.Show(DialogCanvas);
            });
        }

        public void OnSliderValueChanged(float _) {
            if (slider.normalizedValue is 0f or 1f) {
                scrollRect.verticalNormalizedPosition = slider.normalizedValue;
                return;
            }
            if (Mathf.Abs(scrollRect.verticalNormalizedPosition - slider.normalizedValue) > 0.01f) {
                scrollRect.verticalNormalizedPosition = slider.normalizedValue;
            }
        }

        public void OnScrollerScroll(float _) {
            if (Mathf.Abs(slider.normalizedValue - scrollRect.verticalNormalizedPosition) > 0.01f) {
                slider.normalizedValue = scrollRect.verticalNormalizedPosition;
            }
        }
        
        public void OnCloseButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            HideWaiting();
            Hide();
        }

        public void OnButtonInfoClicked() {
            DialogLeaderboardInformation.Create().ContinueWith((dialog) => {
                dialog.Show(DialogCanvas);
            });
        }
        
        public void OnButtonInfoDecayClicked() {
            DialogPointDecay.Create().ContinueWith((dialog) => {
                dialog.Show(DialogCanvas);
            });
        }

        protected override void OnYesClick() {
            // Do nothing
        }
    }
}
