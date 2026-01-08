using System;

using Analytics;

using App;

using Constant;

using Cysharp.Threading.Tasks;

using Game.Manager;
using Game.UI;

using Scenes.ShopScene.Scripts;

using Senspark;

using Services.Server.Exceptions;

using Sfs2X.Entities.Data;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

using Utils;

namespace Game.Dialog {
    public class BLSwapGem : MonoBehaviour {
        [FormerlySerializedAs("_balanceText")]
        [SerializeField]
        private Text balanceText;

        [FormerlySerializedAs("_balanceInput")]
        [SerializeField]
        private InputField balanceInput;

        [FormerlySerializedAs("_swapButton")]
        [SerializeField]
        private Button swapButton;

        [FormerlySerializedAs("_tokenBalanceText")]
        [SerializeField]
        private Text tokenBalanceText;

        [FormerlySerializedAs("_tokenPrice")]
        [SerializeField]
        private Text tokenPrice;

        [FormerlySerializedAs("_tokenOutput")]
        [SerializeField]
        private Text tokenOutput;

        [SerializeField]
        private SwapTokenButton swapTokenButton;
        
        [SerializeField]
        private Text minimumText;

        private IChestRewardManager _chestRewardManager;
        private ILogManager _logManager;
        private IServerManager _serverManager;
        private ISoundManager _soundManager;
        private IAnalytics _analytics;
        private IUserAccountManager _userAccountManager;
        private INetworkConfig _networkConfig;
        private UserAccount _account;

        private float _balance;
        private Canvas _canvas;
        private float _gemBalance;
        private double _tokenPrice;
        private int _tokenType = (int)RewardType.BCOIN;
        private bool _isExit = false;
        private double _ratioBcoin;
        private double _ratioSen;
        private int _minGemSwap;

        private float BalanceInputText {
            set => balanceInput.text = value == 0 ? "" : $"{Math.Truncate(value)}";
        }

        private void Awake() {
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _networkConfig = ServiceLocator.Instance.Resolve<INetworkConfig>();
            _isExit = true;
        }

        private void OnDestroy() {
            _isExit = false;
        }

        public void Initialize(Canvas canvas) {
            _account = _userAccountManager.GetRememberedAccount() ??
                       throw new Exception("Account is null");
            _canvas = canvas;
            tokenOutput.text = "0";
            swapButton.interactable = false;
            var self = this;
            UniTask.Void(async () => {
                try {
                    var rBcoin = await _serverManager.General.PreviewToken(1, _account.network, (int)RewardType.BCOIN);
                    var rSen = await _serverManager.General.PreviewToken(1, _account.network, (int)RewardType.Senspark);
                    _minGemSwap = await _serverManager.General.GetSwapTokenConfig();
                    if (!self._isExit) {
                        return;
                    }
                    minimumText.text = "Minimum: " + _minGemSwap;
                    _ratioBcoin = rBcoin;
                    _ratioSen = rSen;
                    UpdateUI();
                } catch (Exception e) {
                    DialogOK.ShowError(canvas, e.Message);
                }
            });
            swapTokenButton.SetChangeTokenCallback(currentType => {
                _tokenType = (int)currentType;
                UpdateUI();
                tokenOutput.text = (_balance * GetRatioToken()).ToString("0.########");
            });
            UpdateUI();
        }

        public void OnBalanceChanged(string value) {
            _logManager.Log($"value: {value}");
            try {
                _balance = value == string.Empty ? 0f : Mathf.Min(float.Parse(value), _gemBalance);
            } finally {
                tokenOutput.text = (_balance * GetRatioToken()).ToString("0.########");
                BalanceInputText = _balance;
                swapButton.interactable = _balance >= _minGemSwap;
            }
        }

        public void OnButtonMaxClicked() {
            _soundManager.PlaySound(Audio.Tap);
            BalanceInputText = _chestRewardManager.GetChestReward(BlockRewardType.Gem);
            _logManager.Log($"input: {balanceInput.text}");
            _logManager.Log($"balance: {_balance}");
        }

        public async void OnButtonSwapClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var waiting = new WaitingUiManager(_canvas);
            waiting.Begin(0);
            var confirm = await DialogConfirm.Create();

            var data = new SFSObject();
            data.PutUtfString("Amount", $"{_balance} Gem");
            data.PutUtfString("Network", _networkConfig.NetworkName);
            data.PutUtfString("TokenType", RewardTypeUtils.GetName(_tokenType));

            confirm.SetInfo(
                "Do you want to swap?",
                "Yes",
                "No",
                () => UniTask.Void(async () => {
                    var ratio = GetRatioToken();
                    try {
                        var tokenReceive = await _serverManager.General.SwapToken(_balance, _account.network, _tokenType);
                        _analytics.Iap_TrackSwapGem(_balance, (float)(_balance * ratio), TrackResult.Done);
                        await _serverManager.General.GetChestReward();
                        DialogSwapGemSuccess.ShowInfo(_canvas, _balance * ratio, _tokenType);
                        data.PutFloat("Receive", tokenReceive);
                        _serverManager.General.SendMessageSlack(":diamonds: User Swap Gem :white_check_mark:", data);
                        UpdateUI();
                        waiting.End();
                    } catch (Exception e) {
                        _analytics.Iap_TrackSwapGem(_balance, (float)(_balance * ratio), TrackResult.Error);
                        data.PutUtfString("Error", e.Message);
                        _serverManager.General.SendMessageSlack(":diamonds: User Swap Gem :x:", data);
                        waiting.End();
                        if (e is ErrorCodeException) {
                            DialogError.ShowError(_canvas, e.Message);
                        } else {
                            DialogOK.ShowError(_canvas, e.Message);
                        }
                    }
                }),
                () => waiting.End()
            );
            confirm.Show(_canvas);
        }

        public void HideChangeToken() {
            swapTokenButton.HideChangeToken();
        }

        private double GetRatioToken() {
            if (_tokenType == (int)RewardType.BCOIN) {
                return _ratioBcoin;
            }
            return _ratioSen;
        }

        private void UpdateUI() {
            _gemBalance = _chestRewardManager.GetChestReward(BlockRewardType.Gem);
            _balance = Mathf.Min(_balance, _gemBalance);
            BalanceInputText = _balance;
            balanceText.text = $"{Math.Truncate(_gemBalance)}";
            tokenBalanceText.text =
                $"{Math.Truncate(_chestRewardManager.GetChestReward(RewardTypeUtils.GetName(_tokenType)))}";
            tokenPrice.text = $"1 GEM = {GetRatioToken():0.########} {RewardTypeUtils.GetName(_tokenType)}";
        }
    }
}