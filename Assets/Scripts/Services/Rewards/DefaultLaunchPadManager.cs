using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.UI;
using Senspark;
using Services.Rewards;

using UnityEngine.Assertions;

namespace App {
    public class DefaultLaunchPadManager : ILaunchPadManager {
        private class RewardType : IRewardType {
            public BlockRewardType Type { get; }
            public string Name { get; }

            public RewardType(BlockRewardType type, string name) {
                Type = type;
                Name = name;
            }
        }

        private readonly ILogManager _logManager;
        private readonly IChestRewardManager _chestRewardManager;
        private readonly IBlockchainStorageManager _blockchainStorageManager;
        private readonly NetworkType _networkType;
        private List<TokenData> _data;

        private const float HOLD_WALLET_COIN = 5;
        private const string ResourcePath = "LaunchPadResource";

        public DefaultLaunchPadManager(
            ILogManager logManager,
            IChestRewardManager chestRewardManager,
            IBlockchainStorageManager blockchainStorageManager
        ) {
            _logManager = logManager;
            _chestRewardManager = chestRewardManager;
            _blockchainStorageManager = blockchainStorageManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public UniTask SyncRemoteData() {
            if (_data != null) {
                return UniTask.CompletedTask;
            }

            var resource = UnityEngine.Resources.Load<LaunchPadResource>(ResourcePath);
            Assert.IsNotNull(resource);

            var tokens = LaunchPadTokenTable.Build();
            foreach (var token in tokens) {
                token.icon = resource.GetIcon(token.tokenName, token.networkSymbol);
            }
            var missing = resource.GetMissingIcons();
            if (missing.Count > 0) {
                _logManager.Log($"[LaunchPad] Thiếu icon chưa gán trong LaunchPadResource: {string.Join(", ", missing)}");
            }
            _data = tokens;
            return UniTask.CompletedTask;
        }

        public bool CanShowInLaunchPad(IRewardType type, DataType symbol) {
            var token = GetData(type, symbol);
            return token != null && token.displayOnLaunchPad;
        }

        public bool CanShowInLaunchPad(ITokenReward type) {
            return CanShowInLaunchPad(type.Type, RewardUtils.ParseDataType(type.Network));
        }

        public bool CanClaim(IRewardType type, DataType symbol, float rewardValue) {
            var minValue = GetMinValueToClaim(type, symbol);
            if (minValue == 0) {
                minValue = float.Epsilon;
            }
            if (rewardValue < minValue) {
                return false;
            }
            try {
                var token = GetData(type, symbol);
                if (!token.enableClaim) {
                    return false;
                }
                if (type.Type is BlockRewardType.BCoin or BlockRewardType.Senspark or BlockRewardType.BCoinDeposited) {
                    var walletBcoin = _blockchainStorageManager.GetBalance(BlockRewardType.BCoin);
                    var walletSen = _blockchainStorageManager.GetBalance(BlockRewardType.Senspark);
                    return walletBcoin >= HOLD_WALLET_COIN || walletSen >= HOLD_WALLET_COIN;
                }
                
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public (float, string) GetClaimFee(IRewardType type, DataType symbol) {
            var token = GetData(type, symbol);
            if (!token.useTax) {
                return (0, null);
            }
            var claimAmount = _chestRewardManager.GetChestReward(type);
            var tax = GetTaxValue(claimAmount);
            return (claimAmount * tax, token.displayName);
        }

        public TokenData GetCurrentNetworkData(BlockRewardType type) {
            var network = RewardUtils.ConvertNetworkToDatatype(_networkType);
            return GetData(type, network);
        }

        public TokenData GetData(IRewardType type, DataType symbol) {
            return GetData(type.Type, symbol);
        }

        public TokenData GetData(ITokenReward type) {
            var network = type.Network;
            return GetData(type.Type.Type, RewardUtils.ParseDataType(network));
        }

        public TokenData GetData(BlockRewardType type, DataType symbol) {
            return _data.FirstOrDefault(e => e.tokenName == type && e.networkSymbol == symbol);
        }

        public List<TokenData> GetForceDisplayTokens() {
            var list = _data.Where(e =>
                //DevHoang: Add new airdrop
                e.networkSymbol != DataType.TON &&
                e.networkSymbol != DataType.SOL &&
                e.networkSymbol != DataType.RON &&
                e.networkSymbol != DataType.BAS &&
                e.networkSymbol != DataType.VIC &&
                e.alwaysDisplay).ToList();
            return list;
        }

        public List<TokenData> GetForceDisplayTokensTelegram() {
            var list = _data.Where(e =>
                e.code == 18 || // StarCore - TR
                (e.networkSymbol == DataType.TON &&
                e.alwaysDisplay)
            ).ToList();
            return list;
        }
        public List<TokenData> GetForceDisplayTokensSolana() {
            var list = _data.Where(e =>
                (e.networkSymbol == DataType.SOL &&
                 e.alwaysDisplay ||
                 e.code == 18) // StarCore - TR
            ).ToList();
            return list;
        }
        
        public List<TokenData> GetForceDisplayTokensRonin() {
            var list = _data.Where(e =>
                    (e.networkSymbol == DataType.RON &&
                     e.alwaysDisplay)
            ).ToList();
            return list;
        }
        
        public List<TokenData> GetForceDisplayTokensBase() {
            var list = _data.Where(e =>
                    (e.networkSymbol == DataType.BAS &&
                     e.alwaysDisplay)
            ).ToList();
            return list;
        }
        
        public List<TokenData> GetForceDisplayTokensViction() {
            var list = _data.Where(e =>
                (e.networkSymbol == DataType.VIC &&
                 e.alwaysDisplay)
            ).ToList();
            return list;
        }
        
        public IRewardType CreateRewardType(BlockRewardType type) {
            var name = RewardUtils.ConvertToBlockRewardType(type);
            return new RewardType(type, name);
        }

        private float GetMinValueToClaim(IRewardType type, DataType symbol) {
            var token = GetData(type, symbol);
            return token.minValueToClaim;
        }

        private static float GetTaxValue(float claimAmount) {
            return claimAmount switch {
                < 60f => 0.1f,
                < 80f => 0.06f,
                _ => 0.03f
            };
        }
    }
}