using System;
using JetBrains.Annotations;
using Scenes.FarmingScene.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLWallet {
    public enum ClaimButtonMode { Withdraw, Waiting }

    public class BLWalletInformation : MonoBehaviour
    {
        [SerializeField]
        private Text contentText;

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private Slider slider;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Button btDeposit;

        [SerializeField]
        private Button btWithdraw;

        [SerializeField, CanBeNull]
        private Text btWithdrawLabel;

        [SerializeField]
        private RectTransform rtContent;

        private DataWallet _info;
        private ClaimButtonMode _claimMode = ClaimButtonMode.Withdraw;
        private bool _claimInteractableOverride = true;

        [CanBeNull]
        private Action<DataWallet> _onDeposit = null;

        [CanBeNull]
        private Action<DataWallet> _onWithdraw = null;

        private void Awake() {
            if (!btWithdrawLabel && btWithdraw) {
                btWithdrawLabel = btWithdraw.GetComponentInChildren<Text>(true);
            }
        }

        public void UiApplyStyle(TypeMenuLeftWallet tab) {
            switch (tab) {
                case TypeMenuLeftWallet.Mine:
                    btDeposit.gameObject.SetActive(false);
                    btWithdraw.gameObject.SetActive(false);
                    rtContent.SetBottom(25);
                    break;
                case TypeMenuLeftWallet.Deposit:
                    btDeposit.gameObject.SetActive(true);
                    btWithdraw.gameObject.SetActive(false);
                    rtContent.SetBottom(80);
                    break;
                case TypeMenuLeftWallet.Withdraw:
                    btDeposit.gameObject.SetActive(false);
                    btWithdraw.gameObject.SetActive(true);
                    rtContent.SetBottom(80);
                    break;
            }
        }

        public void DisplayInfo(DataWallet info) {
            _info = info;
            var data = info.RefInfo;
            if (info.IsBridge && info.BridgeIcon) {
                icon.sprite = info.BridgeIcon;
            } else if (info.RefTokenData != null) {
                icon.sprite = info.RefTokenData.icon;
            }
            contentText.text = data == null ? "" : data.content;
            btDeposit.interactable = info.IsEnableDeposit;
            RefreshWithdrawInteractable();
        }

        public void SetClaimButton(ClaimButtonMode mode, bool interactable) {
            _claimMode = mode;
            _claimInteractableOverride = interactable;
            if (btWithdrawLabel) {
                btWithdrawLabel.text = mode switch {
                    ClaimButtonMode.Waiting => "WAITING",
                    _ => "WITHDRAW",
                };
            }
            RefreshWithdrawInteractable();
        }

        private void RefreshWithdrawInteractable() {
            btWithdraw.interactable = _info.IsEnableWithdraw && _claimInteractableOverride;
        }
        
        public void OnSliderValueChanged(float _) {
            if (Math.Abs(scrollRect.verticalNormalizedPosition - slider.normalizedValue) > 0.01f) {
                scrollRect.verticalNormalizedPosition = slider.normalizedValue;
            }
        }
        
        public void SetOnDeposit(Action<DataWallet> onDeposit) {
            _onDeposit = onDeposit;
        }

        public void SetOnWithdraw(Action<DataWallet> onWithdraw) {
            _onWithdraw = onWithdraw;
        }

        public void OnBtDepositClick() {
            _onDeposit?.Invoke(_info);
        }
        
        public void OnBtWithdrawClick() {
            _onWithdraw?.Invoke(_info);
        }
    }
}
