using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using App;
using BomberLand.Button;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using PvpMode.Dialogs;
using PvpMode.Services;
using Senspark;
using Share.Scripts.Dialog;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ton.Leaderboard;

public class ClubInfoTab : MonoBehaviour {
    // Main Panel
    [SerializeField]
    private Image avatarTR;
    
    [SerializeField]
    private GameObject premiumFrame;
    
    [SerializeField]
    private Text clubNameText;
    
    [SerializeField]
    private Sprite avatarDefault;
    
    [SerializeField]
    private Image clubRankIcon;
    
    [SerializeField]
    private TextMeshProUGUI clubRankText;
    
    [SerializeField]
    private AirdropRankTypeResource resource;
    
    [SerializeField]
    private TextMeshProUGUI minedClubText;
    
    [SerializeField]
    private TextMeshProUGUI playersClubText;
    
    // Member Panel
    [SerializeField]
    private Text minedText;
    
    [SerializeField]
    private LeaderBoardItemPortrait currentRank;
    
    [SerializeField]
    private LeaderBoardItemPortrait[] rankItems;
    
    [SerializeField]
    private ScrollRect scrollView;
    
    [SerializeField]
    private LeaderBoardItemPortrait notRankItem;
    
    [SerializeField]
    private TextMeshProUGUI promoteText;
    
    // Buttons and other panels
    [SerializeField]
    private XButton seasonButton;
        
    [SerializeField]
    private XButton lifetimeButton;
    
    [SerializeField]
    private GameObject joinClubButton;
    
    [SerializeField]
    private GameObject requestLeaveClubButton;
    
    [SerializeField]
    private GameObject leaveClubPanel;
    
    [SerializeField]
    private GameObject joinClubPanel;
    
    private IServerManager _serverManager;
    private ISoundManager _soundManager;
    private IUserTonManager _userTonManager;
    private ObserverHandle _handle;
    private IClubInfo _clubInfo;
    private Canvas _canvas;
    private Action<long> _clubLeague;
    private Action<long> _clubPromote;
    private List<LeaderBoardItemPortrait> _rankListObjs = new List<LeaderBoardItemPortrait>();
    private DialogClubAirdrop _dialogClub;
    
    private const string TELEGRAM_BOT_TEST = "qwrertyuio_bot";
    private const string TELEGRAM_BOT_PROD = "bombcrypto_io_bot";
    private const int NAME_LENGTH_LIMIT = 16;
    private const int MAX_RANK_ITEM = 200;

    private void Awake() {
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _userTonManager = ServiceLocator.Instance.Resolve<IServerManager>().UserTonManager;
        _handle = new ObserverHandle();
        _handle.AddObserver(_userTonManager, new UserTonObserver() {
            OnJoinClub = OnJoinClubByTelegram,
            OnLeaveClub = UpdateLeaveClubInfo
        });
    }

    public void SetCanvas(Canvas canvas) {
        _canvas = canvas;
    }
    
    public void SetDialogClub(DialogClubAirdrop dialogClub) {
        _dialogClub = dialogClub;
    }

    public void SetAction(Action<long> clubLeague, Action<long> clubPromote) {
        _clubLeague = clubLeague;
        _clubPromote = clubPromote;
    }

    public void SetInfo(IClubInfo clubInfo, ICoinLeaderboardConfigResult[] config = null) {
        EnableLeaveClub(false);
        EnableJoinClub(false);
        
        _clubInfo = clubInfo;
        if (config != null) {
            var rankType = GetClubRankType(config, _clubInfo.PointTotal);
            clubRankIcon.sprite = resource.GetAirdropRankTypeIcon(rankType);
            clubRankText.text = $"{rankType}";
        }
        UpdateClubInfoPanel();
        SetCurrentClub(_clubInfo.CurrentMember != null);
        OnChangeTypeClicked(seasonButton);
        UpdatePromoteText();
    }

    private void UpdateClubInfoPanel() {
        if (_clubInfo.Avatar != null) {
            var avatarTexture = new Texture2D(1, 1); // Create a temporary texture
            avatarTexture.LoadImage(_clubInfo.Avatar);
            var avatarSprite = Sprite.Create(avatarTexture,
                new Rect(0, 0, avatarTexture.width, avatarTexture.height), new Vector2(0.5f, 0.5f));
            avatarTR.sprite = avatarSprite;
        }
        premiumFrame.SetActive(_clubInfo.IsTopBidClub);
        clubNameText.text = $"{GetShortenName(_clubInfo.Name)}";
        avatarTR.sprite = avatarDefault;
        minedClubText.text = _clubInfo.PointTotal > 0 ? $"{_clubInfo.PointTotal:#,0.####}" : "--";
        playersClubText.text = $"{_clubInfo.Members.Length}";
    }
    
