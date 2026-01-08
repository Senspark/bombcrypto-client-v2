using System;
using System.Collections.Generic;

using App;

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

            var t = RewardUtils.ConvertToBlockRewardType(_depositToken.tokenName);
            _depositType = (t, _networkType) switch {
                (BlockRewardType.BCoinDeposited, NetworkType.Binance) => RpcTokenCategory.Bcoin,
                (BlockRewardType.BCoin, NetworkType.Binance) => RpcTokenCategory.Bcoin,
                (BlockRewardType.BCoinDeposited, NetworkType.Polygon) => RpcTokenCategory.Bomb,
                (BlockRewardType.BCoin, NetworkType.Polygon) => RpcTokenCategory.Bomb,
                (BlockRewardType.SensparkDeposited, NetworkType.Binance) => RpcTokenCategory.SenBsc,
                (BlockRewardType.Senspark, NetworkType.Binance) => RpcTokenCategory.SenBsc,
                (BlockRewardType.SensparkDeposited, NetworkType.Polygon) => RpcTokenCategory.SenPolygon,
                (BlockRewardType.Senspark, NetworkType.Polygon) => RpcTokenCategory.SenPolygon,
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
                DialogOK.ShowError(DialogCanvas, "Invalid Token");
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
                    var category = _depositType switch {
                        RpcTokenCategory.Bcoin or RpcTokenCategory.Bomb => 0,
                        RpcTokenCategory.SenBsc or RpcTokenCategory.SenPolygon => 1,
                        _ => throw new Exception("Invalid Token")
                    };
                    var success = await _blockChainManager.Deposit(_depositValue, category);

                    if (success) {
                        await _serverManager.General.SyncDeposited();
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
                        DialogError.ShowError(DialogCanvas, e.Message, () => { _isClicked = false; });
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message, () => { _isClicked = false; });
                    }
                } finally {
                    waiting.Hide();
                }
            });
        }
    }
}