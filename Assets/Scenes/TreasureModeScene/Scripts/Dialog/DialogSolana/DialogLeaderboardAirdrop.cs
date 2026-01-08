using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Analytics;
using App;
using BomberLand.Button;
using Cysharp.Threading.Tasks;
using Engine.Utils;
using Game.Dialog;
using PvpMode.Dialogs;
using PvpMode.Services;
using Senspark;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using TMPro;
using Ton.Leaderboard;
using UnityEngine;
using UnityEngine.UI;

public class DialogLeaderboardAirdrop : Dialog {
    [SerializeField]
    private LeaderBoardItemPortrait itemPrefab;

    [SerializeField]
    private LeaderBoardItemPortrait[] rankMedals;

    [SerializeField]
    private Transform itemContain;

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
    private TextMeshProUGUI seasonText;
    
    [SerializeField]
    private GameObject leaderboardTypeItem;

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

    [SerializeField]
    private GameObject[] dataObjects;

    private const int MAX_RANK_ITEM = 200;

    private IServerManager _serverManager;
    private ISoundManager _soundManager;
    private ILogManager _logManager;
    private IAnalytics _analytics;
    private CancellationTokenSource _cancellationTokenSource;
    private Dictionary<int, LeaderBoardItemPortrait> _objRankList;
    private ICoinLeaderboardConfigResult[] _coinLeaderboardConfigResult;
    private ICoinRankingResult _coinRankingResult;
    private ICoinRankingResult _allSeasonCoinRankingResult;
    private AirdropRankType _currentUserType;
    private AirdropRankType _currentTabType;

    public static UniTask<DialogLeaderboardAirdrop> Create() {
        return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogLeaderboardAirdrop>();
    }

    protected override void Awake() {
        base.Awake();
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
        _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
        _cancellationTokenSource = new CancellationTokenSource();
        _objRankList = new Dictionary<int, LeaderBoardItemPortrait>();
        OnWillHide(() => _cancellationTokenSource.Cancel());
    }

    private void Start() {
        GetLeaderBoardList();
        _analytics.TrackScene(SceneType.VisitRanking);
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        _cancellationTokenSource.Dispose();
    }

    private void EnableDataObjects(bool state) {
        foreach (var item in dataObjects) {
            item.SetActive(state);
        }
    }

    private async void GetLeaderBoardList() {
        var dialog = await DialogWaiting.Create();
        dialog.Show(DialogCanvas);
        EnableDataObjects(false);
        UniTask.Void(async _ => {
            try {
                if (AppConfig.IsSolana() || AppConfig.IsAirDrop() || AppConfig.IsWebAirdrop()) {
                    _coinLeaderboardConfigResult = await _serverManager.UserSolanaManager.GetCoinLeaderboardConfigSol();
                    _coinRankingResult = await _serverManager.UserSolanaManager.GetCoinRankingSol();
                    _allSeasonCoinRankingResult = await _serverManager.UserSolanaManager.GetAllSeasonCoinRankingSol();
                } else {
                    _coinLeaderboardConfigResult = await _serverManager.Pvp.GetCoinLeaderboardConfig();
                    _coinRankingResult = await _serverManager.Pvp.GetCoinRanking();
                    _allSeasonCoinRankingResult = await _serverManager.Pvp.GetAllSeasonCoinRanking();
                }
                _currentUserType = GetRankType(_coinLeaderboardConfigResult, _coinRankingResult.CurrentRank.Point);
                UpdateSeasonText(_coinRankingResult.RemainTime);
                OnChangeTypeClicked(seasonButton);
            } catch (Exception e) {
                await DialogError.ShowError(DialogCanvas, e.Message, OnCloseButtonClicked);
            } finally {
                dialog.Hide();
                EnableDataObjects(true);
            }
        }, _cancellationTokenSource.Token);
    }

    private void UpdateSeasonText(int remainTime) {
        if (seasonText == null) 
            return;

        string remainTimeText;
        if (remainTime < 0) {
            remainTimeText = Epoch.GetTimeStringDayHourMinute(-remainTime);
            seasonText.text = $"START SEASON: {remainTimeText}";
        } else {
            remainTimeText = Epoch.GetTimeStringDayHourMinute(remainTime);
            seasonText.text = $"END SEASON: {remainTimeText}";
        }
    }

    private AirdropRankType GetRankType(ICoinLeaderboardConfigResult[] config, float point) {
        var result = (int)AirdropRankType.Bronze;
        foreach (var type in config) {
            if (point <= type.UpRankPointUser) break;
            result++;
        }
        return (AirdropRankType)result;
    }