    private string GetShortenName(string name) {
        if (name.Length > NAME_LENGTH_LIMIT) {
            var start = name.Substring(0, 5);
            var end = name.Substring(name.Length - 4);
            var shortenName = $"{start}...{end}";
            return shortenName;
        }
        return name;
    }

    private void UpdateSortType(XButton button) {
        button.SetActive(true);
        scrollView.content.anchoredPosition = Vector2.zero;
        scrollView.StopMovement();
        if (button == seasonButton) {
            minedText.text = "Mined this season";
            lifetimeButton.SetActive(false);
            SetThisSeason();
        } else {
            minedText.text = "Mined lifetime";
            seasonButton.SetActive(false);
            SetLifetime();
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

    private void SetCurrentClub(bool state) {
        joinClubButton.SetActive(!state);
        requestLeaveClubButton.SetActive(state);
    }

    private void SetThisSeason() {
        var members = _clubInfo.Members
            .OrderByDescending(member => member.PointCurrentSeason)
            .Take(MAX_RANK_ITEM)
            .ToList();

        foreach (var item in rankItems) {
            item.gameObject.SetActive(false);
        }
        
        foreach (var obj in _rankListObjs) {
            Destroy(obj.gameObject);
        }
        _rankListObjs.Clear();
        
        if (_clubInfo.CurrentMember == null) {
            currentRank.gameObject.SetActive(false);
        }
        for (var i = 0; i < members.Count; i++) {
            if (i < 3) {
                rankItems[i].gameObject.SetActive(true);
                rankItems[i].SetInfo(i + 1, members[i].Name, (float)members[i].PointCurrentSeason);
            } else {
                var obj = Instantiate(notRankItem, scrollView.content);
                var item = obj.GetComponent<LeaderBoardItemPortrait>();
                item.SetInfo(i + 1, members[i].Name, (float)members[i].PointCurrentSeason);
                _rankListObjs.Add(item);
            }

            if (_clubInfo.CurrentMember != null) {
                if (Math.Abs(_clubInfo.CurrentMember.PointCurrentSeason - members[i].PointCurrentSeason) <= Mathf.Epsilon) {
                    currentRank.gameObject.SetActive(true);
                    currentRank.SetCurrentInfo(i + 1, _clubInfo.CurrentMember.Name, (float)_clubInfo.CurrentMember.PointCurrentSeason);
                }
            }
        }
    }

    private void SetLifetime() {
        var members = _clubInfo.Members
            .OrderByDescending(member => member.PointTotal)
            .Take(MAX_RANK_ITEM)
            .ToList();
        
        foreach (var obj in _rankListObjs) {
            Destroy(obj.gameObject);
        }
        _rankListObjs.Clear();
        
        for (var i = 0; i < members.Count; i++) {
            if (i < 3) {
                rankItems[i].gameObject.SetActive(true);
                rankItems[i].SetInfo(i + 1, members[i].Name, (float)members[i].PointTotal);
            } else {
                var obj = Instantiate(notRankItem, scrollView.content);
                var item = obj.GetComponent<LeaderBoardItemPortrait>();
                item.SetInfo(i + 1, members[i].Name, (float)members[i].PointTotal);
                _rankListObjs.Add(item);
            }

            if (_clubInfo.CurrentMember == null) {
                currentRank.gameObject.SetActive(false);
            } else {
                if (Math.Abs(_clubInfo.CurrentMember.PointTotal - members[i].PointTotal) <= Mathf.Epsilon) {
                    currentRank.gameObject.SetActive(true);
                    currentRank.SetCurrentInfo(i + 1, _clubInfo.CurrentMember.Name, (float)_clubInfo.CurrentMember.PointTotal);
                }
            }
        }
    }

    private void UpdatePromoteText() {
        if (_clubInfo.IsTopBidClub && _clubInfo.CurrentMember != null) {
            promoteText.gameObject.SetActive(true);
            var localNow = DateTime.UtcNow.ToLocalTime();
            var targetTime = localNow.Date.AddDays(1);
            var remainingTime = targetTime - DateTime.UtcNow.ToLocalTime();
            StartCoroutine(CountdownToTargetTime(remainingTime));
        } else {
            promoteText.gameObject.SetActive(false);
            StopAllCoroutines();
        }
    }
    
    private IEnumerator CountdownToTargetTime(TimeSpan remainingTime) {
        while (remainingTime.TotalSeconds >= 0) {
            promoteText.SetText($"Your club is being promoted for  <color=#13EE00>{remainingTime.Hours}h {remainingTime.Minutes}m {remainingTime.Seconds}s</color>");
            yield return new WaitForSeconds(1);
            remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
        }
        promoteText.gameObject.SetActive(false);
    }
    
    public void OnChangeTypeClicked(XButton button) {
        _soundManager.PlaySound(Audio.Tap);
        UpdateSortType(button);
    }

    public void OnClubLeague() {
        _soundManager.PlaySound(Audio.Tap);
        _clubLeague?.Invoke(_clubInfo.ClubId);
    }

    public void OnPromoteClub() {
        _soundManager.PlaySound(Audio.Tap);
        _clubPromote?.Invoke(_clubInfo.ClubId);
    }

    public void OnRequestLeaveClub() {
        _soundManager.PlaySound(Audio.Tap);
        EnableLeaveClub(true);
    }
    
    public void OnRequestJoinClub() {
        if (_dialogClub.GetClubInfo() == null) {
            OnJoinClub();
        } else {
            _soundManager.PlaySound(Audio.Tap);
            EnableJoinClub(true);
        }
    }

    public void OnInviteFriends() {
        _soundManager.PlaySound(Audio.Tap);
        var bot = AppConfig.IsProduction ? TELEGRAM_BOT_PROD : TELEGRAM_BOT_TEST;
        Application.OpenURL(
            $"https://t.me/share/url?url=https://t.me/{bot}?startapp=c-{_clubInfo.ReferralCode}-k-{_clubInfo.ClubId}&text=%F0%9F%94%A5+Join+our+Bombcrypto+Club+and+connect+with+other+players%21+%F0%9F%92%8E+Share+strategies+and+tips+while+growing+your+Bombcrypto+empire.+Don%E2%80%99t+miss+out%E2%80%94tap+the+link+and+join+the+action+now.+%F0%9F%9A%80%F0%9F%92%A3");
    }
    
    private void EnableLeaveClub(bool state) {
        leaveClubPanel.SetActive(state);
    }
    
    private void EnableJoinClub(bool state) {
        joinClubPanel.SetActive(state);
    }

    public void OnNotLeaveClub() {
        _soundManager.PlaySound(Audio.Tap);
        EnableLeaveClub(false);
    }
    
    public void OnNotJoinClub() {
        _soundManager.PlaySound(Audio.Tap);
        EnableJoinClub(false);
    }

    public void OnLeaveClub() {
        _soundManager.PlaySound(Audio.Tap);
        UniTask.Void(async () => {
            var dialog = await DialogWaiting.Create();
            dialog.Show(_canvas);
            try {
                await _serverManager.General.LeaveClub();
                UpdateLeaveClubInfo();
            } catch (Exception ex) {
                DialogError.ShowError(_canvas, ex.Message);
                Debug.LogError(ex);
            } finally {
                dialog.Hide();
            }
        });
    }
    
    public void OnJoinClub() {
        _soundManager.PlaySound(Audio.Tap);
        UniTask.Void(async () => {
            var dialog = await DialogWaiting.Create();
            dialog.Show(_canvas);
            try {
                var newClubInfo = await _serverManager.General.JoinClub(_clubInfo.ClubId);
                _dialogClub.UpdateClubInfo(newClubInfo);
                SetInfo(newClubInfo);
            } catch (Exception ex) {
                DialogError.ShowError(_canvas, ex.Message);
                Debug.LogError(ex);
            } finally {
                dialog.Hide();
            }
        });
    }

    public IMemberClubInfo[] RemoveElementFromList(IMemberClubInfo[] membersList, IMemberClubInfo member) {
        return Array.FindAll(membersList,
            item => !(item.Name == member.Name && Math.Abs(item.PointTotal - member.PointTotal) <= Mathf.Epsilon));
    }

    public void OnJoinClubByTelegram(IClubInfo newClubInfo) {
        if (newClubInfo.ClubId == _clubInfo.ClubId) {
            _dialogClub.UpdateClubInfo(newClubInfo);
            SetInfo(newClubInfo);
        }
    }

    public void UpdateLeaveClubInfo() {
        _dialogClub.UpdateClubInfo(null);
        _clubInfo.Members = RemoveElementFromList(_clubInfo.Members, _clubInfo.CurrentMember);
        _clubInfo.CurrentMember = null;
        SetInfo(_clubInfo);
    }
}
