using System;
using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Data;
using BLPvpMode.Manager;

using Data;

using UnityEngine;
using UnityEngine.UI;

namespace Stream {
    public class PvpMatchItemListView : MonoBehaviour {
        [SerializeField]
        private PvpMatchItemView _itemPrefab;

        [SerializeField]
        private ScrollRect _scrollRect;

        private ITimeManager _timeManager;
        private List<PvpMatchItemView> _items;
        private IPvpRoomInfo[] _models;
        private Func<IPvpRoomInfo, bool>[] _filters;

        public IPvpRoomInfo[] Models {
            get => _models;
            set {
                _models = value;
                UpdateModels();
            }
        }

        public Func<IPvpRoomInfo, bool>[] Filters {
            get => _filters;
            set {
                _filters = value;
                UpdateModels();
            }
        }

        public Action<IPvpRoomInfo> OnViewMatch { get; set; }
        public Action<IPvpRoomInfo> OnJoinMatch { get; set; }

        private void Awake() {
            _timeManager = new EpochTimeManager();
            _items = new List<PvpMatchItemView>();
        }

        private void UpdateModels() {
            var filters = _filters ?? Array.Empty<Func<IPvpRoomInfo, bool>>();
            var models = _models;
            foreach (var filter in filters) {
                models = models.Where(filter).ToArray();
            }

            for (var i = models.Length; i < _items.Count; ++i) {
                _items[i].gameObject.SetActive(false);
            }
            for (var i = _items.Count; i < models.Length; ++i) {
                var item = Instantiate(_itemPrefab, _scrollRect.content);
                _items.Add(item);
            }
            var now = _timeManager.Timestamp;
            for (var i = 0; i < models.Length; ++i) {
                var item = _items[i];
                var model = models[i];
                item.gameObject.SetActive(true);
                item.DisplayNames = model.MatchInfo.Info.Select(info => info.DisplayName).ToArray();
                item.Ranks = model.MatchInfo.Info.Select(info => info.Rank).ToArray();
                item.MatchStatus = model.MatchData.Status switch {
                    MatchStatus.Ready => "Ready",
                    MatchStatus.Started => "Started",
                    MatchStatus.Finished => "Finished",
                    MatchStatus.MatchStarted => "MatchStarted",
                    MatchStatus.MatchFinished => "MatchFinished",
                    _ => throw new ArgumentOutOfRangeException(),
                };
                item.MatchDuration = (int) (now - model.MatchData.StartTimestamp) / 1000;
                item.ViewerCount = model.MatchData.ObserverCount;
                item.Mode = model.MatchInfo.Mode;
                item.OnViewMatch = () => OnViewMatch?.Invoke(model);
                item.OnJoinMatch = () => OnJoinMatch?.Invoke(model);
            }
        }
    }
}