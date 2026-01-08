using System;
using System.Collections.Generic;
using System.Linq;
using App;
using Cysharp.Threading.Tasks;
using Engine.Utils;
using Game.Dialog;
using PvpMode.Dialogs;
using PvpMode.Services;
using Senspark;

using Share.Scripts.Dialog;

using Ton.Leaderboard;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class PortraitRankingUI : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private LevelScene levelScene;
        
        [SerializeField]
        private GameObject mainContent;

        [SerializeField]
        private GameObject rankContent;
        
        [SerializeField]
        private GameObject joinClubText;
        
        [SerializeField]
        private GameObject clubInfoObject;
        
        [SerializeField]
        private Image clubIcon;
        
        [SerializeField]
        private Text clubName;

        [SerializeField]
        private Text seasonTitle;
        
        [SerializeField]
        private Text remainText;

        [SerializeField]
        private Text rankValue;

        [SerializeField]
        private Image rankIcon;
        
        [SerializeField]
        private AirdropRankTypeResource resource;
        
        private ICoinLeaderboardConfigResult[] _coinLeaderboardConfigResult;
        private ICoinRankingResult _coinRankingResult;
        private IClubInfo _clubInfo;
        private AirdropRankType _currentTabType;
        private DialogLeaderboardAirdrop _dialogLeaderboard;
        private DialogClubAirdrop _dialogClub;
        private IServerManager _serverManager;
        private IUserTonManager _userTonManager;
        private ObserverHandle _handle;

        private void Awake() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _userTonManager = ServiceLocator.Instance.Resolve<IServerManager>().UserTonManager;
            _handle = new ObserverHandle();
            _handle.AddObserver(_userTonManager, new UserTonObserver() {
                OnJoinClub = OnJoinClubByTelegram,
                OnLeaveClub = OnLeaveClubByTelegram
            });
            UniTask.Void(async () => {
                _coinLeaderboardConfigResult = await _serverManager.Pvp.GetCoinLeaderboardConfig();
                try {
                    _coinRankingResult = await _serverManager.Pvp.GetCoinRanking();
                    _currentTabType =
                        GetUserRankType(_coinLeaderboardConfigResult, _coinRankingResult.CurrentRank.Point);
                    rankIcon.sprite = resource.GetAirdropRankTypeIcon(_currentTabType);
                    UpdateRankListByType();
                    var rank = _coinRankingResult.CurrentRank.RankNumber;
                    rankValue.text = rank > 0 ? $"{rank}" : "--";
                } catch (Exception) {
                    rankIcon.sprite = resource.GetAirdropRankTypeIcon(AirdropRankType.Bronze);
                    rankValue.text = "--";
                }
                _clubInfo = await _serverManager.General.GetClubInfo();
                UpdateClubInfo();
            });
        }

        private void UpdateClubInfo() {
            if (_clubInfo == null) {
                joinClubText.gameObject.SetActive(true);
                clubInfoObject.gameObject.SetActive(false);
            } else {
                joinClubText.gameObject.SetActive(false);
                clubInfoObject.gameObject.SetActive(true);
                if (_clubInfo.Name.Length > 10) {
                    clubName.text = $"{_clubInfo.Name.Substring(0, 10)}...";
                } else {
                    clubName.text = _clubInfo.Name;
                }
                
                var clubRank = GetClubRankType(_coinLeaderboardConfigResult, _clubInfo.PointTotal);
                clubIcon.sprite = resource.GetAirdropRankTypeIcon(clubRank);
            }
        }
        
        private AirdropRankType GetClubRankType(ICoinLeaderboardConfigResult[] config, double point) {
            var result = (int)AirdropRankType.Bronze;
            foreach (var type in config) {
                if (point <= type.UpRankPointClub) break;
                result++;
            }
            return (AirdropRankType)result;
        }
        
        private AirdropRankType GetUserRankType(ICoinLeaderboardConfigResult[] config, float point) {
            var result = (int)AirdropRankType.Bronze;
            foreach (var type in config) {
                if (point <= type.UpRankPointUser) break;
                result++;
            }
            return (AirdropRankType)result;
        }

        private void UpdateRankListByType() {
            var rankTypeInfo = new DefaultPvpServerBridge.RankTypeInfo(_coinLeaderboardConfigResult, _currentTabType);
            var result = new List<ICoinRankingItemResult>();
            foreach (var rankItem in _coinRankingResult.RankList) {
                if (_currentTabType == AirdropRankType.Bronze) {
                    if (rankItem.Point >= rankTypeInfo.startPointUser && rankItem.Point <= rankTypeInfo.endPointUser) {
                        result.Add(rankItem);
                    }
                }
                else if (_currentTabType == AirdropRankType.Mega) {
                    if (rankItem.Point > rankTypeInfo.startPointUser) {
                        result.Add(rankItem);
                    }
                } else {
                    if (rankItem.Point > rankTypeInfo.startPointUser && rankItem.Point <= rankTypeInfo.endPointUser) {
                        result.Add(rankItem);
                    }
                }
            }
            result = result.OrderByDescending(r => r.Point).ToList();
            for (var i = 0; i < result.Count; i++) {
                result[i].RankNumber = i + 1;

                if (Math.Abs(_coinRankingResult.CurrentRank.Point - result[i].Point) <= Mathf.Epsilon) {
                    _coinRankingResult.CurrentRank.RankNumber = result[i].RankNumber;
                }
            }
        }

        public async void ShowRanking() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            var dialog = await DialogWaiting.Create();
            dialog.Show(canvasDialog);
            await UniTask.Delay(1000); // Delay 1s
            mainContent.SetActive(false);
            rankContent.SetActive(true);
            _dialogLeaderboard = await DialogLeaderboardAirdrop.Create();
            // _dialogLeaderboard.OnGetRemainTime = (remainTime) => {
            //     if (remainTime < 0) {
            //         SetEndSeason("Season start in", -remainTime);
            //     } else {
            //         SetEndSeason("Season end in", remainTime);
            //     }
            // };
            _dialogLeaderboard.OnWillHide(() => {
                mainContent.SetActive(true);
                rankContent.SetActive(false);
                levelScene.PauseStatus.SetValue(this, false);
            });
            CloseOtherDialog();
            _dialogLeaderboard.Show(canvasDialog);
            levelScene.PauseStatus.SetValue(this, true);
            dialog.Hide();
        }

        private void SetEndSeason(string title, int remainTime) {
            seasonTitle.text = title;
            remainText.text = Epoch.GetTimeStringDayHourMinute(remainTime);
        }
        
        public void OnBackClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            if (_dialogLeaderboard != null) {
                _dialogLeaderboard.Hide();
            }
            if (_dialogClub != null) {
                _clubInfo = _dialogClub.GetClubInfo();
                _dialogClub.Back();
                UpdateClubInfo();
            }
        }
        
        private void CloseOtherDialog() {
            var trans = canvasDialog.transform;
            var count = trans.childCount;
            for (var i = 0; i < count; i++) {
                var other = trans.GetChild(i).GetComponent<Dialog.Dialog>();
                if (other != null) {
                    other.Hide();
                }
            }
        }

        public void OnClubClick() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            levelScene.PauseStatus.SetValue(this, true);
            UniTask.Void(async () => {
                var dialog = await DialogWaiting.Create();
                dialog.Show(canvasDialog);
                await UniTask.Delay(1000); // Delay 1s
                var clubInfo = await _serverManager.General.GetClubInfo();
                mainContent.SetActive(false);
                rankContent.SetActive(true);
                _dialogClub = await DialogClubAirdrop.Create();
                if (clubInfo == null) {
                    _dialogClub.SetClubTab(ClubTab.JoinTab);
                } else {
                    _dialogClub.UpdateClubInfo(clubInfo);
                    _dialogClub.SetClubTab(ClubTab.InfoTab);
                }
                // _dialogClub.OnGetRemainTime = (remainTime) => {
                //     if (remainTime < 0) {
                //         SetEndSeason("Season start in", -remainTime);
                //     } else {
                //         SetEndSeason("Season end in", remainTime);
                //     }
                // };
                _dialogClub.OnWillHide(() => {
                    mainContent.SetActive(true);
                    rankContent.SetActive(false);
                    levelScene.PauseStatus.SetValue(this, false);
                });
                CloseOtherDialog();
                _dialogClub.Show(canvasDialog);
                dialog.Hide();
            });
        }

        private void OnJoinClubByTelegram(IClubInfo newClubInfo) {
            _clubInfo = newClubInfo;
            UpdateClubInfo();
        }
        
        private void OnLeaveClubByTelegram() {
            _clubInfo = null;
            UpdateClubInfo();
        }
    }
}
