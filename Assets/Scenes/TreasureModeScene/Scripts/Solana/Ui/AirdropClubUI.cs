using System;
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

public class AirdropClubUI : MonoBehaviour
{
    [SerializeField]
    private Canvas canvasDialog;

    [SerializeField]
    private LevelScene levelScene;
    
    [SerializeField]
    private GameObject joinClubObject;
    
    [SerializeField]
    private GameObject clubInfoObject;
    
    [SerializeField]
    private Text rankValue;
    
    [SerializeField]
    private Image rankIcon;
    
    [SerializeField]
    private Text clubName;
    
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
        _handle = new ObserverHandle();
        _handle.AddObserver(_serverManager, new ServerObserver() {
            OnJoinClub = OnJoinClubByTelegram,
            OnLeaveClub = OnLeaveClubByTelegram
        });
        UniTask.Void(async () => {
            try {
                _clubInfo = await _serverManager.General.GetClubInfo();
            } catch (Exception) {
                _clubInfo = null;
            }
            UpdateClubInfo();
        });
    }

    private void UpdateClubInfo() {
        if (_clubInfo == null) {
            joinClubObject.SetActive(true);
            clubInfoObject.gameObject.SetActive(false);
        } else {
            joinClubObject.SetActive(false);
            clubInfoObject.gameObject.SetActive(true);
            if (_clubInfo.Name.Length > 10) {
                clubName.text = $"{_clubInfo.Name.Substring(0, 10)}...";
            } else {
                clubName.text = _clubInfo.Name;
            }

            UniTask.Void(async () => {
                try {
                    _coinLeaderboardConfigResult ??= await _serverManager.Pvp.GetCoinLeaderboardConfig();
                    _coinRankingResult ??= await _serverManager.Pvp.GetCoinRanking();
                    var clubRank = GetClubRankType(_coinLeaderboardConfigResult, _clubInfo.PointTotal);
                    rankIcon.sprite = resource.GetAirdropRankTypeIcon(clubRank);
                    var rank = _coinRankingResult.CurrentRank.RankNumber;
                    rankValue.text = rank > 0 ? $"{rank}" : "--";
                } catch (Exception) {
                    rankIcon.sprite = resource.GetAirdropRankTypeIcon(AirdropRankType.Bronze);
                    rankValue.text = "--";
                }
            });
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

    public void OnClubClick() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        levelScene.PauseStatus.SetValue(this, true);
        UniTask.Void(async () => {
            var dialog = await DialogWaiting.Create();
            dialog.Show(canvasDialog);
            try {
                await UniTask.Delay(1000); // Delay 1s
                var clubInfo = await _serverManager.General.GetClubInfo();
                _clubInfo = null;
                _dialogClub = await DialogClubAirdrop.Create();
                if (clubInfo == null) {
                    _dialogClub.SetClubTab(ClubTab.JoinTab);
                } else {
                    _dialogClub.UpdateClubInfo(clubInfo);
                    _dialogClub.SetClubTab(ClubTab.InfoTab);
                }
                _dialogClub.OnWillHide(() => { levelScene.PauseStatus.SetValue(this, false); });
                CloseOtherDialog();
                _dialogClub.Show(canvasDialog);
            } catch (Exception) {
                levelScene.PauseStatus.SetValue(this, false);
                DialogOK.ShowError(levelScene.DialogCanvas, "Get Club info failed");
            } finally {
                dialog.Hide();
            }
        });
    }

    private void OnJoinClubByTelegram(IClubInfo newClubInfo) {
        _clubInfo = newClubInfo;
        UpdateClubInfo();
    }
    
    private void OnLeaveClubByTelegram(IClubInfo clubInfo) {
        _clubInfo = null;
        UpdateClubInfo();
    }
}
