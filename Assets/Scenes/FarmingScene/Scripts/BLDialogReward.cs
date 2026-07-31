using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using App;
using App.BomberLand;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Game.Dialog.BomberLand.BLWallet;
using Game.Manager;
using Game.UI;
using Game.UI.Information;
using Senspark;
using Server.Models;
using Services.Rewards;
using Services.Server.Exceptions;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using Utils;

namespace Scenes.FarmingScene.Scripts {
    public struct DataWallet {
        public TokenData RefTokenData;
        public ITokenReward RefTokenReward;
        public IRewardType RefRewardType;
        public float ClaimValue;
        public float PendingValue;
        public InformationData RefInfo;
        public bool IsEnableDeposit;
        public bool IsEnableWithdraw;
        public bool IsBridge;
        public bool BridgeBalanceKnown;
        public string BridgeSymbol;
        public string BridgeDepositChain;
        public string BridgeWithdrawChain;
        public Sprite BridgeIcon;
    }

    public class BLDialogReward : Dialog {
        [SerializeField]
        private WaitingPanel waitingPanel;

        [SerializeField]
        private BLFrameWallet frameWallet;

        [SerializeField]
        private BLFrameWallet[] frameWallets;

        [SerializeField]
        private Sprite bcoinBridgeIcon;

        [SerializeField]
        private Sprite senBridgeIcon;

        private IBlockchainManager _blockchainManager;
        private IServerManager _serverManager;
        private IStorageManager _storeManager;
        private ISoundManager _soundManager;
        private ILanguageManager _languageManager;
        private IChestRewardManager _chestRewardManager;
        private ILaunchPadManager _launchPadManager;
        private ILogManager _logManager;
        private IClaimTokenManager _claimTokenManager;
        private INetworkConfig _networkConfig;
        private IInformationManager _informationManager;
        private IBlockchainStorageManager _blockchainStorageManager;
        private IUserSolanaManager _userSolanaManager;

        private CancellationTokenSource _cancellation;
        private ObserverHandle _handle;
        private BLDialogRewardController _controller;

        private Dictionary<NetworkType, BLDialogRewardController.BlockchainHeroAmount> _heroAmount;
        private List<DataWallet> _walletDataFull;
        private DataWallet? _currentSelectedWallet;
        private bool _bridgeWithdrawInFlight;
        private bool _nativeInFlight;
        private double _bridgeFeePercent = 5;
        private bool _bridgeFeeLoaded;
        private List<DataWallet> _bridgeRows;
        private List<DataWallet> _bridgeDepositBase;
        private List<DataWallet> _bridgeWithdrawBase;

        public static UniTask<BLDialogReward> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogReward>();
        }