    public void OnChangeTypeClicked(XButton button) {
        _soundManager.PlaySound(Audio.Tap);
        button.SetActive(true);
        leaderboardTypeItem.SetActive(button == seasonButton);
        currentRankIcon.gameObject.SetActive(button == seasonButton);
        if (button == seasonButton) {
            _currentTabType = _currentUserType;
            UpdateSeasonInfo();
            minedText.text = "Mined this season";
            lifetimeButton.SetActive(false);
            SetCurrentInfo(_coinRankingResult.CurrentRank);
        } else {
            UpdateLifetimeInfo();
            minedText.text = "Mined lifetime";
            seasonButton.SetActive(false);
            SetCurrentInfo(_allSeasonCoinRankingResult.CurrentRank);
        }
    }

    private void UpdateSeasonInfo() {
        scrollView.content.anchoredPosition = Vector2.zero;
        scrollView.StopMovement();
        titleText.text = $"{_currentTabType} leaderboard";
        leftButton.gameObject.SetActive(_currentTabType != AirdropRankType.Bronze);
        rightButton.gameObject.SetActive(_currentTabType != AirdropRankType.Mega);
        typeIcon.sprite = resource.GetAirdropRankTypeIcon(_currentTabType);
        var rankTypeInfo = new DefaultPvpServerBridge.RankTypeInfo(_coinLeaderboardConfigResult, _currentTabType);
        var minString = _currentTabType == AirdropRankType.Bronze
            ? $"{rankTypeInfo.startPointUser:N0}"
            : $">{rankTypeInfo.startPointUser:N0}";
        var maxString = _currentTabType == AirdropRankType.Mega ? $"" : $" to {rankTypeInfo.endPointUser:N0}";
        descText.text = $"from {minString}{maxString} Star Core";
        var sortedRankList = UpdateRankListByType();
        SetRankListSeasonInfo(sortedRankList);
    }

    private ICoinRankingItemResult[] UpdateRankListByType() {
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
        return result.Take(MAX_RANK_ITEM).ToArray();
    }

    private void UpdateLifetimeInfo() {
        titleText.text = $"lifetime leaderboard";
        SetRankListLifetimeInfo(_allSeasonCoinRankingResult.RankList);
    }

    private void SetCurrentInfo(ICoinRankingItemResult item) {
        currentUser.SetCurrentInfo(
            item.RankNumber,
            item.Name,
            item.Point
        );
        currentRankIcon.sprite = resource.GetAirdropRankTypeIcon(_currentUserType);
    }

    private void SetRankListSeasonInfo(ICoinRankingItemResult[] list) {
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

        for (var i = 0; i < list.Length; i++) {
            if (i < 3) {
                var item = rankMedals[i];
                item.gameObject.SetActive(true);
                item.SetInfo(i + 1, list[i].Name, list[i].Point);
                _objRankList[i + 1] = item;
            } else {
                var item = Instantiate(itemPrefab, itemContain, false);
                item.SetInfo(i + 1, list[i].Name, list[i].Point);
                _objRankList[i + 1] = item;
            }
        }
    }

    private void SetRankListLifetimeInfo(ICoinRankingItemResult[] list) {
        _logManager.Log();
        foreach (var rank in list) {
            if (_objRankList.ContainsKey(rank.RankNumber)) {
                var obj = _objRankList[rank.RankNumber];
                obj.gameObject.SetActive(true);
                obj.SetInfo(rank.RankNumber, rank.Name, rank.Point);
            } else {
                if (rank.RankNumber <= 3) {
                    var item = rankMedals[rank.RankNumber - 1];
                    item.gameObject.SetActive(true);
                    item.SetInfo(rank.RankNumber, rank.Name, rank.Point);
                    _objRankList[rank.RankNumber] = item;
                } else {
                    var obj = Instantiate(itemPrefab, itemContain, false);
                    obj.SetInfo(rank.RankNumber, rank.Name, rank.Point);
                    _objRankList[rank.RankNumber] = obj;
                }
            }
        }

        foreach (var obj in _objRankList) {
            var isHasItem = false;
            foreach (var rank in list) {
                if (obj.Key == rank.RankNumber) {
                    isHasItem = true;
                    break;
                }
            }
            if (!isHasItem) {
                obj.Value.gameObject.SetActive(false);
            }
        }
    }

    public void OnLeftButton() {
        _soundManager.PlaySound(Audio.Tap);
        var index = (int)_currentTabType;
        index--;
        _currentTabType = (AirdropRankType)index;
        UpdateSeasonInfo();
    }

    public void OnRightButton() {
        _soundManager.PlaySound(Audio.Tap);
        var index = (int)_currentTabType;
        index++;
        _currentTabType = (AirdropRankType)index;
        UpdateSeasonInfo();
    }

    public void OnCloseButtonClicked() {
        _soundManager.PlaySound(Audio.Tap);
        Hide();
    }
}