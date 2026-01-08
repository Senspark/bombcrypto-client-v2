using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App;
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
    }

    public class BLDialogReward : Dialog {
        [SerializeField]
        private WaitingPanel waitingPanel;

        [SerializeField]
        private BLFrameWallet frameWallet;

        [SerializeField]
        private BLFrameWallet[] frameWallets;

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
                    await frameWallet.InitUi();
                    await RefreshClaimableHeroAmount();
                    await _serverManager.General.GetChestReward();
                    waitingPanel.gameObject.SetActive(false);
                } catch (Exception e) when (!token.IsCancellationRequested) {
                    _logManager.Log(e.Message);
                    DialogOK.ShowError(DialogCanvas, e.Message);
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
                    if (t.Network != NetworkSymbol.Sol.Name
                        && rewardType.Type != BlockRewardType.BLCoin) {
                        continue;
                    }
                } 
                // RON ,VIC, BAS chỉ hiện starcore network, ko hiện star core TR
                else if (AppConfig.IsRonin()) {
                    if (t.Network != NetworkSymbol.Ron.Name) {
                        continue;
                    }
                } else if (AppConfig.IsBase()) {
                    if (t.Network != NetworkSymbol.Bas.Name) {
                        continue;
                    }
                } else if (AppConfig.IsViction()) {
                    if (t.Network != NetworkSymbol.Vic.Name) {
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
                    if(rewardType.Type == BlockRewardType.BLCoin && t.Network != NetworkSymbol.TR.Name ) {
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
                    DataWallet d;
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
                        var n = RewardUtils.ConvertToNetworkType(data.NetworkSymbol.Name);
                        claimValue += _heroAmount[n].GetTotal();
                    }
                    DataWallet d;
                    d.RefTokenData = data;
                    d.RefTokenReward = new TokenReward(data.tokenName, data.NetworkSymbol.Name);
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

        #region CLAIM BCOIN

        private void ClaimCoin(DataWallet dataWallet) {
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                try {
                    var res = await _controller.ClaimCoin(dataWallet);
                    if (res.Successful) {
                        var dialog = await DialogBCoinReward.Create();
                        dialog.SetReward(dataWallet.RefTokenData, res.ClaimValue).Show(DialogCanvas);
                    } else {
                        DialogOK.ShowInfo(DialogCanvas, "Claim Fail");
                    }
                } catch (Exception e) {
                    DialogOK.ShowError(DialogCanvas, e.Message);
                } finally {
                    UpdateAllTokensUI();
                    waiting.Hide();
                }
            });
        }

        private void ClaimDeposit(DataWallet dataWallet) {
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                try {
                    var res = await _controller.ClaimCoin(dataWallet);
                    if (res.Successful) {
                        var dialog = await DialogBCoinReward.Create();
                        dialog.SetReward(dataWallet.RefTokenData, res.ClaimValue).Show(DialogCanvas);
                    } else {
                        DialogOK.ShowInfo(DialogCanvas, "Claim Fail");
                    }
                } catch (Exception e) {
                    DialogOK.ShowError(DialogCanvas, e.Message);
                } finally {
                    UpdateAllTokensUI();
                    waiting.Hide();
                }
            });
        }

        #endregion

        #region CLAIM HERO

        private void ClaimBHero(DataWallet dataWallet) {
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                try {
                    var res = await _controller.ClaimHero();
                    await RefreshClaimableHeroAmount();
                    await _serverManager.General.GetChestReward();
                    var success = res.Response.Succeed;
                    var error = res.Response.ErrorMessage;
                    var lostAmount = res.LostHeroAmount;

                    var errorMsg = error ?? (!success ? "Claim Failed" : null);
                    if (errorMsg != null) {
                        DialogOK.ShowError(DialogCanvas, error);
                    }

                    if (res.LostHeroAmount > 0) {
                        await ProcessTokenHelper.FusionFailed(DialogCanvas, lostAmount);
                    }
                } catch (Exception e) {
                    DialogOK.ShowError(DialogCanvas, e.Message);
                } finally {
                    UpdateAllTokensUI();
                    waiting.Hide();
                }
            });
        }

        #endregion

        #region CLAIM OTHER TOKENS

        private void ClaimOtherToken(DataWallet dataWallet) {
            UniTask.Void(async () => {
                var waiting = await ShowDialogWaiting();
                try {
                    var (tokenData, result) = await _controller.ClaimOtherCoin(dataWallet.RefRewardType);
                    var dialog = await DialogBCoinReward.Create();
                    dialog.SetReward(tokenData, result.ClaimedValue).Show(DialogCanvas);
                } catch (Exception ex) {
                    DialogOK.ShowError(DialogCanvas, ex.Message);
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
                if (item.RefTokenData != null && item.RefTokenData.tokenName == "BOMBERMAN") {
                    nftList.Add(item);
                } else {
                    tokensList.Add(item);
                }
            }
            tokensList.Sort((a, b) => a.RefTokenData.sortOrder.CompareTo(b.RefTokenData.sortOrder));
            frameWallet.ApplyUiTab(TypeMenuLeftWallet.Mine, tokensList, nftList);
            frameWallet.SetOnDeposit(OnDeposit);
            frameWallet.SetOnWithdraw(OnWithdraw);
        }

        private void OnDeposit(DataWallet dataWallet) {
            try {
                _soundManager.PlaySound(Audio.Tap);
                _controller.ThrowIfCannotDeposit(dataWallet);
                UniTask.Void(async () => {
                    if (AppConfig.IsAirDrop()) {
                        var dialogAirdrop = await DialogDepositAirdrop.Create(dataWallet.RefRewardType.Type);
                        dialogAirdrop.Show(DialogCanvas);
                    } else {
                        DialogDeposit.Create().ContinueWith(dialog => {
                            dialog.Init(dataWallet.RefTokenData).Show(DialogCanvas);
                        });
                    }
           
                });
            } catch (Exception e) {
                DialogOK.ShowError(DialogCanvas, e.Message);
            }
        }

        private void OnWithdraw(DataWallet dataWallet) {
            var t = dataWallet.RefRewardType.Type;
            switch (t) {
                case BlockRewardType.Hero:
                    if (AppConfig.IsAirDrop()) {
                        ClaimHeroAirdrop((int)dataWallet.ClaimValue + (int)dataWallet.PendingValue);
                    } else {
                        ClaimBHero(dataWallet);
                    }
                    return;
                case BlockRewardType.BCoin:
                case BlockRewardType.Senspark:
                    ClaimCoin(dataWallet);
                    return;
                case BlockRewardType.BCoinDeposited:
                case BlockRewardType.SensparkDeposited:
                    ClaimDeposit(dataWallet);
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
                        DialogError.ShowError(DialogCanvas, e.Message);
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
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
                        DialogError.ShowError(DialogCanvas, e.Message);
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
                    }
                } finally {
                    waiting.End();
                }
            });
        }
    }
}