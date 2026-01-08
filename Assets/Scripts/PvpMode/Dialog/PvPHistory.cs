using System;
using System.Threading;

using App;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Senspark;

using Game.Dialog;

using PvpMode.Services;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.Dialogs {
    public class PvPHistory : Dialog {
        [SerializeField]
        private Text addressText;

        [SerializeField]
        private MatchItem itemWin;

        [SerializeField]
        private MatchItem itemLose;

        [SerializeField]
        private Transform itemContain;

        [SerializeField]
        private GameObject waiting;

        [SerializeField]
        private RectTransform waitingRun;

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private Slider slider;

        public static PvPHistory Create() {
            var prefab = Resources.Load<PvPHistory>("Prefabs/PvpMode/Dialog/PvPHistory");
            return Instantiate(prefab);
        }

        private IStorageManager _storeManager;
        private IAccountManager _accountManager;
        private IServerManager _serverManager;
        private CancellationTokenSource _cancellationTokenSource;

        protected override void Awake() {
            base.Awake();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _accountManager = ServiceLocator.Instance.Resolve<IAccountManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _cancellationTokenSource = new CancellationTokenSource();
            OnWillHide(() => _cancellationTokenSource.Cancel());
        }

        private void Start() {
            var address = GetUserName();
            addressText.text = $"{address}";
            ShowWaiting();
            GetPvPHistoryList();
        }
        
        private string GetUserName() {
            var userName = _storeManager.NickName;
            if (string.IsNullOrWhiteSpace(userName) || userName == _accountManager.Account) {
                userName = App.Utils.FormatWalletId(_accountManager.Account);
            }
            return userName;
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

        private void GetPvPHistoryList() {
            UniTask.Void(async (token) => {
                try {
                    var result = await _serverManager.Pvp.GetPvpHistory();
                    SetHistoryListInfo(result.HistoryList);
                } catch (Exception e) {
                    DialogError.ShowError(DialogCanvas, e.Message, OnCloseButtonClicked);
                }
                HideWaiting();
            }, _cancellationTokenSource.Token);
        }

        private void SetHistoryListInfo(IPvpHistoryItemResult[] list) {
            foreach (var history in list) {
                var itemPrefab = history.IsWin ? itemWin : itemLose;
                var obj = Instantiate(itemPrefab, itemContain, false);
                obj.SetInfo(history.MatchId, history.OpponentName, history.Opponent, history.Time, history.Date, history.IsWin);
            }
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
            HideWaiting();
            Hide();
        }
    }
}