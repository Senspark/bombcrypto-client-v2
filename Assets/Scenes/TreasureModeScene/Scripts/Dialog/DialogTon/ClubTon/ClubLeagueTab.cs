using System;
using System.Collections.Generic;
using System.Linq;
using App;
using BomberLand.Button;
using Game.Dialog;
using PvpMode.Dialogs;
using PvpMode.Services;
using Senspark;
using TMPro;
using Ton.Leaderboard;
using UnityEngine;
using UnityEngine.UI;

public class ClubLeagueTab : MonoBehaviour
{
    [SerializeField]
    private LeaderBoardItemPortrait itemPrefab;

    [SerializeField]
    private LeaderBoardItemPortrait[] rankMedals;
    
    [SerializeField]
    private Transform content;
        
    [SerializeField]
    private Text minedText;
    
    [SerializeField]
    private Image currentRankIcon;
    
    [SerializeField]
    private LeaderBoardItemPortrait currentUser;
        
    [SerializeField]
    private AirdropRankTypeResource resource;
        
    [SerializeField]
    private XButton seasonButton;
        
    [SerializeField]
    private XButton lifetimeButton;
        
    [SerializeField]
    private TextMeshProUGUI titleText;
        
    [SerializeField]
    private Button leftButton;
    
    [SerializeField]
    private Button rightButton;
        
    [SerializeField]
    private Image typeIcon;
        
    [SerializeField]
    private Text descText;

    [SerializeField]
    private ScrollRect scrollView;
    
    private ISoundManager _soundManager;
    private AirdropRankType _currentTabType;
    private IClubRank[] _clubRank;
    private ICoinLeaderboardConfigResult[] _config;
    private bool _isSeason;
    private Action<long> _clubInfo;
    private Dictionary<int, LeaderBoardItemPortrait> _objRankList = new Dictionary<int, LeaderBoardItemPortrait>();
    private DialogClubAirdrop _dialogClub;
    
    private const int MAX_RANK_ITEM = 200;
    
