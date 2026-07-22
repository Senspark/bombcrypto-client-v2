using System;
using System.Threading.Tasks;

using App;

using Services.Rewards;

namespace Scenes.FarmingScene.Scripts {
    public class BLDialogRewardController {
        private readonly IServerManager _serverManager;
        private readonly ILaunchPadManager _launchPadManager;
        private readonly IClaimTokenManager _claimTokenManager;
        private readonly IChestRewardManager _chestRewardManager;
        private readonly IBlockchainManager _blockchainManager;
        private readonly IBlockchainStorageManager _blockchainStorageManager;
        private readonly IStorageManager _storageManager;
        private readonly IFeatureManager _featureManager;
        private readonly NetworkType _networkType;
        private int _blockchainClaimableHero;

        public BLDialogRewardController(
            IServerManager serverManager,
            ILaunchPadManager launchPadManager,
            IClaimTokenManager claimTokenManager,
            IChestRewardManager chestRewardManager,
            IBlockchainManager blockchainManager,
            IBlockchainStorageManager blockchainStorageManager,
            IStorageManager storageManager,
            IFeatureManager featureManager,
            NetworkType networkType
        ) {
            _serverManager = serverManager;
            _launchPadManager = launchPadManager;
            _claimTokenManager = claimTokenManager;
            _chestRewardManager = chestRewardManager;
            _blockchainManager = blockchainManager;
            _blockchainStorageManager = blockchainStorageManager;
            _storageManager = storageManager;
            _featureManager = featureManager;
            _networkType = networkType;
        }

        public static bool IsBridge(BlockRewardType type) {
            return type is BlockRewardType.BcoinBridge or BlockRewardType.SenBridge;
        }

        private bool IsEvmLogin() {
            return _networkType is NetworkType.Binance or NetworkType.Polygon;
        }

        private DataType CurrentNetwork => RewardUtils.ConvertNetworkToDatatype(_networkType);

        public bool CanDeposit(TokenData tokenData) {
            if (AppConfig.IsSolana())
                return true;
            var featureEnable = _featureManager.EnableDeposit;
            var tokenEnable = tokenData.enableDeposit;
            var correctNetwork = IsBridge(tokenData.tokenName) ? IsEvmLogin() : tokenData.networkSymbol == CurrentNetwork;
            return featureEnable && tokenEnable && correctNetwork;
        }

        public bool CanWidthDraw(TokenData tokenData, IRewardType type) {
            var currentReward = GetClaimableValue(tokenData, type);
            var featureEnable = _featureManager.EnableClaim;
            var tokenEnable = tokenData.enableClaim;
            var correctNetwork = IsBridge(tokenData.tokenName) ? IsEvmLogin() : tokenData.networkSymbol == CurrentNetwork;
            var canClaim = _launchPadManager.CanClaim(type, tokenData.networkSymbol, currentReward);
            return featureEnable && tokenEnable && correctNetwork && canClaim;
        }

        public void ThrowIfCannotDeposit(DataWallet data) {
            if (!_featureManager.EnableDeposit) {
                throw new Exception("Not support");
            }
            var tokenData = data.RefTokenData;
            var correctNetwork = IsBridge(tokenData.tokenName) ? IsEvmLogin() : tokenData.networkSymbol == CurrentNetwork;
            if (!correctNetwork || !tokenData.enableDeposit) {
                throw new Exception("Not allow to deposit");
            }
        }
        
        public void ThrowIfCannotClaim(DataWallet data) {
            if (!_featureManager.EnableClaim) {
                throw new Exception("Not support");
            }
            var tokenData = data.RefTokenData;
            var rewardType = data.RefRewardType;
            if (tokenData.networkSymbol != CurrentNetwork || !tokenData.enableClaim) {
                throw new Exception("Not allow to claim");
            }
            var currentReward = GetClaimableValue(tokenData, rewardType);
            var canClaim = _launchPadManager.CanClaim(rewardType, tokenData.networkSymbol, currentReward);
            if (!canClaim) {
                throw new Exception("Cannot claim");
            }
        }

