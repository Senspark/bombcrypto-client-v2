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

        public bool CanDeposit(TokenData tokenData) {
            if (AppConfig.IsSolana())
                return true;
            var featureEnable = _featureManager.EnableDeposit;
            var tokenEnable = tokenData.enableDeposit;
            var correctNetwork = tokenData.NetworkSymbol == _networkType;
            return featureEnable && tokenEnable && correctNetwork;
        }

        public bool CanWidthDraw(TokenData tokenData, IRewardType type) {
            var currentReward = GetClaimableValue(tokenData, type);
            var featureEnable = _featureManager.EnableClaim;
            var tokenEnable = tokenData.enableClaim;
            var correctNetwork = tokenData.NetworkSymbol == _networkType;
            var canClaim = _launchPadManager.CanClaim(type, tokenData.NetworkSymbol, currentReward);
            return featureEnable && tokenEnable && correctNetwork && canClaim;
        }

        public void ThrowIfCannotDeposit(DataWallet data) {
            if (!_featureManager.EnableDeposit) {
                throw new Exception("Not support");
            }
            if (data.RefTokenData.NetworkSymbol != _networkType || !data.RefTokenData.enableDeposit) {
                throw new Exception("Not allow to deposit");
            }
        }
        
        public void ThrowIfCannotClaim(DataWallet data) {
            if (!_featureManager.EnableClaim) {
                throw new Exception("Not support");
            }
            var tokenData = data.RefTokenData;
            var rewardType = data.RefRewardType;
            if (tokenData.NetworkSymbol != _networkType || !tokenData.enableClaim) {
                throw new Exception("Not allow to claim");
            }
            var currentReward = GetClaimableValue(tokenData, rewardType);
            var canClaim = _launchPadManager.CanClaim(rewardType, tokenData.NetworkSymbol, currentReward);
            if (!canClaim) {
                throw new Exception("Cannot claim");
            }
        }

        public async Task<ClaimCoinResult> ClaimCoin(DataWallet data) {
            string message = null;
            var claimed = 0d;

            try {
                ThrowIfCannotClaim(data);
                var result = await _claimTokenManager.ClaimToken(data.RefRewardType.Type, data.RefTokenData.code);
                claimed += result;
            } catch (Exception ex) {
                message = ex.Message;
            }

            if (claimed > 0) {
                var (balanceChanged, newBalance) = await WaitForBalanceChanged(data.RefRewardType);
                return new ClaimCoinResult {
                    Successful = balanceChanged,
                    ClaimValue = claimed,
                    NewBalance = newBalance,
                };
            }

            message = !string.IsNullOrWhiteSpace(message) ? message : "Claim Failed";
            throw new Exception(message);
        }

        public async Task<ClaimHeroResult> ClaimHero() {
            var result = await _claimTokenManager.ClaimHero();
            var lostAmount = 0;
            if (result.Succeed) {
                lostAmount = result.ProcessDetails?.fusionFailAmount ?? 0;
                await _serverManager.General.SyncHero(true);
            }
            return new ClaimHeroResult {
                Response = result,
                LostHeroAmount = lostAmount
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
            var claimable = await _blockchainManager.GetClaimableHero();
            var giveAway = await _blockchainManager.GetGiveAwayHero();
            var pending = await _blockchainManager.GetPendingHero();
            var result = new BlockchainHeroAmount {
                ClaimableHero = claimable,
                GiveAwayHero = giveAway,
                PendingHero = pending.pendingHeroes
            };
            _blockchainClaimableHero = result.GetTotal();
            return result;
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
            var currentReward = _chestRewardManager.GetChestRewardByNetwork(type, tokenData.NetworkSymbol) +
                                _chestRewardManager.GetClaimPendingRewardByNetwork(type, tokenData.NetworkSymbol);

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

        public class ClaimHeroResult {
            public ClaimHeroResponse Response;
            public int LostHeroAmount;
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