    private void Awake() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
    }
    
    public void SetDialogClub(DialogClubAirdrop dialogClub) {
        _dialogClub = dialogClub;
    }

    public void SetAction(Action<long> clubInfo) {
        _clubInfo = clubInfo;
    }
    
    public void SetInfo(IClubRank[] clubRank, ICoinLeaderboardConfigResult[] config) {
        _config = config;
        _clubRank = clubRank;
        OnChangeTypeClicked(seasonButton);
    }
    
    public void OnChangeTypeClicked(XButton button) {
        _soundManager.PlaySound(Audio.Tap);
        _currentTabType = AirdropRankType.Bronze;
        button.SetActive(true);
        _isSeason = button == seasonButton;
        UpdateInfo();
        SetCurrentInfo();
        if (button == seasonButton) {
            minedText.text = "Mined this season";
            lifetimeButton.SetActive(false);
        } else {
            minedText.text = "Mined lifetime";
            seasonButton.SetActive(false);
        }
    }
    
    private void UpdateInfo() {
        scrollView.content.anchoredPosition = Vector2.zero;
        scrollView.StopMovement();
        titleText.text = $"{_currentTabType} league";
        leftButton.gameObject.SetActive(_currentTabType != AirdropRankType.Bronze);
        rightButton.gameObject.SetActive(_currentTabType != AirdropRankType.Mega);
        typeIcon.sprite = resource.GetAirdropRankTypeIcon(_currentTabType);
        var rankTypeInfo = new DefaultPvpServerBridge.RankTypeInfo(_config, _currentTabType);
        var minString = _currentTabType == AirdropRankType.Bronze ? $"{rankTypeInfo.startPointClub:N0}" : $">{rankTypeInfo.startPointClub:N0}";
        var maxString = _currentTabType == AirdropRankType.Mega ? $"" : $" to {rankTypeInfo.endPointClub:N0}";
        descText.text = $"from {minString}{maxString} Star Core";
        SetRankListInfo();
    }
    
    private void SetRankListInfo() {
        foreach (var item in _objRankList.Values) {
            item.gameObject.SetActive(false);
        }
        var keys = _objRankList.Keys.ToList();
        for (var i = 0; i < keys.Count; i++) {
            if (i < 3) {
                _objRankList[keys[i]].gameObject.SetActive(false);
            } else {
                Destroy(_objRankList[keys[i]].gameObject);
                _objRankList.Remove(keys[i]);
            }
        }
        
        var result = UpdateRankListByType(_currentTabType).Take(MAX_RANK_ITEM).ToList();
        for (var i = 0; i < result.Count; i++) {
            if (i < 3) {
                var item = rankMedals[i];
                item.gameObject.SetActive(true);
                var point = _isSeason ? result[i].PointCurrentSeason : result[i].PointTotal;
                item.SetInfo(i + 1, result[i].Name, (float)point); 
                item.SetItemId(result[i].ClubId);
                item.SetItemButton(_clubInfo);
                _objRankList[i + 1] = item;
            } else {
                var item = Instantiate(itemPrefab, content, false);
                var point = _isSeason ? result[i].PointCurrentSeason : result[i].PointTotal;
                item.SetInfo(i + 1, result[i].Name, (float)point);
                item.SetItemId(result[i].ClubId);
                item.SetItemButton(_clubInfo);
                _objRankList[i + 1] = item;
            }
        }
    }

    private void SetCurrentInfo() {
        currentUser.gameObject.SetActive(false);
        var clubInfo = _dialogClub.GetClubInfo();
        if (clubInfo != null) {
            var point = _isSeason? clubInfo.PointCurrentSeason : clubInfo.PointTotal;
            var rankType = GetClubRankType(point);
            var result = UpdateRankListByType(rankType);
            for (var i = 0; i < result.Count; i++) {
                var clubPoint = _isSeason ? clubInfo.PointCurrentSeason : clubInfo.PointTotal;
                var itemPoint = _isSeason ? result[i].PointCurrentSeason : result[i].PointTotal;
                if (Math.Abs(clubPoint - itemPoint) <= Mathf.Epsilon) {
                    currentUser.gameObject.SetActive(true);
                    currentUser.SetCurrentInfo(
                        i + 1,
                        clubInfo.Name,
                        (float)itemPoint
                    );
                    currentRankIcon.sprite = resource.GetAirdropRankTypeIcon(rankType);
                }
            }
        }
    }
    
    private List<IClubRank> UpdateRankListByType(AirdropRankType rankType) {
        var rankTypeInfo = new DefaultPvpServerBridge.RankTypeInfo(_config, rankType);
        var result = new List<IClubRank>();
        foreach (var item in _clubRank) {
            var point = _isSeason ? item.PointCurrentSeason : item.PointTotal;
            if (_currentTabType == AirdropRankType.Bronze) {
                if (point >= rankTypeInfo.startPointClub && point <= rankTypeInfo.endPointClub) {
                    result.Add(item);
                }
            }
            else if (_currentTabType == AirdropRankType.Mega) {
                if (point > rankTypeInfo.startPointClub) {
                    result.Add(item);
                }
            } else {
                if (point > rankTypeInfo.startPointClub && point <= rankTypeInfo.endPointClub) {
                    result.Add(item);
                }
            }
        }
        result = result.OrderByDescending(r => _isSeason ? r.PointCurrentSeason : r.PointTotal).ToList();
        return result;
    }
    
    private AirdropRankType GetClubRankType(double point) {
        var result = (int)AirdropRankType.Bronze;
        foreach (var type in _config) {
            if (point <= type.UpRankPointClub) break;
            result++;
        }
        return (AirdropRankType)result;
    }
    
    public void OnLeftButton() {
        _soundManager.PlaySound(Audio.Tap);
        var index = (int)_currentTabType;
        index--;
        _currentTabType = (AirdropRankType)index;
        UpdateInfo();
    }
    
    public void OnRightButton() {
        _soundManager.PlaySound(Audio.Tap);
        var index = (int)_currentTabType;
        index++;
        _currentTabType = (AirdropRankType)index;
        UpdateInfo();
    }
}
