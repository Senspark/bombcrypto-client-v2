using System;
using System.Collections.Generic;
using System.Linq;

using App;

using Cysharp.Threading.Tasks;

using Data;

using Game.Dialog;

using Newtonsoft.Json;

using PvpSchedule;
using PvpSchedule.Models;

using Senspark;

using Services;

using Share.Scripts.Communicate;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using Tournament;

using UnityEngine;

namespace Scenes.PvpModeScene.Scripts {
    public enum TournamentTab {
        MyMatches,
        MatchSchedule,
        Leaderboard
    }

    public class DialogPvpSchedule : Dialog {
        [SerializeField]
        private TournamentToggle[] toggles;

        [SerializeField]
        private PvpMatchList pvpMatches;

        [SerializeField]
        private TournamentLeaderboard tournamentLeaderboard;

        [SerializeField]
        private GameObject waiting;

        [SerializeField]
        private GameObject buttonSchedule;
        
        private ITRHeroManager _trHeroManager;
        private IStorageManager _storageManager;
        private IApiManager _apiManager;
        private IUserAccountManager _userAccountManager;
        private IMasterUnityCommunication _unityCommunication;
        private int _heroId;
        private IPvpRoomInfo[] _roomList;
        private List<string> _myMatch;
        
        private const float REFRESH_INTERVAL = 30;
        private float _timeProgress;

        public static UniTask<DialogPvpSchedule> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogPvpSchedule>();
        }

        protected override async void Awake() {
            base.Awake();
            _trHeroManager = ServiceLocator.Instance.Resolve<ITRHeroManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _apiManager = ServiceLocator.Instance.Resolve<IApiManager>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            await LoadPvpMatch();
            LoadLeaderboard();
            OnSelect(TournamentTab.MyMatches);
        }

        private void Start() {
            _timeProgress = 0;

            pvpMatches.SetCallback(JoinPvpQueue, ShowPvPMatch);
            foreach (var iter in toggles) {
                iter.SetSelectCallback(OnSelect);
            }
            toggles[0].SetSelected();
            
            // if (AppConfig.IsProduction) {
            //     buttonSchedule.SetActive(false);
            // }
            buttonSchedule.SetActive(false);

        }

        private async void Update() {
            _timeProgress += Time.deltaTime;
            if (_timeProgress >= REFRESH_INTERVAL) {
                _timeProgress = 0;
                await LoadPvpMatch();
            }
        }

        private void OnSelect(TournamentTab tab) {
            switch (tab) {
                case TournamentTab.MyMatches:
                    ShowMyWatches();
                    break;
                case TournamentTab.MatchSchedule:
                    ShowMatchSchedule();
                    break;
                case TournamentTab.Leaderboard:
                    ShowLeaderBoard();
                    break;
            }
        }

        private void ShowMyWatches() {
            tournamentLeaderboard.gameObject.SetActive(false);
            
            pvpMatches.MyMatch = _myMatch;
            pvpMatches.Filters = new Func<IPvpMatchSchedule, bool>[] {
                info => _myMatch.Contains(info.MatchId),
            };
            pvpMatches.gameObject.SetActive(true);
        }

        private void ShowMatchSchedule() {
            tournamentLeaderboard.gameObject.SetActive(false);
            pvpMatches.MyMatch = _myMatch;
            pvpMatches.Filters = new Func<IPvpMatchSchedule, bool>[] { _ => true, };
            pvpMatches.gameObject.SetActive(true);
        }

        private void ShowLeaderBoard() {
            pvpMatches.gameObject.SetActive(false);
            tournamentLeaderboard.gameObject.SetActive(true);
        }
        
        private async UniTask LoadPvpMatch() {
            waiting.SetActive(true);
            var userName = _unityCommunication.JwtSession.Account.userName;
            _myMatch = await _apiManager.GetMyMatches(userName);

                try {
                    // Load Pvp Heroes để lấy heroId cho join
                    var result = await _trHeroManager.GetHeroesAsync("HERO");
                    var pvpHeroes = result as TRHeroData[] ?? result.ToArray();
                    UpdateSelectedHeroKey(pvpHeroes);

                    _heroId = GetLastPlayedHeroId(pvpHeroes);
                    _storageManager.SelectedHeroKey = _heroId;

                    var matchList = await _apiManager.GetPvpMatches();

                    // set End if joined match
                    var joinedList = TournamentUtils.LoadJoinedList();
                    for (var i = matchList.Count - 1; i >= 0; i--) {
                        var match = matchList[i];
                        if (match.Status == MatchStatus.Waiting && joinedList.Contains(match.MatchId)) {
                            match.Status = MatchStatus.Ended;
                        }
                    }
                    var roomListResult = await _apiManager.GetPvpRoomList();
                    _roomList = roomListResult.ToArray();

                    pvpMatches.Models = matchList.OrderBy(x => OrderIndex(x.Status)).ToArray();
                } catch (Exception e) {
                    DialogOK.ShowError(DialogCanvas, e.Message);
                } finally {
                    waiting.SetActive(false);
                }
            
        }

        private void UpdateSelectedHeroKey(TRHeroData[] trHeroes) {
            foreach (var iter in trHeroes) {
                if (iter.IsActive) {
                    _storageManager.SelectedHeroKey = iter.InstanceId;
                    break;
                }
            }
        }

        private int OrderIndex(MatchStatus status) {
            return status switch {
                MatchStatus.Waiting => 0,
                MatchStatus.InProgress => 1,
                MatchStatus.Upcoming => 2,
                MatchStatus.Ended => 3,
                _ => 4
            };
        }

        private void LoadLeaderboard() {
            tournamentLeaderboard.gameObject.SetActive(true);
        }

        private int GetLastPlayedHeroId(TRHeroData[] pvpHeroes) {
            var id = _storageManager.SelectedHeroKey;
            foreach (var iter in pvpHeroes) {
                if (id == iter.InstanceId) {
                    return id;
                }
            }
            return pvpHeroes[0].InstanceId;
        }

        private void JoinPvpQueue(IPvpMatchSchedule model) {
            DialogPvpStream.Create().ContinueWith(dialog => {
                dialog.OnDidShow(() => dialog.JoinPvpQueue(_heroId, model));
                dialog.Show(DialogCanvas);
            });
        }

        private void ShowPvPMatch(IPvpMatchSchedule model) {
            var roomInfo = FindRoomInfo(model.MatchId);
            if (roomInfo == null) {
                DialogOK.ShowError(DialogCanvas, $"Can not found match {model.MatchId}");
                return;
            }
            DialogPvpStream.Create().ContinueWith(dialog => {
                dialog.OnDidShow(() => UniTask.Void(async () => await dialog.JoinMatch(roomInfo)));
                dialog.Show(DialogCanvas);
            });
        }

        private IPvpRoomInfo FindRoomInfo(string matchId) {
            return _roomList.FirstOrDefault(room => room.MatchInfo.Id == matchId);
        }
    }
}