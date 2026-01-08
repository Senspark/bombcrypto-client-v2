using System;
using JetBrains.Annotations;
using Scenes.FarmingScene.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLWallet {
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
        
        [SerializeField]
        private RectTransform rtContent;
        
        private DataWallet _info;
        
        [CanBeNull]
        private Action<DataWallet> _onDeposit = null;
        
        [CanBeNull]
        private Action<DataWallet> _onWithdraw = null;

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
            if (info.RefTokenData != null) {
                icon.sprite = info.RefTokenData.icon;
            }
            contentText.text = data == null ? "" : data.content;
            btDeposit.interactable = info.IsEnableDeposit;
            btWithdraw.interactable = info.IsEnableWithdraw;
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
