using System;

using App;

using Game.UI.LaunchPadTokens;

using JetBrains.Annotations;

using Senspark;

using Services.Rewards;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class ChestRewardTokenItem : MonoBehaviour {
        [SerializeField]
        private Image background;

        [SerializeField]
        private Color activeColor;
    
        [SerializeField]
        private Color inactiveColor;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text tokenName;
    
        [SerializeField]
        private Text balanceLbl;

        [SerializeField] [CanBeNull]
        private Text pendingLbl;

        [SerializeField]
        private Button claimBtn;
    
        [SerializeField]
        private Button depositBtn;
    
        [SerializeField]
        private Button farmBtn;
    
        [SerializeField]
        private Button helpBtn;

        [SerializeField]
        private bool showHelp;
    
        [SerializeField]
        private GameObject isFarmingBtn;

        [SerializeField]
        private GameObject buttonsGroup;

        private Action _onClaimBtnClicked;
        private Action _onDepositBtnClicked;
        private Action _onFarmBtnClicked;
        private Action _onHelpBtnClicked;
        private ISoundManager _soundManager;
        private bool _enableFarmBtn;
        private bool _isMining;
        private bool _isVisible;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            SetButtonsVisible(false);
        }

        public void SetTokenData(TokenData data) {
            icon.sprite = data.icon;
            tokenName.text = data.displayName;
        }

        public void SetClaimBtnInteractable(bool interactable) {
            claimBtn.interactable = interactable;
        }

        public void SetFarmBtnInteractable(bool canClick, bool isMining) {
            farmBtn.interactable = canClick;
            _isMining = isMining;
            ShowIsFarmingOrFarmBtn();
        }

        public void SetButtonsVisible(bool visible) {
            _isVisible = visible;
            buttonsGroup.gameObject.SetActive(visible);
            background.color = visible ? activeColor : inactiveColor;
            ShowIsFarmingOrFarmBtn();
            helpBtn.gameObject.SetActive(showHelp && visible);
        }

        public void SetEnableClaimBtn(bool enable, Action onClaimClicked) {
            claimBtn.gameObject.SetActive(enable);
            _onClaimBtnClicked = onClaimClicked;
        }
    
        public void SetEnableDepositBtn(bool enable, Action onDepositClicked) {
            depositBtn.gameObject.SetActive(enable);
            _onDepositBtnClicked = onDepositClicked;
        }

        public void SetEnableFarmBtn(bool enable, Action onFarmClicked) {
            SetEnableFarmBtn(enable);
            _onFarmBtnClicked = onFarmClicked;
        }

        public void SetHelpBtn(Action onHelpClicked) {
            _onHelpBtnClicked = onHelpClicked;
        }

        public void SetEnableFarmBtn(bool enable) {
            _enableFarmBtn = enable;
            farmBtn.gameObject.SetActive(enable);
            isFarmingBtn.gameObject.SetActive(enable);
        }

        public void SetBalance(float claimableValue, float pendingValue) {
            balanceLbl.text = App.Utils.FormatBcoinValue(claimableValue);
            if (pendingLbl && pendingValue > 0) {
                pendingLbl.text = App.Utils.FormatBcoinValue(pendingValue);
            }
        }

        public void OnClaimBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _onClaimBtnClicked?.Invoke();
        }

        public void OnDepositBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _onDepositBtnClicked?.Invoke();
        }

        public void OnFarmBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _onFarmBtnClicked?.Invoke();
        }

        public void OnHelpBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _onHelpBtnClicked?.Invoke();
        }

        private void ShowIsFarmingOrFarmBtn() {
            if (_enableFarmBtn) {
                if (_isMining) {
                    isFarmingBtn.gameObject.SetActive(true);
                    farmBtn.gameObject.SetActive(false);
                } else {
                    isFarmingBtn.gameObject.SetActive(false);
                    farmBtn.gameObject.SetActive(_isVisible);
                }
            }
        }
    }
}