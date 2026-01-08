using System;
using System.Collections.Generic;
using System.Linq;

using App;

using Senspark;

using Game.Dialog.BomberLand.BLGacha;

using PvpMode.Utils;

using PvpSchedule.Models;

using Share.Scripts.Communicate;

using TMPro;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace PvpSchedule {
    public class PvpMatchItem : MonoBehaviour {
        [SerializeField]
        private GameObject[] status;

        [SerializeField]
        private TextMeshProUGUI timeText;

        [SerializeField]
        private Image iconVS;

        [SerializeField]
        private GameObject score;

        [SerializeField]
        private TextMeshProUGUI scoreText;

        [SerializeField]
        private Image[] ranks;

        [SerializeField]
        private TextMeshProUGUI[] playerNames;

        [SerializeField]
        private TextMeshProUGUI followerText;

        [SerializeField]
        private Button buttonJoin;

        [SerializeField]
        private Button buttonSpectate;

        [SerializeField]
        private BLGachaRes resource;

        private bool _isMyMatch;
        private MatchStatus _matchStatus;
        private long _startTime;
        private long _endTime;
        private float _timeProgress;

        private IMasterUnityCommunication _unityCommunication;

        private Action _joinCallback;
        private Action _spectateCallback;

        private void Awake() {
            _unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            foreach (var iter in status) {
                iter.SetActive(false);
            }
        }

        private void Update() {
            _timeProgress += Time.deltaTime;
            if (_timeProgress < 1) {
                return;
            }
            _timeProgress = 0;
            UpdateStatus();
        }

        public void SetInfo(IPvpMatchSchedule matchSchedule,
            Action joinCallback,
            Action spectateCallback,
            List<string> myMatch) {
            _joinCallback = joinCallback;
            _spectateCallback = spectateCallback;

            _matchStatus = matchSchedule.Status;
            _startTime = matchSchedule.FindBeginTimestamp;
            _endTime = matchSchedule.FindEndTimestamp;

            // var userId = _userAccountManager.GetRememberedAccount().id;
            if(myMatch != null)
                _isMyMatch = myMatch.Contains(matchSchedule.MatchId);
            else {
                _isMyMatch = false;
            }

            UpdateStatus();
            UpdatePlayers(matchSchedule.Players);
            followerText.text = $"{matchSchedule.ObserverCount}";
            UpdateButtons();
        }

        public void OnButtonJoinClicked() {
            _joinCallback.Invoke();
        }

        public void OnButtonSpectateClicked() {
            _spectateCallback.Invoke();
        }

        private void UpdateStatus() {
            if (_matchStatus != MatchStatus.Waiting && _matchStatus != MatchStatus.Upcoming) {
                UpdateStatusWithoutTime();
                return;
            }
            var now = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).Ticks / TimeSpan.TicksPerMillisecond;
            var remainTime = _matchStatus == MatchStatus.Waiting ? _endTime - now : _startTime - now;
            if (remainTime <= 0) {
                // // hết thời gian chờ => end
                // if (_matchStatus == MatchStatus.Waiting) {
                //     _matchStatus = MatchStatus.Ended;
                //     UpdateStatusWithoutTime();
                //     return;
                // }
                //
                // // tới thời gian bắt đầu => waiting
                // _matchStatus = MatchStatus.Waiting;
                // remainTime = _endTime - now;
                remainTime = 0;
            }
            UpdateStatusText();
            timeText.text = TimeUtil.ConvertTimeToString(remainTime);
            timeText.gameObject.SetActive(true);
        }

        private void UpdateStatusWithoutTime() {
            UpdateStatusText();
            timeText.gameObject.SetActive(false);
        }

        private void UpdateStatusText() {
            foreach (var iter in status) {
                iter.gameObject.SetActive(false);
            }
            status[GetStatusIndex(_matchStatus)].gameObject.SetActive(true);
        }

        private static int GetStatusIndex(MatchStatus matchStatus) {
            return matchStatus switch {
                MatchStatus.Waiting => 0,
                MatchStatus.InProgress => 1,
                MatchStatus.Upcoming => 2,
                MatchStatus.Ended => 3,
                _ => throw new Exception($"Invalid match status {matchStatus}")
            };
        }

        private async void UpdatePlayers(IPvpPlayerMatch[] players) {
            Assert.IsTrue(players.Length == 2);
            var scores = new int[2];
            for (var i = 0; i < players.Length; i++) {
                var player = players[i];
                ranks[i].sprite = await resource.GetSpriteByPvpRank(player.Rank);
                playerNames[i].text = Ellipsis.EllipsisAddress(player.DisplayName);
                scores[i] = player.Score;
            }

            if (_matchStatus == MatchStatus.Ended) {
                iconVS.gameObject.SetActive(false);
                scoreText.text = $"{scores[0]} - {scores[1]}";
                score.gameObject.SetActive(true);
            } else {
                iconVS.gameObject.SetActive(true);
                score.gameObject.SetActive(false);
            }
        }

        private void UpdateButtons() {
            if (_matchStatus is MatchStatus.Upcoming or MatchStatus.Ended) {
                buttonJoin.gameObject.SetActive(false);
                return;
            }
            buttonJoin.gameObject.SetActive(_matchStatus == MatchStatus.Waiting && _isMyMatch);
            // if (AppConfig.IsProduction) {
            //     buttonSpectate.gameObject.SetActive(false);
            // } else {
            //     buttonSpectate.gameObject.SetActive(_matchStatus == MatchStatus.InProgress && !_isMyMatch);
            // }
            buttonSpectate.gameObject.SetActive(false);
        }
    }
}