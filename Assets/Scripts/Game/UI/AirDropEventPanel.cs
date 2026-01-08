using System;

using App;

using Game.Dialog;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.UI {
    public class AirDropEventPanel : MonoBehaviour {
        [SerializeField]
        private Text title;

        [SerializeField]
        private Text eventName;

        [SerializeField]
        private Text rewardAmount;

        [SerializeField]
        private Image icon;
        
        [SerializeField]
        private Text desc;

        [SerializeField]
        private Button claimBtn;
        
        [SerializeField]
        private GameObject disableBtn;

        [SerializeField]
        private Text timeLeftTxt;

        [SerializeField]
        private Text supplyTxt;

        [SerializeField]
        private GameObject openGroup;
        
        [SerializeField]
        private GameObject closedGroup;

        private ISoundManager _soundManager;
        private ILanguageManager _languageManager;
        private IChestRewardManager _chestRewardManager;
        
        private float _claimFee;
        private Canvas _dialogCanvas;
        private Action<IAirDropEvent> _onClaimBtnClicked;
        private Action<IAirDropEvent> _onHelpBtnClicked;
        private IAirDropEvent _event;

        public void Init(
            IAirDropEvent ev,
            Action<IAirDropEvent> onClaimBtnClicked,
            Action<IAirDropEvent> onHelpBtnClicked,
            ISoundManager soundManager,
            ILanguageManager languageManager,
            IChestRewardManager chestRewardManager,
            AirDropEventData data,
            Canvas dialogCanvas) {
            _event = ev;
            _onClaimBtnClicked = onClaimBtnClicked;
            _onHelpBtnClicked = onHelpBtnClicked;
            _soundManager = soundManager;
            _languageManager = languageManager;
            _chestRewardManager = chestRewardManager;
            _dialogCanvas = dialogCanvas;

            var isClosed = ev.Closed;
            openGroup.SetActive(!isClosed);
            closedGroup.SetActive(isClosed);
            
            rewardAmount.text = $"x{ev.RewardAmount}";
            eventName.text = ev.EventName;

            if (!isClosed) {
                var ts = ev.RemainingTime;
                desc.text = $"BUY {ev.BomberBought}/{ev.BomberToBuy}{Environment.NewLine}S Heroes";
                supplyTxt.text = $"Supply: {ev.SupplyClaimed}/{ev.SupplyTotal}";

                if (data != null) {
                    title.text = data.title;
                    icon.sprite = data.icon;
                    _claimFee = data.fee;
                } else {
                    title.text = null;
                }

                if (ev.ClaimStatus == AirDropClaimStatus.Completed) {
                    claimBtn.gameObject.SetActive(false);
                    disableBtn.SetActive(true);
                    desc.text = "CLAIMED";
                    timeLeftTxt.text = null;
                } else if (ev.ClaimStatus == AirDropClaimStatus.Checking) {
                    claimBtn.gameObject.SetActive(false);
                    disableBtn.SetActive(true);
                    desc.text = "TRY AGAIN LATER";
                    timeLeftTxt.text = null;
                } else {
                    var canClaim = ev.BomberBought >= ev.BomberToBuy;
                    claimBtn.gameObject.SetActive(canClaim);
                    disableBtn.SetActive(!canClaim);
                    timeLeftTxt.text = null;
                }
                timeLeftTxt.text = $@"Time left: {ts:dd}D {ts:hh}H {ts:mm}M";
            } else {
                supplyTxt.text = null;
                if (data != null) {
                    title.text = data.title;
                    icon.sprite = data.icon;
                    _claimFee = data.fee;
                }
            }
        }

        public async void OnClaimBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            
            var str = _languageManager.GetValue(LocalizeKey.info_claim_bcoin_confirm);
            str = string.Format(str, $"{_event.EventName}", $"{App.Utils.FormatBcoinValue(_claimFee)} SEN");

            var dialog = await DialogConfirm.Create();
            dialog
                .SetInfo(str, "Claim", null, Claim, null)
                .Show(_dialogCanvas);
        }

        public void OnHelpBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _onHelpBtnClicked?.Invoke(_event);
        }
        
        private void Claim() {
            // Chưa Claim lần nào thì kiểm tra SEN
            if (_event.ClaimStatus == AirDropClaimStatus.Pending) {
                var sen = _chestRewardManager.GetChestReward(BlockRewardType.Senspark);
                if (sen < _claimFee) {
                    var msg = _languageManager.GetValue(LocalizeKey.info_not_enough);
                    DialogOK.ShowError(_dialogCanvas, string.Format(msg, "SEN"));
                    return;
                }
            }

            claimBtn.interactable = false;
            _onClaimBtnClicked?.Invoke(_event);
        }
    }
}