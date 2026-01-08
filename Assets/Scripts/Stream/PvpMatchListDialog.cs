using System;
using System.Linq;
using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using Data;

using Senspark;

using UnityEngine;

namespace Stream {
    public class PvpMatchListDialog : MonoBehaviour {
        [SerializeField]
        private PvpMatchItemListView _listView;

        [SerializeField]
        private RectTransform _refreshLayer;

        private IApiManager _apiManager;
        private IUserAccountManager _userAccountManager;
        private bool _refreshing;

        public Action<IPvpRoomInfo> OnViewMatch { get; set; }
        public Action<IPvpRoomInfo> OnJoinMatch { get; set; }

        public static PvpMatchListDialog Show(Canvas canvas) {
            var prefab = Resources.Load<PvpMatchListDialog>($"Prefabs/Stream/{nameof(PvpMatchListDialog)}");
            var dialog = Instantiate(prefab, canvas.transform, false);
            return dialog;
        }

        private void Awake() {
            _apiManager = ServiceLocator.Instance.Resolve<IApiManager>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _refreshLayer.gameObject.SetActive(false);
            _listView.OnViewMatch = info => OnViewMatch?.Invoke(info);
            _listView.OnJoinMatch = info => OnJoinMatch?.Invoke(info);
            Refresh();
        }

        private void Refresh() {
            UniTask.Void(async () => { //
                if (_refreshing) {
                    return;
                }
                _refreshing = true;
                _refreshLayer.gameObject.SetActive(true);
                await RefreshInternal();
                _refreshLayer.gameObject.SetActive(false);
                _refreshing = false;
            });
        }

        private async Task RefreshInternal() {
            var infoList = await _apiManager.GetPvpRoomList();
            _listView.Models = infoList.ToArray();
        }

        public void OnRefreshButtonPressed() {
            Refresh();
        }

        public void OnMyMatchButtonPressed() {
            _listView.Filters = new Func<IPvpRoomInfo, bool>[] {
                info => info.MatchInfo.Info.Any(item =>
                    item.Username == _userAccountManager.GetRememberedAccount().userName)
            };
        }

        public void OnAllMatchButtonPressed() {
            _listView.Filters = new Func<IPvpRoomInfo, bool>[] {
                _ => true,
            };
        }

        public void OnCloseButtonPressed() {
            Destroy(gameObject);
        }
    }
}