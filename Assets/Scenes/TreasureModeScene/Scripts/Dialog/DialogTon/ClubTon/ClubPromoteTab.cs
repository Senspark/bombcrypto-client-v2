using System;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Senspark;
using Share.Scripts.Dialog;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClubPromoteTab : MonoBehaviour {
    // Main Panel
    [SerializeField]
    private GameObject promoteClubItem;
    
    [SerializeField]
    private Transform content;
    
    [SerializeField]
    private TextMeshProUGUI boostInfoText;
    
    [SerializeField]
    private Button selectBoostBtn;
    
    [SerializeField]
    private ScrollRect scrollView;
    
    // Promote Info Panel
    [SerializeField]
    private GameObject promoteInfoPanel;
    
    // Promote Confirm Panel
    [SerializeField]
    private TextMeshProUGUI boostPriceText;
    
    [SerializeField]
    private TextMeshProUGUI boostDescText;
    
    [SerializeField]
    private GameObject promoteConfirmPanel;

    private Canvas _canvas;
    private IServerManager _serverManager;
    private ISoundManager _soundManager;
    private IChestRewardManager _chestRewardManager;
    private IClubBidPrice[] _clubBidPrice;
    private long _clubId;
    private int _curIndex;
    private List<ClubPromoteItem> _itemList = new List<ClubPromoteItem>();
    private Action _onBack;
    private Action _clubPromoted;

    private void Awake() {
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        EnablePromoteInfo(false);
        EnablePromoteConfirm(false);
    }
    
    public void SetCanvas(Canvas canvas) {
        _canvas = canvas;
    }
    
    public void SetAction(Action onBack, Action clubPromoted) {
        _onBack = onBack;
        _clubPromoted = clubPromoted;
    }

    public void SetInfo(IClubBidPrice[] clubBidPrice, long clubId) {
        _clubId = clubId;
        _clubBidPrice = clubBidPrice;
        _curIndex = -1;
        selectBoostBtn.interactable = _clubBidPrice.Length > 0;

        if (_itemList.Count > 0) {
            foreach (var item in _itemList) {
                Destroy(item.gameObject);
            }
        }
        _itemList.Clear();
        
        for (var i = 0; i < clubBidPrice.Length; i++) {
            var obj = Instantiate(promoteClubItem, content);
            var promoteItem = obj.GetComponent<ClubPromoteItem>();
            promoteItem.SetInfo(i, _clubBidPrice[i], (index) => {
                if (_curIndex != -1) {
                    _itemList[_curIndex].EnableSelectFrame(false);
                }
                _curIndex = index;
                boostInfoText.SetText($"boost <sprite index=\"0\"> {_clubBidPrice[_curIndex].Price}");
            });
            _itemList.Add(promoteItem);
        }
        if (_clubBidPrice.Length > 0) {
            _itemList[0].EnableSelectFrame(true);
        }
    }
    
    private string GetSuffix(int packageId) {
        if (packageId % 10 == 1) {
            return $"{packageId}st";
        }
        if (packageId % 10 == 2)
        {
            return $"{packageId}nd";
        }
        if (packageId % 10 == 3)
        {
            return $"{packageId}rd";
        }
        return $"{packageId}th";
    }
    
    private void EnablePromoteInfo(bool state) {
        promoteInfoPanel.SetActive(state);
    }
    
    private void EnablePromoteConfirm(bool state) {
        promoteConfirmPanel.SetActive(state);
    }
    
    public void OnEnablePromoteInfo(bool state) {
        _soundManager.PlaySound(Audio.Tap);
        EnablePromoteInfo(state);
    }
    
    public void OnRequestPromote() {
        _soundManager.PlaySound(Audio.Tap);
        boostPriceText.text = $"{_clubBidPrice[_curIndex].Price}";
        boostDescText.text = $"Confirm boost for 24h at {GetSuffix(_clubBidPrice[_curIndex].PackageId)} with";
        EnablePromoteConfirm(true);
    }
    
    public void OnNotConfirmPromote() {
        _soundManager.PlaySound(Audio.Tap);
        EnablePromoteConfirm(false);
    }
    
    public void OnConfirmPromote() {
        _soundManager.PlaySound(Audio.Tap);
        var tonValue = _chestRewardManager.GetChestReward(BlockRewardType.TonDeposited);
        if (tonValue < _clubBidPrice[_curIndex].Price) {
            UniTask.Void(async () => {
                var dialog = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.TonDeposited);
                dialog.Show(_canvas);
            });
        } else {
            UniTask.Void(async () => {
                var dialog = await DialogWaiting.Create();
                dialog.Show(_canvas);
                try {
                    var newClubBidPrice = await _serverManager.General.BoostClub(_clubId, _clubBidPrice[_curIndex].PackageId);
                    DialogOK.ShowInfo(_canvas, "Successfully");
                    _chestRewardManager.AdjustChestReward(BlockRewardType.TonDeposited, -_clubBidPrice[_curIndex].Price);
                    SetInfo(newClubBidPrice, _clubId);
                    scrollView.content.anchoredPosition = Vector2.zero;
                    scrollView.StopMovement();
                } catch (Exception ex) {
                    DialogError.ShowError(_canvas, ex.Message);
                    Debug.LogError(ex);
                } finally {
                    EnablePromoteConfirm(false);
                    dialog.Hide();
                }
            });
        }
    }
    
    public void OnPromotedClubs() {
        _soundManager.PlaySound(Audio.Tap);
        _clubPromoted?.Invoke();
    }

    public void OnCloseButton() {
        _soundManager.PlaySound(Audio.Tap);
        _onBack?.Invoke();
    }
}
