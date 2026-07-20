using System;
using System.Collections.Generic;
using System.Numerics;

using App;
using App.BomberLand;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Services.Rewards;
using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogDeposit : Dialog {
        [SerializeField]
        private Text depositValue;

        [SerializeField]
        private List<IntValueButton> valuesButtons;

        [SerializeField]
        private List<Image> coinIcons;

        [SerializeField]
        private Button confirmButton;

        private ISoundManager _soundManager;
        private IBlockchainManager _blockChainManager;
        private IStorageManager _storeManager;
        private IServerManager _serverManager;
        private IBlockchainStorageManager _blockchainStorageManager;
        private IFeatureManager _featureManager;

        private NetworkType _networkType;
        private RpcTokenCategory _depositType;
        private TokenData _depositToken;
        private int _depositValue;

        // preset selection + balance check.
        private bool _isBridge;
        private string _bridgeSymbol;

        private bool _isClicked;
        private readonly int[] _bcoinValues = { 100, 200, 500 };
        private readonly int[] _senValues = { 500, 1000, 2000 };

        public static UniTask<DialogDeposit> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogDeposit>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _blockChainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
            _networkType = ServiceLocator.Instance.Resolve<INetworkConfig>().NetworkType;
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();

            confirmButton.interactable = false;
        }

        public DialogDeposit Init(TokenData depositToken) {
            _depositToken = depositToken;

            var t = _depositToken.tokenName;
            _isBridge = t is BlockRewardType.BcoinBridge or BlockRewardType.SenBridge;
            _bridgeSymbol = t == BlockRewardType.SenBridge ? "SEN" : "BCOIN";
            _depositType = (t, _networkType) switch {
                (BlockRewardType.BCoinDeposited, NetworkType.Binance) => RpcTokenCategory.Bcoin,
                (BlockRewardType.BCoin, NetworkType.Binance) => RpcTokenCategory.Bcoin,
                (BlockRewardType.BcoinBridge, NetworkType.Binance) => RpcTokenCategory.Bcoin,
                (BlockRewardType.BCoinDeposited, NetworkType.Polygon) => RpcTokenCategory.Bomb,
                (BlockRewardType.BCoin, NetworkType.Polygon) => RpcTokenCategory.Bomb,
                (BlockRewardType.BcoinBridge, NetworkType.Polygon) => RpcTokenCategory.Bomb,
                (BlockRewardType.SensparkDeposited, NetworkType.Binance) => RpcTokenCategory.SenBsc,
                (BlockRewardType.Senspark, NetworkType.Binance) => RpcTokenCategory.SenBsc,
                (BlockRewardType.SenBridge, NetworkType.Binance) => RpcTokenCategory.SenBsc,
                (BlockRewardType.SensparkDeposited, NetworkType.Polygon) => RpcTokenCategory.SenPolygon,
                (BlockRewardType.Senspark, NetworkType.Polygon) => RpcTokenCategory.SenPolygon,
                (BlockRewardType.SenBridge, NetworkType.Polygon) => RpcTokenCategory.SenPolygon,
                _ => throw new ArgumentOutOfRangeException(t.ToString())
            };

            try {
                var currentCoin = _blockchainStorageManager.GetBalance(_depositType);
                var prices = _depositType switch {
                    RpcTokenCategory.Bcoin or RpcTokenCategory.Bomb => _bcoinValues,
                    RpcTokenCategory.SenBsc or RpcTokenCategory.SenPolygon => _senValues,
                    _ => throw new Exception()
                };
                for (var i = 0; i < prices.Length; i++) {
                    var btn = valuesButtons[i];
                    btn.SetCallback(OnSelectDepositValue);
                    btn.SetValue(prices[i]);
                    btn.SetInteractable(currentCoin >= btn.Value);
                    btn.Selected = false;
                }
                coinIcons.ForEach(e => e.sprite = _depositToken.icon);
            } catch (Exception) {
                DialogOK.ShowErrorMsgOnly(DialogCanvas, "Invalid Token");
                Hide();
            }
            return this;
        }

        private void OnSelectDepositValue(IntValueButton selected) {
            var value = selected.Value;
            _depositValue = value;
            depositValue.text = value.ToString();
            valuesButtons.ForEach(e => e.Selected = false);
            selected.Selected = true;
            confirmButton.interactable = true;
        }

        protected override void OnYesClick() {
            if(!confirmButton.IsInteractable())
                return;
            
            if (_isClicked)
                return;
            _isClicked = true;
            OnConfirmBtnClicked();
        }

        // Best-effort bridge activity report — fire-and-forget so it never blocks the deposit flow.
        private void FireBridgeNotify(string kind, int serverType, string chain, string txHash = null) {
            UniTask.Void(async () => {
                try {
                    await _serverManager.General.NotifyCrosschainBridge(kind, serverType, chain, txHash);
                } catch (Exception e) {
                    UnityEngine.Debug.LogWarning($"[Bridge-notify] {kind} failed: {e.Message}");
                }
            });
        }

        public void OnConfirmBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);

            UniTask.Void(async () => {
                var waiting = await DialogWaiting.Create();
                waiting.Show(DialogCanvas);
                waiting.ShowLoadingAnim();

                try {
                    if (!_featureManager.EnableDeposit) {
                        throw new Exception("Not support");
                    }
                    bool success;
                    if (_isBridge) {
                        var amountWei = (new BigInteger(_depositValue) * BigInteger.Pow(10, 18)).ToString();
                        var chain = _networkType == NetworkType.Polygon ? "POLYGON" : "BSC";
                        // Server bridge block-reward-type codes (match BLDialogReward.BridgeIds): BCOIN=29, SEN=30.
                        var serverType = _bridgeSymbol == "SEN" ? 30 : 29;
                        FireBridgeNotify(BridgeNotifyKind.DepositPrepare, serverType, chain);
                        var res = await _blockChainManager.BridgeDeposit(chain, _bridgeSymbol, amountWei);
                        success = res.success;
                        if (success) FireBridgeNotify(BridgeNotifyKind.DepositDone, serverType, chain, res.txHash);
                    } else {
                        var category = _depositType switch {
                            RpcTokenCategory.Bcoin or RpcTokenCategory.Bomb => 0,
                            RpcTokenCategory.SenBsc or RpcTokenCategory.SenPolygon => 1,
                            _ => throw new Exception("Invalid Token")
                        };
                        success = await _blockChainManager.Deposit(_depositValue, category);
                    }

                    if (success) {
                        await _serverManager.General.SyncDeposited(
                            _isBridge ? DepositSyncTarget.Bridge : DepositSyncTarget.Old);
                        await App.Utils.WaitForBalanceChange(_depositType, _blockChainManager,
                            _blockchainStorageManager);
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Deposit Successfully");
                        Hide();
                    } else {
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Deposit Failed", new DialogOK.Optional {
                            OnDidHide =
                                () => { _isClicked = false; }
                        });
                    }
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e, () => { _isClicked = false; });
                    } else {
                        DialogOK.ShowError(DialogCanvas, e, () => { _isClicked = false; });
                    }
                } finally {
                    waiting.Hide();
                }
            });
        }
    }
}