using System;
using System.Collections.Generic;
using System.Linq;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Game.UI;
using PvpMode.Services;
using Senspark;
using Share.Scripts.Dialog;
using Ton.Leaderboard;
using UnityEngine;
using UnityEngine.UI;

public class AirdropRankingUi : MonoBehaviour {
    [SerializeField]
    private Canvas canvasDialog;

    [SerializeField]
    private LevelScene levelScene;

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
    private ObserverHandle _handle;

    private void Awake() {
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        GetLeaderBoardData();
    }

    private void GetLeaderBoardData() {
        UniTask.Void(async () => {
            try {
                if (AppConfig.IsSolana()) {
                    _coinLeaderboardConfigResult = await _serverManager.UserSolanaManager.GetCoinLeaderboardConfigSol();
                    _coinRankingResult = await _serverManager.UserSolanaManager.GetCoinRankingSol();
                } else {
                    _coinLeaderboardConfigResult = await _serverManager.Pvp.GetCoinLeaderboardConfig();
                    _coinRankingResult = await _serverManager.Pvp.GetCoinRanking();
                }
                _currentTabType = GetUserRankType(_coinLeaderboardConfigResult, _coinRankingResult.CurrentRank.Point);
                rankIcon.sprite = resource.GetAirdropRankTypeIcon(_currentTabType);
                UpdateRankListByType();
                var rank = _coinRankingResult.CurrentRank.RankNumber;
                rankValue.text = rank > 0 ? $"{rank}" : "--";
            } catch (Exception) {
                rankIcon.sprite = resource.GetAirdropRankTypeIcon(AirdropRankType.Bronze);
                rankValue.text = "--";
            }
        });
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
            } else if (_currentTabType == AirdropRankType.Mega) {
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
        try {
            await UniTask.Delay(1000); // Delay 1s
            _dialogLeaderboard = await DialogLeaderboardAirdrop.Create();
            _dialogLeaderboard.OnWillHide(() => {
                levelScene.PauseStatus.SetValue(this, false);
                GetLeaderBoardData();
            });
            CloseOtherDialog();
            _dialogLeaderboard.Show(canvasDialog);
            levelScene.PauseStatus.SetValue(this, true);
        } catch (Exception) {
            levelScene.PauseStatus.SetValue(this, false);
            DialogOK.ShowError(levelScene.DialogCanvas, "Get Ranking info failed");
        } finally {
            dialog.Hide();
        }
    }

    private void CloseOtherDialog() {
        var trans = canvasDialog.transform;
        var count = trans.childCount;
        for (var i = 0; i < count; i++) {
            var other = trans.GetChild(i).GetComponent<Dialog>();
            if (other != null) {
                other.Hide();
            }
        }
    }
}