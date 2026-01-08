using System;

using TMPro;

using UnityEngine;

namespace Stream {
    public class PvpMatchItemView : MonoBehaviour {
        [SerializeField]
        private TextMeshProUGUI[] _displayNameTexts;

        [SerializeField]
        private TextMeshProUGUI[] _rankTexts;

        [SerializeField]
        private TextMeshProUGUI _statusText;

        [SerializeField]
        private TextMeshProUGUI _durationText;

        [SerializeField]
        private RectTransform _viewButton;

        [SerializeField]
        private TextMeshProUGUI _viewerText;

        [SerializeField]
        private RectTransform _joinButton;

        [SerializeField]
        private TextMeshProUGUI _modeText;

        private string[] _displayNames;
        private int[] _ranks;
        private int _duration;
        private int _viewerCount;
        private BLPvpMode.Engine.Info.PvpMode _mode;

        public string[] DisplayNames {
            get => _displayNames;
            set {
                _displayNames = value;
                UpdateDisplayNames();
            }
        }

        public int[] Ranks {
            get => _ranks;
            set {
                _ranks = value;
                UpdatePoints();
            }
        }

        public string MatchStatus {
            get => _statusText.text;
            set => _statusText.text = value;
        }

        public int MatchDuration {
            get => _duration;
            set {
                _duration = value;
                UpdateDuration();
            }
        }

        public int ViewerCount {
            get => _viewerCount;
            set {
                _viewerCount = value;
                UpdateViewerCount();
            }
        }

        public BLPvpMode.Engine.Info.PvpMode Mode {
            get => _mode;
            set {
                _mode = value;
                UpdateMode();
            }
        }

        public Action OnViewMatch { get; set; }
        public Action OnJoinMatch { get; set; }

        private void Awake() {
            UpdateDuration();
            UpdateViewerCount();
        }

        private void UpdateDisplayNames() {
            for (var i = 0; i < _displayNameTexts.Length; ++i) {
                var text = _displayNameTexts[i];
                var displayName = _displayNames[i];
                text.text = displayName.Length > 16 ? $"{displayName[..8]}...{displayName[^8..]}" : displayName;
            }
        }

        private void UpdatePoints() {
            for (var i = 0; i < _rankTexts.Length; ++i) {
                var text = _rankTexts[i];
                var rank = _ranks[i];
                text.text = $"R{rank}";
            }
        }

        private void UpdateDuration() {
            _durationText.text = $"{_duration / 60}:{_duration % 60}";
        }

        private void UpdateViewerCount() {
            _viewerText.text = $"{_viewerCount} viewers";
        }

        private void UpdateMode() {
            _modeText.text = $"Mode: {Mathf.RoundToInt(Mathf.Log((int) _mode) / Mathf.Log(2))}";
        }

        public void OnViewButtonPressed() {
            OnViewMatch?.Invoke();
        }

        public void OnJoinButtonPressed() {
            OnJoinMatch?.Invoke();
        }
    }
}