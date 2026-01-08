using System;
using App;
using Game.Dialog;
using PvpMode.Services;
using Senspark;
using Share.Scripts.Dialog;
using Ton.Leaderboard;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClubJoinTab : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI titleText;
    
    [SerializeField]
    private GameObject infoText;
    
    [SerializeField]
    private GameObject buttons;
    
    [SerializeField]
    private GameObject clubJoinItem;
    
    [SerializeField]
    private Transform content;

    [SerializeField]
    private GameObject joinClubPanel;
    
    [SerializeField]
    private InputField joinClubName;
    
    [SerializeField]
    private GameObject createClubPanel;
    
    [SerializeField]
    private InputField createClubName;
    
    [SerializeField]
    private Button joinClubButton;
    
    [SerializeField]
    private Button createClubButton;

    private IServerManager _serverManager;
    private ISoundManager _soundManager;
    
    private Canvas _canvas;
    private DialogClubAirdrop _dialogClub;

    private void Awake() {
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
    }

    public void SetCanvas(Canvas canvas) {
        _canvas = canvas;
    }
    
    public void SetDialogClub(DialogClubAirdrop dialogClub) {
        _dialogClub = dialogClub;
    }
    
    public void SetJoinTab() {
        UpdateUI("join club", true);
    }

    public void SetPromotedClubsTab() {
        UpdateUI("promoted clubs", false);
    }

    private void UpdateUI(string text, bool state) {
        titleText.SetText(text);
        infoText.SetActive(state);
        buttons.SetActive(state);
    }
    
    public void SetInfo(IClubRank[] clubRanks, ICoinLeaderboardConfigResult[] config, Action<long> onClubInfo) {
        foreach (var rank in clubRanks) {
            var obj = Instantiate(clubJoinItem, content);
            var item = obj.GetComponent<ClubJoinItem>();
            var rankType = GetClubRankType(config, (float)rank.PointTotal);
            item.SetInfo(rank, rankType, (id) => {
                onClubInfo?.Invoke(id);
            });
        }
    }
    
    private AirdropRankType GetClubRankType(ICoinLeaderboardConfigResult[] config, float point) {
        var result = (int)AirdropRankType.Bronze;
        foreach (var type in config) {
            if (point <= type.UpRankPointClub) break;
            result++;
        }
        return (AirdropRankType)result;
    }

    public void EnableJoinClubPanel(bool state) {
        _soundManager.PlaySound(Audio.Tap);
        joinClubPanel.SetActive(state);
        joinClubName.text = string.Empty;
        OnJoinClubInputChange("");
    }

    public async void OnJoinButton() {
        _soundManager.PlaySound(Audio.Tap);
        var dialog = await DialogWaiting.Create();
        dialog.Show(_canvas);
        try {
            var newClubInfo = await _serverManager.General.JoinAnotherClub(joinClubName.text);
            _dialogClub.UpdateClubInfo(newClubInfo);
            _dialogClub.CreateInfoTab(newClubInfo.ClubId);
        } catch (Exception ex) {
            var normalized = ex.Message.Replace("\\n", "\n");
            var parts = normalized.Split('\n');
            var result = "";
            for (var i = 0; i < parts.Length; i++) {
                result += parts[i];
                if (i < parts.Length - 1) {
                    result += "\n";
                }
            }
            DialogOK.ShowError(_canvas, result);
            Debug.LogError(ex);
        } finally {
            dialog.Hide();
        }
    }
    
    public void EnableCreateClubPanel(bool state) {
        _soundManager.PlaySound(Audio.Tap);
        createClubPanel.SetActive(state);
        createClubName.text = string.Empty;
        OnCreateClubInputChange("");
    }
    
    public async void OnCreateButton() {
        _soundManager.PlaySound(Audio.Tap);
        var dialog = await DialogWaiting.Create();
        dialog.Show(_canvas);
        try {
            var newClubInfo = await _serverManager.General.CreateClub(createClubName.text);
            _dialogClub.UpdateClubInfo(newClubInfo);
            _dialogClub.CreateInfoTab(newClubInfo.ClubId);
        } catch (Exception ex) {
            DialogOK.ShowError(_canvas, ex.Message);
            Debug.LogError(ex);
        } finally {
            dialog.Hide();
        }
    }

    public void OnJoinClubInputChange(string input) {
        joinClubButton.interactable = input.Length > 0;
    }
    
    public void OnCreateClubInputChange(string input) {
       createClubButton.interactable = input.Length > 0;
    }
}