        protected override void Awake() {
            base.Awake();
            IgnoreOutsideClick = true;
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _launchPadManager = ServiceLocator.Instance.Resolve<ILaunchPadManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _claimTokenManager = ServiceLocator.Instance.Resolve<IClaimTokenManager>();
            _networkConfig = ServiceLocator.Instance.Resolve<INetworkConfig>();
            _informationManager = ServiceLocator.Instance.Resolve<IInformationManager>();
            _blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
            _userSolanaManager = ServiceLocator.Instance.Resolve<IServerManager>().UserSolanaManager;
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();

            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnChestReward = OnChestReward
            });
            _handle.AddObserver(_claimTokenManager, new ClaimTokenObserver {
                OnStateChanged = ApplyClaimButtonState,
            });
            _controller = new BLDialogRewardController(_serverManager, _launchPadManager, _claimTokenManager,
                _chestRewardManager, _blockchainManager, _blockchainStorageManager, _storeManager, featureManager,
                _networkConfig.NetworkType);
            waitingPanel.gameObject.SetActive(true);

            if (frameWallets is { Length: 2 }) {
                var isPad = ScreenUtils.IsIPadScreen();
                if (isPad) {
                    frameWallets[0].gameObject.SetActive(false);
                    frameWallets[1].gameObject.SetActive(true);
                    frameWallet = frameWallets[1];
                } else {
                    frameWallets[0].gameObject.SetActive(true);
                    frameWallets[1].gameObject.SetActive(false);
                }
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _handle.Dispose();
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        private void Start() {
            _cancellation = new CancellationTokenSource();
            UniTask.Void(async token => {
                try {
                    frameWallet.OnClaim = OnClaim;
                    frameWallet.OnSelectionChanged = OnSelectionChanged;
                    await frameWallet.InitUi();
                    await RefreshClaimableHeroAmount();
                    await _serverManager.General.GetChestReward();
                    waitingPanel.gameObject.SetActive(false);
                } catch (Exception e) when (!token.IsCancellationRequested) {
                    _logManager.Log(e.Message);
                    DialogOK.ShowError(DialogCanvas, e);
                    Hide();
                }
            }, _cancellation.Token);
        }

        private async Task RefreshClaimableHeroAmount() {
            _heroAmount = new Dictionary<NetworkType, BLDialogRewardController.BlockchainHeroAmount>();
            if (AppConfig.IsAirDrop()) {
                var heroAmount = new BLDialogRewardController.BlockchainHeroAmount {
                    ClaimableHero = 0,
                    GiveAwayHero = 0,
                    PendingHero = 0
                };
                _heroAmount[_networkConfig.NetworkType] = heroAmount;
                return;
            }
            var networkAmount = (NetworkType[])Enum.GetValues(typeof(NetworkType));
            foreach (var n in networkAmount) {
                _heroAmount[n] = await _controller.GetHeroOnBlockchain(n);
            }
        }

        private void InstantiateTokens(IChestReward chestReward) {
            var wallets = new List<DataWallet>();
            var addedData = new List<TokenData>();

            foreach (var t in chestReward.Rewards) {
                //DevHoang: Add new airdrop
                var rewardType = t.Type;
                if (AppConfig.IsTon()) {
                    var validRewardTypes = new List<BlockRewardType> {
                            BlockRewardType.BLCoin, BlockRewardType.TonDeposited,
                            BlockRewardType.Hero, BlockRewardType.BCoinDeposited
                        };
                    if (!validRewardTypes.Contains(rewardType.Type)) {
                        continue;
                    }
                } else if (AppConfig.IsSolana()) {
                    if (t.Network != nameof(DataType.SOL)
                        && rewardType.Type != BlockRewardType.BLCoin) {
                        continue;
                    }
                } 
                // RON ,VIC, BAS chỉ hiện starcore network, ko hiện star core TR
                else if (AppConfig.IsRonin()) {
                    if (t.Network != nameof(DataType.RON)) {
                        continue;
                    }
                } else if (AppConfig.IsBase()) {
                    if (t.Network != nameof(DataType.BAS)) {
                        continue;
                    }
                } else if (AppConfig.IsViction()) {
                    if (t.Network != nameof(DataType.VIC)) {
                        continue;
                    }
                }
                // BSC, POL ko hiện đồng deposit của network khác
                else {
                    if (rewardType.Type == BlockRewardType.TonDeposited ||
                        rewardType.Type == BlockRewardType.SolDeposited ||
                        rewardType.Type == BlockRewardType.RonDeposited ||
                        rewardType.Type == BlockRewardType.BasDeposited ||
                        rewardType.Type == BlockRewardType.VicDeposited) {
                        continue;
                    }
                    // BSC, POL chỉ hiện starcore TR, ko hiện starcore network
                    if(rewardType.Type == BlockRewardType.BLCoin && t.Network != nameof(DataType.TR) ) {
                        continue;
                    }
                }
                if (_launchPadManager.CanShowInLaunchPad(t)) {
                    var data = _launchPadManager.GetData(t);
                    var claimValue = t.Value;
                    if (rewardType.Type == BlockRewardType.Hero) {
                        var n = RewardUtils.ConvertToNetworkType(t.Network);
                        claimValue += _heroAmount[n].GetTotal();
                    }
                    addedData.Add(data);
                    // Add Data
                    DataWallet d = default;
                    d.RefTokenData = data;
                    d.RefTokenReward = t;
                    d.RefRewardType = rewardType;
                    d.ClaimValue = claimValue;
                    d.PendingValue = t.ClaimPending;
                    d.IsEnableDeposit = _controller.CanDeposit(data);
                    d.IsEnableWithdraw = _controller.CanWidthDraw(data, rewardType);
                    var info = _informationManager.GetTokenData(t);
                    d.RefInfo = info;
                    wallets.Add(d);
                }
            }

            // FORCE DISPLAY TOKENS
            List<TokenData> forceDisplayTokens;
            //DevHoang: Add new airdrop
            if (AppConfig.IsTon()) {
                forceDisplayTokens = _launchPadManager.GetForceDisplayTokensTelegram();
            } else if (AppConfig.IsSolana()) {
                forceDisplayTokens = _launchPadManager.GetForceDisplayTokensSolana();
            } else if (AppConfig.IsRonin()) {
                forceDisplayTokens = _launchPadManager.GetForceDisplayTokensRonin();
            } else if (AppConfig.IsBase()) {
                forceDisplayTokens = _launchPadManager.GetForceDisplayTokensBase();
            } else if (AppConfig.IsViction()) {
                forceDisplayTokens = _launchPadManager.GetForceDisplayTokensViction();
            } else {
                forceDisplayTokens = _launchPadManager.GetForceDisplayTokens();
            }
            foreach (var data in forceDisplayTokens) {
                if (!addedData.Contains(data)) {
                    var rewardType = _launchPadManager.CreateRewardType(data.tokenName);
                    var claimValue = 0;
                    if (rewardType.Type == BlockRewardType.Hero) {
                        var n = RewardUtils.ConvertToNetworkType(data.networkSymbol.ToString());
                        claimValue += _heroAmount[n].GetTotal();
                    }
                    DataWallet d = default;
                    d.RefTokenData = data;
                    d.RefTokenReward = new TokenReward(RewardUtils.ConvertToBlockRewardType(data.tokenName), data.networkSymbol.ToString());
                    d.RefRewardType = rewardType;
                    d.ClaimValue = claimValue;
                    d.PendingValue = 0;
                    d.IsEnableDeposit = _controller.CanDeposit(data);
                    d.IsEnableWithdraw = _controller.CanWidthDraw(data, rewardType);
                    var info = _informationManager.GetTokenData(d.RefTokenReward);
                    d.RefInfo = info;
                    wallets.Add(d);
                }
            }
            _walletDataFull = wallets;
            //
            RefreshUi();
        }

        private void UpdateAllTokensUI() {
        }

        #region V4 CLAIM

        private void ClaimFlow(DataWallet wallet) {
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                try {
                    _controller.ThrowIfCannotClaim(wallet);
                    var result = await _claimTokenManager.Claim(wallet.RefRewardType.Type, wallet.RefTokenData.code);
                    if (!this) return;
                    var type = wallet.RefRewardType.Type;
                    if (type == BlockRewardType.Hero) {
                        await RefreshClaimableHeroAmount();
                        await _serverManager.General.GetChestReward();
                        if (!this) return;
                        if (result.FusionFailAmount > 0) {
                            await ProcessTokenHelper.FusionFailed(DialogCanvas, result.FusionFailAmount);
                        }
                    } else {
                        var coinResult = await _controller.WaitForClaimCompletion(wallet, result.ClaimedAmount);
                        if (!this) return;
                        if (coinResult.Successful) {
                            var dialog = await DialogBCoinReward.Create();
                            if (!this) return;
                            dialog.SetReward(wallet.RefTokenData, coinResult.ClaimValue, DialogCanvas).Show(DialogCanvas);
                        } else {
                            DialogOK.ShowInfo(DialogCanvas, "Claim Fail");
                        }
                    }
                } catch (Exception e) {
                    if (this) DialogOK.ShowError(DialogCanvas, e);
                } finally {
                    if (this) UpdateAllTokensUI();
                    waiting.Hide();
                }
            });
        }

        private void BridgeWithdrawFlow(DataWallet wallet) {
            if (_bridgeWithdrawInFlight) return;
            var (serverType, symbol) = BridgeIds(wallet.RefRewardType.Type);
            var withdrawChain = wallet.BridgeWithdrawChain;
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                float? withdrawable;
                double feePercent;
                try {
                    // The cached amount can be stale (e.g. the user withdrew on another
                    // device), so re-read on-chain before confirming what they'll withdraw.
                    _blockchainManager.InvalidateBridgeRead(wallet.BridgeDepositChain, wallet.BridgeSymbol);
                    _blockchainManager.InvalidateBridgeRead(wallet.BridgeWithdrawChain, wallet.BridgeSymbol);
                    withdrawable = await TryReadBridgeWithdrawable(wallet);
                    feePercent = await GetBridgeFeePercentAsync();
                } finally {
                    waiting.Hide();
                }
                if (!this) return;

                if (!withdrawable.HasValue) {
                    DialogOK.ShowInfo(DialogCanvas, "Info", "Cannot read balance right now");
                    return;
                }
                wallet.ClaimValue = withdrawable.Value;
                ApplyBridgeWithdrawable(wallet);
                if (wallet.ClaimValue <= 0) {
                    DialogOK.ShowInfo(DialogCanvas, "Info", "Nothing to withdraw");
                    return;
                }

                var net = wallet.ClaimValue * (100 - feePercent) / 100;
                var networkName = withdrawChain == DialogBridgeAmount.ChainBsc ? "BNB Chain" : "Polygon";
                var desc = $"Withdraw {wallet.ClaimValue:0.####} {symbol}\n" +
                           $"Network: {networkName}\n" +
                           $"You receive ~ {net:0.####} (fee {feePercent}%)";
                var confirm = await DialogConfirm.Create();
                if (!this) return;
                confirm.SetInfo(desc, "Confirm", "Cancel",
                        () => ExecuteBridgeWithdraw(wallet, serverType, symbol, withdrawChain), null)
                    .Show(DialogCanvas);
            });
        }

        private void ApplyBridgeWithdrawable(DataWallet wallet) {
            if (_bridgeRows == null) return;
            var idx = _bridgeRows.FindIndex(r => r.BridgeSymbol == wallet.BridgeSymbol
                && r.BridgeDepositChain == wallet.BridgeDepositChain
                && r.BridgeWithdrawChain == wallet.BridgeWithdrawChain);
            if (idx < 0) return;
            var d = _bridgeRows[idx];
            d.ClaimValue = wallet.ClaimValue;
            d.IsEnableWithdraw = d.ClaimValue > 0;
            d.BridgeBalanceKnown = true;
            _bridgeRows[idx] = d;
            ApplyBridgeTabs();
        }

        private void ExecuteBridgeWithdraw(DataWallet wallet, int serverType, string symbol, string withdrawChain) {
            if (_bridgeWithdrawInFlight) return;
            _bridgeWithdrawInFlight = true;
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                try {
                    var enabled = await _blockchainManager.GetBridgeWithdrawEnabled(withdrawChain);
                    if (!this) return;
                    if (!enabled) {
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Withdraw is currently disabled");
                        return;
                    }
                    var res = await _serverManager.General.RequestCrosschainBridgeWithdraw(serverType, withdrawChain);
                    if (!this) return;
                    if (res.Code != 0 || string.IsNullOrEmpty(res.Signature)) {
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Cannot withdraw right now");
                        return;
                    }
                    var tx = await _blockchainManager.BridgeWithdraw(withdrawChain, symbol, res.OtherDeposited,
                        res.Deadline, res.Signature);
                    if (!this) return;
                    if (tx.success) {
                        FireBridgeNotify(BridgeNotifyKind.WithdrawDone, serverType, withdrawChain, tx.txHash);
                        var reward = await DialogBCoinReward.Create();
                        if (!this) return;
                        reward.SetReward(new TokenData { displayName = symbol, icon = wallet.BridgeIcon }, tx.net, DialogCanvas)
                            .Show(DialogCanvas);
                    } else {
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Withdraw Failed");
                    }
                } catch (Exception e) {
                    if (this) DialogOK.ShowError(DialogCanvas, e);
                } finally {
                    _bridgeWithdrawInFlight = false;
                    if (this) RefreshBridgeRow(wallet);
                    waiting.Hide();
                }
            });
        }

        private async UniTask<double> GetBridgeFeePercentAsync() {
            if (_bridgeFeeLoaded) return _bridgeFeePercent;
            try {
                var config = await _serverManager.General.GetTreasureHuntDataConfig();
                if (this && config != null) {
                    _bridgeFeePercent = config.BridgeFeePercent;
                    _bridgeFeeLoaded = true;
                }
            } catch (Exception e) {
                if (this) _logManager.Log($"[Bridge] fee config load failed, using default {_bridgeFeePercent}%: {e.Message}");
            }
            return _bridgeFeePercent;
        }

        private static (int serverType, string symbol) BridgeIds(BlockRewardType type) {
            return type == BlockRewardType.SenBridge ? (30, "SEN") : (29, "BCOIN");
        }

        private void ProcessLeftoverPendingHeroFlow(DataWallet wallet) {
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                try {
                    var pending = await _blockchainManager.GetPendingHero();
                    if (!this) return;

                    if (pending.pendingHeroes <= 0) {
                        waiting.Hide();
                        ClaimFlow(wallet);
                        return;
                    }

                    _logManager.Log(
                        $"[Leftover-mint] pendingHeroes={pending.pendingHeroes} " +
                        $"pendingHeroesFusion={pending.pendingHeroesFusion} " +
                        $"code={wallet.RefTokenData.code}");

                    var detail = await _blockchainManager.ProcessTokenRequests();
                    if (!this) return;
                    await _serverManager.General.SyncHero(notifyNewIds: true, forceFresh: true);
                    if (!this) return;
                    if (!detail.result) {
                        throw new Exception("Try again");
                    }

                    if (detail.fusionFailAmount > 0) {
                        await ProcessTokenHelper.FusionFailed(DialogCanvas, detail.fusionFailAmount);
                        if (!this) return;
                    }

                    await RefreshClaimableHeroAmount();
                    await _serverManager.General.GetChestReward();
                    if (!this) return;
                    
                } catch (Exception e) {
                    _logManager.Log($"[Leftover-mint] failed: {e}");
                    if (this) DialogOK.ShowError(DialogCanvas, e);
                } finally {
                    if (this) UpdateAllTokensUI();
                    waiting.Hide();
                }
            });
        }

        private void OnSelectionChanged(DataWallet wallet) {
            _currentSelectedWallet = wallet;
            ApplyClaimButtonState();
        }

        private void ApplyClaimButtonState() {
            if (!_currentSelectedWallet.HasValue) return;
            var phase = _claimTokenManager.Phase;

            ClaimButtonMode mode;
            bool interactable;
            switch (phase) {
                case ClaimPhase.None:
                    mode = ClaimButtonMode.Withdraw;
                    interactable = true;
                    break;
                case ClaimPhase.Waiting:
                case ClaimPhase.Submitting:
                default:
                    mode = ClaimButtonMode.Waiting;
                    interactable = false;
                    break;
            }
            frameWallet.SetClaimButton(mode, interactable);
        }

        #endregion

        #region CLAIM OTHER TOKENS

        private void ClaimOtherToken(DataWallet dataWallet) {
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                try {
                    var (tokenData, result) = await _controller.ClaimOtherCoin(dataWallet.RefRewardType);
                    var dialog = await DialogBCoinReward.Create();
                    dialog.SetReward(tokenData, result.ClaimedValue, DialogCanvas).Show(DialogCanvas);
                } catch (Exception ex) {
                    DialogOK.ShowError(DialogCanvas, ex);
                }
                UpdateAllTokensUI();
                waiting.Hide();
            });
        }

        #endregion

        private async UniTask<DialogWaiting> ShowDialogWaiting() {
            var dialog = await DialogWaiting.Create();
            dialog.Show(DialogCanvas);
            if (!AppConfig.IsSolana()) {
                dialog.ShowLoadingAnim();
            }
            return dialog;
        }

        private void OnChestReward(IChestReward reward) {
            InstantiateTokens(reward);
        }

        public void OnCloseBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        private void RefreshUi() {
            // Apply data deposit
            var depositList = (from item in _walletDataFull
                where item.RefTokenData != null && item.RefTokenData.enableDeposit
                orderby item.RefTokenData.sortOrder
                select item).ToList();
            frameWallet.ApplyUiTab(TypeMenuLeftWallet.Deposit, depositList, null);

            // Apply data withdraw
            var withdrawList = (from item in _walletDataFull
                where item.RefTokenData != null && item.RefTokenData.enableClaim
                orderby item.RefTokenData.sortOrder
                select item).ToList();
            frameWallet.ApplyUiTab(TypeMenuLeftWallet.Withdraw, withdrawList, null);
            // Apply data token & nft list
            var tokensList = new List<DataWallet>();
            var nftList = new List<DataWallet>();
            foreach (var item in _walletDataFull) {
                if (depositList.Contains(item)) {
                    continue;
                }
                if (item.RefTokenData != null && item.RefTokenData.tokenName == BlockRewardType.Hero) {
                    nftList.Add(item);
                } else {
                    tokensList.Add(item);
                }
            }
            tokensList.Sort((a, b) => a.RefTokenData.sortOrder.CompareTo(b.RefTokenData.sortOrder));
            frameWallet.ApplyUiTab(TypeMenuLeftWallet.Mine, tokensList, nftList);
            frameWallet.SetOnDeposit(OnDeposit);
            frameWallet.SetOnWithdraw(OnWithdraw);
            if (SupportsBridge) {
                AppendBridgeRows(depositList, withdrawList);
            }
        }

        private static bool SupportsBridge =>
            !AppConfig.IsTon() && !AppConfig.IsSolana() && !AppConfig.IsRonin()
            && !AppConfig.IsBase() && !AppConfig.IsViction() && !AppConfig.IsAirDrop();

        private void AppendBridgeRows(List<DataWallet> depositBase, List<DataWallet> withdrawBase) {
            var specs = new[] {
                ("BCOIN", BlockRewardType.BcoinBridge, bcoinBridgeIcon, DialogBridgeAmount.ChainBsc, DialogBridgeAmount.ChainPolygon),
                ("BCOIN", BlockRewardType.BcoinBridge, bcoinBridgeIcon, DialogBridgeAmount.ChainPolygon, DialogBridgeAmount.ChainBsc),
                ("SEN", BlockRewardType.SenBridge, senBridgeIcon, DialogBridgeAmount.ChainBsc, DialogBridgeAmount.ChainPolygon),
                ("SEN", BlockRewardType.SenBridge, senBridgeIcon, DialogBridgeAmount.ChainPolygon, DialogBridgeAmount.ChainBsc),
            };
            var bridgeRows = new List<DataWallet>();
            foreach (var (symbol, type, icon, depositChain, withdrawChain) in specs) {
                // Bridge-specific info still lives in InformationTable as BCOIN_BRIDGE /
                // SEN_BRIDGE — look it up by the bridge reward type. It's a single entry
                // per token, so its network label is irrelevant to the match.
                var info = _informationManager.GetTokenData(
                    new TokenReward(RewardUtils.ConvertToBlockRewardType(type), depositChain));
                bridgeRows.Add(new DataWallet {
                    IsBridge = true,
                    BridgeBalanceKnown = false,
                    BridgeSymbol = symbol,
                    BridgeIcon = icon,
                    BridgeDepositChain = depositChain,
                    BridgeWithdrawChain = withdrawChain,
                    RefRewardType = _launchPadManager.CreateRewardType(type),
                    RefInfo = info,
                    IsEnableDeposit = true,
                    IsEnableWithdraw = false,
                });
            }
            _bridgeRows = bridgeRows;
            _bridgeDepositBase = depositBase;
            _bridgeWithdrawBase = withdrawBase;
            ApplyBridgeTabs();

            UniTask.Void(async () => {
                for (var i = 0; i < bridgeRows.Count; i++) {
                    var d = bridgeRows[i];
                    var withdrawable = await TryReadBridgeWithdrawable(d);
                    if (!this) return;
                    if (withdrawable.HasValue) {
                        d.ClaimValue = withdrawable.Value;
                        d.IsEnableWithdraw = d.ClaimValue > 0;
                        d.BridgeBalanceKnown = true;
                        bridgeRows[i] = d;
                    }
                }
                if (!this) return;
                ApplyBridgeTabs();
            });
        }

        private async UniTask<float?> TryReadBridgeWithdrawable(DataWallet row) {
            try {
                var deposited = await _blockchainManager.GetBridgeDeposited(row.BridgeDepositChain, row.BridgeSymbol);
                var withdrawn = await _blockchainManager.GetBridgeWithdrawn(row.BridgeWithdrawChain, row.BridgeSymbol);
                return Withdrawable(deposited, withdrawn);
            } catch (Exception e) {
                if (this) _logManager.Log($"[Bridge] read failed {row.BridgeSymbol} {row.BridgeDepositChain}->{row.BridgeWithdrawChain}: {e.Message}");
                return null;
            }
        }

        private void RefreshBridgeRow(DataWallet wallet) {
            if (_bridgeRows == null) return;
            var idx = _bridgeRows.FindIndex(r => r.BridgeSymbol == wallet.BridgeSymbol
                && r.BridgeDepositChain == wallet.BridgeDepositChain
                && r.BridgeWithdrawChain == wallet.BridgeWithdrawChain);
            if (idx < 0) return;
            UniTask.Void(async () => {
                var withdrawable = await TryReadBridgeWithdrawable(_bridgeRows[idx]);
                if (!this) return;
                var d = _bridgeRows[idx];
                if (withdrawable.HasValue) {
                    d.ClaimValue = withdrawable.Value;
                    d.IsEnableWithdraw = d.ClaimValue > 0;
                    d.BridgeBalanceKnown = true;
                }
                _bridgeRows[idx] = d;
                ApplyBridgeTabs();
            });
        }

        private void ApplyBridgeTabs() {
            var deposit = new List<DataWallet>(_bridgeDepositBase);
            deposit.AddRange(_bridgeRows);
            frameWallet.ApplyUiTab(TypeMenuLeftWallet.Deposit, deposit, null);
            var withdraw = new List<DataWallet>(_bridgeWithdrawBase);
            withdraw.AddRange(_bridgeRows);
            frameWallet.ApplyUiTab(TypeMenuLeftWallet.Withdraw, withdraw, null);
        }

        private static float Withdrawable(string depositedWei, string withdrawnWei) {
            var dep = BigInteger.Parse(string.IsNullOrEmpty(depositedWei) ? "0" : depositedWei);
            var wd = BigInteger.Parse(string.IsNullOrEmpty(withdrawnWei) ? "0" : withdrawnWei);
            var diff = dep - wd;
            if (diff < 0) {
                diff = 0;
            }
            return (float)((double)diff / 1e18);
        }

        private void OnDeposit(DataWallet dataWallet) {
            try {
                _soundManager.PlaySound(Audio.Tap);
                if (dataWallet.IsBridge) {
                    OpenBridgeDepositDialog(dataWallet);
                    return;
                }
                _controller.ThrowIfCannotDeposit(dataWallet);
                var t = dataWallet.RefRewardType.Type;
                if (AppConfig.IsAirDrop()) {
                    UniTask.Void(async () => {
                        var dialogAirdrop = await DialogDepositAirdrop.Create(t);
                        dialogAirdrop.Show(DialogCanvas);
                    });
                } else if (t is BlockRewardType.BcoinBridge or BlockRewardType.SenBridge) {
                    OpenBridgeDepositDialog(dataWallet);
                } else if (t is BlockRewardType.BnbDeposited or BlockRewardType.PolDeposited) {
                    OpenNativeDepositDialog(dataWallet);
                } else {
                    DialogDeposit.Create().ContinueWith(dialog => {
                        dialog.Init(dataWallet.RefTokenData).Show(DialogCanvas);
                    });
                }
            } catch (Exception e) {
                DialogOK.ShowError(DialogCanvas, e);
            }
        }

        private void OpenBridgeDepositDialog(DataWallet wallet) {
            var (_, symbol) = BridgeIds(wallet.RefRewardType.Type);
            var depositChain = wallet.BridgeDepositChain;
            var category = BridgeCategory(symbol, depositChain);
            UniTask.Void(async () => {
                var available = await _blockchainManager.GetBalance(category);
                if (!this) return;
                var networkName = depositChain == DialogBridgeAmount.ChainBsc ? "BNB Chain" : "Polygon";
                var dialog = await DialogBridgeAmount.Create();
                if (!this) return;
                dialog.Show(DialogBridgeAmount.Mode.Deposit, symbol, networkName, available,
                    0, depositChain, DialogCanvas,
                    (amount, _, _) => ExecuteBridgeDeposit(wallet, symbol, category, depositChain, amount));
            });
        }

        private void ExecuteBridgeDeposit(DataWallet wallet, string symbol, RpcTokenCategory category,
            string depositChain, double amount) {
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                try {
                    var enabled = await _blockchainManager.GetBridgeDepositEnabled(depositChain);
                    if (!this) return;
                    if (!enabled) {
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Deposit is currently disabled");
                        return;
                    }
                    var nano = new BigInteger(Math.Floor(amount * 1e9));
                    var amountWei = (nano * BigInteger.Pow(10, 9)).ToString();
                    var (serverType, _) = BridgeIds(wallet.RefRewardType.Type);
                    FireBridgeNotify(BridgeNotifyKind.DepositPrepare, serverType, depositChain);
                    var res = await _blockchainManager.BridgeDeposit(depositChain, symbol, amountWei);
                    if (!this) return;
                    if (res.success) {
                        FireBridgeNotify(BridgeNotifyKind.DepositDone, serverType, depositChain, res.txHash);
                        await App.Utils.WaitForBalanceChange(category, _blockchainManager, _blockchainStorageManager);
                        if (!this) return;
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Deposit Successfully");
                    } else {
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Deposit Failed");
                    }
                } catch (Exception e) {
                    if (this) DialogOK.ShowError(DialogCanvas, e);
                } finally {
                    if (this) RefreshBridgeRow(wallet);
                    waiting.Hide();
                }
            });
        }

        // Best-effort bridge activity report — fire-and-forget so it never blocks the on-chain flow. A dropped
        // notify just means the indexer's idle sweep picks up the activity a little later.
        private void FireBridgeNotify(string kind, int serverType, string chain, string txHash = null) {
            UniTask.Void(async () => {
                try {
                    await _serverManager.General.NotifyCrosschainBridge(kind, serverType, chain, txHash);
                } catch (Exception e) {
                    if (this) _logManager.Log($"[Bridge-notify] {kind} failed: {e.Message}");
                }
            });
        }

        private static RpcTokenCategory BridgeCategory(string symbol, string chain) {
            var isBsc = chain == DialogBridgeAmount.ChainBsc;
            if (symbol == "SEN") {
                return isBsc ? RpcTokenCategory.SenBsc : RpcTokenCategory.SenPolygon;
            }
            return isBsc ? RpcTokenCategory.Bcoin : RpcTokenCategory.Bomb;
        }

        // ── Native coin (BNB / POL) deposit + withdraw ─────────────────────────────
        // Server block-reward-type codes: BNB_DEPOSITED = 31 (BSC), POL_DEPOSITED = 32 (POLYGON).
        // The reward type is chain-specific, so it also picks the network. Deposit is payable (an
        // amount dialog); withdraw is withdraw-MAX (no amount) with 0% fee, mirroring BCOIN_DEPOSITED.
        private static (int serverType, string chain, string symbol) NativeIds(BlockRewardType type) {
            return type == BlockRewardType.PolDeposited
                ? (32, DialogBridgeAmount.ChainPolygon, "POL")
                : (31, DialogBridgeAmount.ChainBsc, "BNB");
        }

        private void OpenNativeDepositDialog(DataWallet wallet) {
            var (_, chain, symbol) = NativeIds(wallet.RefRewardType.Type);
            var networkName = chain == DialogBridgeAmount.ChainBsc ? "BNB Chain" : "Polygon";
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                double available;
                try {
                    available = await _blockchainManager.GetNativeWalletBalance(chain);
                } catch (Exception e) {
                    if (this) _logManager.Log($"[Native] wallet balance read failed {chain}: {e.Message}");
                    available = 0;
                } finally {
                    waiting.Hide();
                }
                if (!this) return;
                var dialog = await DialogNativeAmount.Create();
                if (!this) return;
                dialog.Show(symbol, networkName, available, wallet.RefTokenData?.icon, DialogCanvas,
                    amount => ExecuteNativeDeposit(wallet, chain, amount));
            });
        }

        private void ExecuteNativeDeposit(DataWallet wallet, string chain, double amount) {
            if (_nativeInFlight) return;
            _nativeInFlight = true;
            var (serverType, _, _) = NativeIds(wallet.RefRewardType.Type);
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                try {
                    var enabled = await _blockchainManager.GetNativeDepositEnabled(chain);
                    if (!this) return;
                    if (!enabled) {
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Deposit is currently disabled");
                        return;
                    }
                    // 9+9 decimal split keeps the 18-decimal wei exact without a double overflow.
                    var nano = new BigInteger(Math.Floor(amount * 1e9));
                    var amountWei = (nano * BigInteger.Pow(10, 9)).ToString();
                    var res = await _blockchainManager.NativeDeposit(chain, amountWei);
                    if (!this) return;
                    if (res.success) {
                        // Fire the sync hint + pull fresh rewards. Deposit crediting is at confirmed depth,
                        // so the balance may lag briefly (login + reconciler also converge) — accepted.
                        await _serverManager.General.SyncNativeDeposit(serverType);
                        if (!this) return;
                        await _serverManager.General.GetChestReward();
                        if (!this) return;
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Deposit Successfully");
                    } else {
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Deposit Failed");
                    }
                } catch (Exception e) {
                    if (this) DialogOK.ShowError(DialogCanvas, e);
                } finally {
                    _nativeInFlight = false;
                    waiting.Hide();
                }
            });
        }

        private void NativeWithdrawFlow(DataWallet wallet) {
            if (_nativeInFlight) return;
            var (_, chain, symbol) = NativeIds(wallet.RefRewardType.Type);
            if (wallet.ClaimValue <= 0) {
                DialogOK.ShowInfo(DialogCanvas, "Info", "Nothing to withdraw");
                return;
            }
            var networkName = chain == DialogBridgeAmount.ChainBsc ? "BNB Chain" : "Polygon";
            var desc = $"Withdraw {wallet.ClaimValue:0.#########} {symbol}\n" +
                       $"Network: {networkName}\n" +
                       "You receive the full amount (no fee)";
            UniTask.Void(async () => {
                var confirm = await DialogConfirm.Create();
                if (!this) return;
                confirm.SetInfo(desc, "Confirm", "Cancel",
                        () => ExecuteNativeWithdraw(wallet, chain), null)
                    .Show(DialogCanvas);
            });
        }

        private void ExecuteNativeWithdraw(DataWallet wallet, string chain) {
            if (_nativeInFlight) return;
            _nativeInFlight = true;
            var (serverType, _, symbol) = NativeIds(wallet.RefRewardType.Type);
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                try {
                    var enabled = await _blockchainManager.GetNativeWithdrawEnabled(chain);
                    if (!this) return;
                    if (!enabled) {
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Withdraw is currently disabled");
                        return;
                    }
                    var res = await _serverManager.General.RequestNativeWithdraw(serverType);
                    if (!this) return;
                    if (res.Code != 0 || string.IsNullOrEmpty(res.Signature)) {
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Cannot withdraw right now");
                        return;
                    }
                    // allowed_cumulative is relayed verbatim (exact wei string) — never parsed to a float.
                    var tx = await _blockchainManager.NativeWithdraw(chain, res.AllowedCumulative, res.Deadline, res.Signature);
                    if (!this) return;
                    if (tx.success) {
                        await _serverManager.General.SyncNativeDeposit(serverType);
                        if (!this) return;
                        await _serverManager.General.GetChestReward();
                        if (!this) return;
                        var reward = await DialogBCoinReward.Create();
                        if (!this) return;
                        reward.SetReward(new TokenData { displayName = symbol, icon = wallet.RefTokenData?.icon }, tx.net, DialogCanvas)
                            .Show(DialogCanvas);
                    } else {
                        DialogOK.ShowInfo(DialogCanvas, "Info", "Withdraw Failed");
                    }
                } catch (Exception e) {
                    if (this) DialogOK.ShowError(DialogCanvas, e);
                } finally {
                    _nativeInFlight = false;
                    waiting.Hide();
                }
            });
        }

        private void OnWithdraw(DataWallet dataWallet) {
            if (_claimTokenManager.Phase == ClaimPhase.None
                && dataWallet.RefRewardType.Type == BlockRewardType.Hero
                && !AppConfig.IsAirDrop()
                && !AppConfig.IsSolana()) {
                ProcessLeftoverPendingHeroFlow(dataWallet);
                return;
            }

            var t = dataWallet.RefRewardType.Type;
            switch (t) {
                case BlockRewardType.Hero:
                    if (AppConfig.IsAirDrop()) {
                        ClaimHeroAirdrop((int)dataWallet.ClaimValue + (int)dataWallet.PendingValue);
                    } else {
                        ClaimFlow(dataWallet);
                    }
                    return;
                case BlockRewardType.BCoin:
                case BlockRewardType.Senspark:
                case BlockRewardType.BCoinDeposited:
                case BlockRewardType.SensparkDeposited:
                    ClaimFlow(dataWallet);
                    return;
                case BlockRewardType.BcoinBridge:
                case BlockRewardType.SenBridge:
                    BridgeWithdrawFlow(dataWallet);
                    return;
                case BlockRewardType.BnbDeposited:
                case BlockRewardType.PolDeposited:
                    NativeWithdrawFlow(dataWallet);
                    return;
                default:
                    ClaimOtherToken(dataWallet);
                    return;
            }
        }

        private void ClaimHeroAirdrop(int amount) {
            _soundManager.PlaySound(Audio.Tap);
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    if (AppConfig.IsSolana()) {
                        await _userSolanaManager.BuyHeroSol(amount, (int)Constant.RewardType.BHero);
                    } else {
                        await _serverManager.General.BuyHeroServer(amount, (int)Constant.RewardType.BHero);
                    }
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e);
                    } else {
                        DialogOK.ShowError(DialogCanvas, e);
                    }
                } finally {
                    waiting.End();
                }
            });
        }

        public async void OnTonDeposit() {
            var currentRewardType = frameWallet.GetCurrentSegment().RewardType;
            _soundManager.PlaySound(Audio.Tap);
            var dialog = await DialogDepositAirdrop.Create(currentRewardType.Type);
            dialog.Show(DialogCanvas);
        }

        public async void OnSolDeposit() {
            //FIXME: Tạm thời không lấy _currentSlotSelect bên BLFrameWallet truyền qua cho DialogDepositTon
            // Sau nay nếu có nhiều hơn 2 items deposit thì cập nhận lại.
            _soundManager.PlaySound(Audio.Tap);
            // var dialog = await DialogDepositSol.Create();
            // dialog.Show(DialogCanvas);
        }

        private void OnClaim(BLWalletSegmentItem item) {
            switch (item.RewardType.Type) {
                case BlockRewardType.Hero:
                    ClaimHero((int)item.Balance);
                    break;
                case BlockRewardType.TonDeposited:
                    ClaimTon();
                    break;
            }
        }

        private void ClaimTon() {
            DialogOK.ShowInfo(DialogCanvas, _languageManager.GetValue(LocalizeKey.ui_coming_soon));
        }

        private void ClaimHero(int amount) {
            _soundManager.PlaySound(Audio.Tap);
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await _serverManager.General.BuyHeroServer(amount, (int)Constant.RewardType.BHero);
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e);
                    } else {
                        DialogOK.ShowError(DialogCanvas, e);
                    }
                } finally {
                    waiting.End();
                }
            });
        }
    }
}