        public async Task<ClaimCoinResult> WaitForClaimCompletion(DataWallet data, float claimed) {
            if (claimed <= 0) {
                throw new Exception("Claim Failed");
            }
            var (balanceChanged, newBalance) = await WaitForBalanceChanged(data.RefRewardType);
            return new ClaimCoinResult {
                Successful = balanceChanged,
                ClaimValue = claimed,
                NewBalance = newBalance,
            };
        }

        public Task<(TokenData, IApproveClaimResponse)> ClaimOtherCoin(IRewardType type) {
            throw new NotImplementedException();
            // var tokenData = _launchPadManager.GetCurrentNetworkData(type.Type);
            // var result = await _serverManager.ApproveClaim(tokenData.code);
            // return (tokenData, result);
        }

        public async Task ChangeMiningToken(IRewardType type) {
            var walletBcoin = _blockchainStorageManager.GetBalance(BlockRewardType.BCoin);
            await _serverManager.General.ChangeMiningToken(type.Name, walletBcoin);
        }

        public async Task<BlockchainHeroAmount> GetHeroOnBlockchain(NetworkType type) {
            if (type != _networkType) {
                return new BlockchainHeroAmount();
            }
            // On-chain reads go through the wallet RPC; a dead/flaky RPC must not
            // block the reward dialog. Fall back to zero amounts so the UI still opens.
            try {
                // Independent reads — fire all three concurrently instead of serially.
                var claimableTask = _blockchainManager.GetClaimableHero();
                var giveAwayTask = _blockchainManager.GetGiveAwayHero();
                var pendingTask = _blockchainManager.GetPendingHero();
                await Task.WhenAll(claimableTask, giveAwayTask, pendingTask);
                var result = new BlockchainHeroAmount {
                    ClaimableHero = claimableTask.Result,
                    GiveAwayHero = giveAwayTask.Result,
                    PendingHero = pendingTask.Result.pendingHeroes
                };
                _blockchainClaimableHero = result.GetTotal();
                return result;
            } catch (Exception e) {
                UnityEngine.Debug.LogWarning($"GetHeroOnBlockchain({type}) failed, defaulting to 0: {e.Message}");
                var result = new BlockchainHeroAmount();
                _blockchainClaimableHero = result.GetTotal();
                return result;
            }
        }

        private async Task<(bool, double)> WaitForBalanceChanged(IRewardType rewardType) {
            var type = rewardType.Type;
            var t = (type, _networkType) switch {
                (BlockRewardType.BCoin, NetworkType.Binance) => RpcTokenCategory.Bcoin,
                (BlockRewardType.BCoinDeposited, NetworkType.Binance) => RpcTokenCategory.Bcoin,
                (BlockRewardType.BCoin, NetworkType.Polygon) => RpcTokenCategory.Bomb,
                (BlockRewardType.BCoinDeposited, NetworkType.Polygon) => RpcTokenCategory.Bomb,
                (BlockRewardType.Senspark, NetworkType.Binance) => RpcTokenCategory.SenBsc,
                (BlockRewardType.Senspark, NetworkType.Polygon) => RpcTokenCategory.SenPolygon,
                (BlockRewardType.SensparkDeposited, NetworkType.Binance) => RpcTokenCategory.SenBsc,
                (BlockRewardType.SensparkDeposited, NetworkType.Polygon) => RpcTokenCategory.SenPolygon,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
            return await App.Utils.WaitForBalanceChange(t, _blockchainManager, _blockchainStorageManager);
        }
        
        private float GetClaimableValue(TokenData tokenData, IRewardType type) {
            var currentReward = _chestRewardManager.GetChestRewardByNetwork(type, tokenData.networkSymbol) +
                                _chestRewardManager.GetClaimPendingRewardByNetwork(type, tokenData.networkSymbol);

            if (type.Type == BlockRewardType.Hero) {
                currentReward += _blockchainClaimableHero;
            }
            return currentReward;
        }

        public class ClaimCoinResult {
            public bool Successful;
            public double ClaimValue;
            public double NewBalance;
        }

        public class BlockchainHeroAmount {
            public int ClaimableHero;
            public int GiveAwayHero;
            public int PendingHero;

            public int GetTotal() {
                return ClaimableHero + GiveAwayHero + PendingHero;
            }
        }
    }
}