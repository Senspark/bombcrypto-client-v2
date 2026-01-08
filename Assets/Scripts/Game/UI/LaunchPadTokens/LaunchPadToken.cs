using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Scenes.FarmingScene.Scripts;

using Services.Rewards;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI.LaunchPadTokens {
    [RequireComponent(typeof(ChestRewardTokenItem))]
    public class LaunchPadToken : MonoBehaviour {
        [SerializeField]
        private ChestRewardTokenItem tokenItem;

        [SerializeField]
        private EventTrigger eventTrigger;
        
        public TokenData TokenData { get; private set; }

        private float _claimableValue;
        private float _pendingValue;
        private IRewardType _rewardType;
        private Action<string, IRewardType> _onClaimClicked;
        private Action<IRewardType> _onFarmClicked;
        private Canvas _dialogCanvas;
        private ILaunchPadManager _launchPadManager;
        private ILanguageManager _languageManager;
        private IFeatureManager _featureManager;
        private IStorageManager _storageManager;
        private NetworkType _currentNetwork;

        public void InitCallback(
            Action<ChestRewardTokenItem> onTokenHover,
            Action<BaseEventData> onScroll,
            Action<string, IRewardType> onClaimClicked,
            Action<IRewardType> onFarmClicked
        ) {
            _onClaimClicked = onClaimClicked;
            _onFarmClicked = onFarmClicked;

            var pointerClick = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerClick
            };
            pointerClick.callback.AddListener(x => onTokenHover?.Invoke(tokenItem));
            eventTrigger.triggers.Add(pointerClick);

            var pointerEnter = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener(x => onTokenHover?.Invoke(tokenItem));
            eventTrigger.triggers.Add(pointerEnter);

            var pointerScroll = new EventTrigger.Entry {
                eventID = EventTriggerType.Scroll
            };
            pointerScroll.callback.AddListener(x => onScroll?.Invoke(x));
            eventTrigger.triggers.Add(pointerScroll);
        }

        public void InitData(
            TokenData data,
            IRewardType rewardType,
            float claimableValue,
            float pendingValue,
            Canvas dialogCanvas,
            IFeatureManager featureManager,
            ILaunchPadManager launchPadManager,
            IStorageManager storageManager,
            ILanguageManager languageManager,
            NetworkType currentNetwork
        ) {
            _dialogCanvas = dialogCanvas;
            TokenData = data;
            _rewardType = rewardType;
            _claimableValue = claimableValue;
            _pendingValue = pendingValue;
            _launchPadManager = launchPadManager;
            _languageManager = languageManager;
            _featureManager = featureManager;
            _storageManager = storageManager;
            _currentNetwork = currentNetwork;
            
            tokenItem.SetTokenData(data);

            var canClaim = featureManager.EnableClaim && data.enableClaim;
            var canDeposit = featureManager.EnableDeposit && data.enableDeposit;
            tokenItem.SetEnableClaimBtn(canClaim, OnClaimBtnClicked);
            tokenItem.SetEnableDepositBtn(canDeposit, OnDepositBtnClicked);
            tokenItem.SetEnableFarmBtn(data.enableFarm, OnFarmBtnClicked);
            tokenItem.SetHelpBtn(OnHelpBtnClicked);
            
            TurnOffButtons();
            Refresh();
        }

        public void Refresh() {
            // Balance
            tokenItem.SetBalance(_claimableValue, _pendingValue);

            // Claim Btn
            tokenItem.SetClaimBtnInteractable(CanClaim());

            // Farm Btn
            var tokenName = TokenData.tokenName;
            var canControl = _featureManager.EnableControlMining;
            var currentMiningToken = _storageManager.MiningTokenType;
            var canClickFarmBtn = currentMiningToken != tokenName;
            var isFarming = tokenName == currentMiningToken && TokenData.NetworkSymbol == _currentNetwork;
            tokenItem.SetFarmBtnInteractable(canClickFarmBtn, isFarming);

            // Show Current Mining Icon
            if (!canControl) {
                tokenItem.SetEnableFarmBtn(!canClickFarmBtn);
            }
        }

        public void OnClaimBtnClicked() {
            tokenItem.SetClaimBtnInteractable(false);
            if (!CanClaim()) {
                return;
            }

            // Nếu đang có pending thì cho retry claim
            if (_pendingValue > 0) {
                _onClaimClicked?.Invoke(null, _rewardType);
                return;
            }

            // Claim trừ fee như bình thường
            var (claimFee, currency) =
                _launchPadManager.GetClaimFee(_rewardType, NetworkSymbol.Convert(_currentNetwork));
            if (string.IsNullOrWhiteSpace(currency)) {
                _onClaimClicked?.Invoke(null, _rewardType);
            } else {
                var canClaim = _claimableValue - claimFee;
                var str = _languageManager.GetValue(LocalizeKey.info_claim_bcoin_confirm);
                str = string.Format(str,
                    $"{App.Utils.FormatBcoinValue(canClaim)} {TokenData.displayName}",
                    $"{App.Utils.FormatBcoinValue(claimFee)} {currency}");
                _onClaimClicked?.Invoke(str, _rewardType);
            }
        }

        public void OnFarmBtnClicked() {
            _onFarmClicked?.Invoke(_rewardType);
        }
        
        public void OnDepositBtnClicked() {
            DialogDeposit.Create().ContinueWith(dialog => {
                dialog.Init(TokenData).Show(_dialogCanvas);
            });
        }

        public async void OnHelpBtnClicked() {
            var dialog = await DialogInformation.Create();
            dialog.OpenTab(_rewardType.Name).Show(_dialogCanvas);
        }

        public void TurnOffButtons() {
            tokenItem.SetButtonsVisible(false);
        }

        private bool CanClaim() {
            var haveClaimFeature = _featureManager.EnableClaim;
            var symbol = NetworkSymbol.Convert(_currentNetwork);
            var canClaim = _launchPadManager.CanClaim(_rewardType, symbol, _claimableValue) || _pendingValue > 0;
            return haveClaimFeature && canClaim;
        }
    }
}