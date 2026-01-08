using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Senspark;
using Newtonsoft.Json;
using Services.Rewards;
using UnityEngine;
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
        private readonly ICacheRequestManager _cacheRequestManager;
        private List<TokenData> _data;

        private readonly string _getBasePath;
        private readonly string _getFilePath;
        
        private const float HOLD_WALLET_COIN = 5;
        private const string RemoteFolder = "LaunchPad";
        private const string JsonFile = "data.json";
        
        private readonly List<string> _iconNameTonUsed = new List<string>() {
            "BL_COIN",
            "TON_DEPOSITED",
            "BOMBERMAN",
            "BCOIN_DEPOSITED"
        };

        public DefaultLaunchPadManager(
            ILogManager logManager,
            IChestRewardManager chestRewardManager,
            IBlockchainStorageManager blockchainStorageManager,
            ICacheRequestManager cacheRequestManager
        ) {
            _logManager = logManager;
            _chestRewardManager = chestRewardManager;
            _blockchainStorageManager = blockchainStorageManager;
            _cacheRequestManager = cacheRequestManager;

            _getBasePath = Path.Combine(Application.streamingAssetsPath, RemoteFolder);
            _getFilePath = Path.Combine(_getBasePath, JsonFile);
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task SyncRemoteData() {
            if (_data != null) {
                return;
            }

            var jsonPath = _getFilePath;
            var data = await GetTextFile(jsonPath);
            var result = JsonConvert.DeserializeObject<TokenData[]>(data);
            Assert.IsNotNull(result);

            _data = new List<TokenData>(result);
            foreach (var r in result) {
                // icon -> sprite
                var iconName = r.iconName;
                if (!string.IsNullOrEmpty(iconName)) {
                    try {
                        if(AppConfig.IsTon() && !IsIconUseForTon(iconName)) {
                            continue;
                        }
                        var iconPath = Path.Combine(_getBasePath, $"{iconName}.png");
                        r.icon = await Utils.LoadImageFromPath(iconPath);
                    } catch (Exception e) {
                        // ignore because of unity main thread
                        _logManager.Log(e.Message);
                    }
                }
            }
        }

        public bool CanShowInLaunchPad(IRewardType type, NetworkSymbol symbol) {
            var token = GetData(type, symbol);
            return token != null && token.displayOnLaunchPad;
        }

        public bool CanShowInLaunchPad(ITokenReward type) {
            return CanShowInLaunchPad(type.Type, new NetworkSymbol(type.Network));
        }

        public bool CanClaim(IRewardType type, NetworkSymbol symbol, float rewardValue) {
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
                
                var fee = token.claimFee;
                if (fee == null) {
                    return true;
                }
                var currency = _chestRewardManager.GetChestReward(fee.tokenName);
                return fee.IsTrue(currency);
            } catch (Exception) {
                return false;
            }
        }

        public (float, string) GetClaimFee(IRewardType type, NetworkSymbol symbol) {
            var token = GetData(type, symbol);
            
            var fee = token.claimFee;
            if (fee == null) {
                return (0, null);
            }
            var feeName = fee.tokenName;
            var feeToken = GetData(feeName, symbol);
            var currency = feeToken.displayName;
            
            if (token.useTax) {
                var claimAmount = _chestRewardManager.GetChestReward(type);
                var tax = GetTaxValue(claimAmount);
                return (claimAmount * tax, currency);
            }
            return (fee.value, currency);
        }

        public TokenData GetCurrentNetworkData(BlockRewardType type) {
            var tokenName = RewardUtils.ConvertToBlockRewardType(type);
            var network = NetworkSymbol.Convert(_networkType);
            return GetData(tokenName, network);
        }

        public TokenData GetData(IRewardType type, NetworkSymbol symbol) {
            var tokenName = type.Name;
            return GetData(tokenName, symbol);
        }

        public TokenData GetData(ITokenReward type) {
            var tokenName = type.Type.Name;
            var network = type.Network;
            return GetData(tokenName, new NetworkSymbol(network));
        }

        public TokenData GetData(BlockRewardType type, NetworkSymbol symbol) {
            var tokenName = RewardUtils.ConvertToBlockRewardType(type);
            return GetData(tokenName, symbol);
        }

        public TokenData GetData(string tokenName, NetworkSymbol symbol) {
            var result = _data.FirstOrDefault(e => e.tokenName == tokenName && e.NetworkSymbol == symbol);
            return result;
        }

        public List<TokenData> GetForceDisplayTokens() {
            var list = _data.Where(e =>
                //DevHoang: Add new airdrop
                e.networkSymbol != "TON" && 
                e.networkSymbol != "SOL" &&
                e.networkSymbol != "RON" &&
                e.networkSymbol != "BAS" &&
                e.networkSymbol != "VIC" &&
                e.alwaysDisplay).ToList();
            return list;
        }

        public List<TokenData> GetForceDisplayTokensTelegram() {
            var list = _data.Where(e =>
                e.code == 18 || // StarCore - TR
                (e.networkSymbol == "TON" &&
                e.alwaysDisplay)
            ).ToList();
            return list;
        }
        public List<TokenData> GetForceDisplayTokensSolana() {
            var list = _data.Where(e =>
                (e.networkSymbol == "SOL" &&
                 e.alwaysDisplay ||
                 e.code == 18) // StarCore - TR
            ).ToList();
            return list;
        }
        
        public List<TokenData> GetForceDisplayTokensRonin() {
            var list = _data.Where(e =>
                    (e.networkSymbol == "RON" &&
                     e.alwaysDisplay)
            ).ToList();
            return list;
        }
        
        public List<TokenData> GetForceDisplayTokensBase() {
            var list = _data.Where(e =>
                    (e.networkSymbol == "BAS" &&
                     e.alwaysDisplay)
            ).ToList();
            return list;
        }
        
        public List<TokenData> GetForceDisplayTokensViction() {
            var list = _data.Where(e =>
                (e.networkSymbol == "VIC" &&
                 e.alwaysDisplay)
            ).ToList();
            return list;
        }
        
        public IRewardType CreateRewardType(string tokenType) {
            var blockType = RewardUtils.ConvertToBlockRewardType(tokenType);
            return new RewardType(blockType, tokenType);
        }

        private float GetMinValueToClaim(IRewardType type, NetworkSymbol symbol) {
            var tokenName = type.Name;
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
        
        private async Task<string> GetTextFile(string path) {
            if (Utils.IsUrl(path)) {
                var (code, res) = await _cacheRequestManager.GetWebResponse(SFSDefine.SFSCommand.GET_LAUNCH_PAD_DATA, path);
                return res;
            }
            return await File.ReadAllTextAsync(path);
        }

        private bool IsIconUseForTon(string iconName) {
            return _iconNameTonUsed.Contains(iconName);
        }
    }
}