using System;
using System.Text;

using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

public class VipItemDisplay : MonoBehaviour {
    [SerializeField]
    private Image icon;

    [SerializeField]
    private Text amount;

    [SerializeField]
    private Button claimBtn;

    [SerializeField]
    private Text claimTxt;

    private Action<VipRewardType> _onClaimBtnClicked;
    private ISoundManager _soundManager;
    private VipRewardType _type;

    private void Awake() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
    }

    public void SetData(Sprite spr, IVipReward data, bool showTotalAmount) {
        _type = data.Type;
        icon.sprite = spr;
        var sb = new StringBuilder();
        sb.Append(data.HavingQuantity);
        if (showTotalAmount) {
            sb.Append($"/{data.Quantity}");
        }
        amount.text = sb.ToString();

        if (claimBtn) {
            var canClaim = data.HavingQuantity > 0;
            claimBtn.interactable = canClaim;
            if (canClaim) {
                var languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
                claimTxt.text = languageManager.GetValue(LocalizeKey.ui_claim);
            } else {
                var timeRemaining = data.NextClaimUtc - DateTime.UtcNow;
                claimTxt.text = FormatTime(timeRemaining);
            }
        }
    }

    public void SetCallback(Action<VipRewardType> onClaimBtnClicked) {
        _onClaimBtnClicked = onClaimBtnClicked;
    }

    public void OnClaimBtnClicked() {
        _soundManager.PlaySound(Audio.Tap);
        _onClaimBtnClicked?.Invoke(_type);
    }

    private static string FormatTime(TimeSpan timeSpan) {
        var sb = new StringBuilder();
        var inserted = 0;
        if (timeSpan.Days != 0) {
            inserted++;
            sb.Append($"{timeSpan.Days}d ");
        }
        if (timeSpan.Hours != 0) {
            inserted++;
            sb.Append($"{timeSpan.Hours}h ");
        }
        if (inserted < 2 && timeSpan.Minutes != 0) {
            inserted++;
            sb.Append($"{timeSpan.Minutes}m ");
        }
        if (inserted < 2 && timeSpan.Seconds != 0) {
            sb.Append($"{timeSpan.Seconds}s");
        }
        return sb.ToString();
    }
}