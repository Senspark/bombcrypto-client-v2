using System;
using System.Globalization;
using System.Threading.Tasks;

using Senspark;

using Sfs2X.Entities.Data;

using Task = System.Threading.Tasks.Task;

namespace App {
    public class PolygonClaimTokenManager : IClaimTokenManager {
        private readonly IServerManager _serverManager;
        private readonly IBlockchainManager _blockchainManager;
        private readonly IChestRewardManager _chestRewardManager;
        
        private const string UrlBscMain = "https://bscscan.com/";
        private const string UrlBscTest = "https://testnet.bscscan.com/";
        private const string UrlPolygonMain = "https://polygonscan.com/";
        private const string UrlPolygonTest = "https://amoy.polygonscan.com/";
        
        private const float MinToken = 0;
        
        public PolygonClaimTokenManager(IServerManager serverManager, IBlockchainManager blockchainManager,
            IChestRewardManager chestRewardManager) {
            _serverManager = serverManager;
            _blockchainManager = blockchainManager;
            _chestRewardManager = chestRewardManager;
        }
        
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
        
        public void Destroy() {
        }
        
        public async Task<float> ClaimToken(BlockRewardType type, int code) {
            if (type == BlockRewardType.Hero) {
                throw new Exception("Wrong Method");
            }
            try {
                var response = (PolygonApproveClaimResponse)await _serverManager.General.ApproveClaim(code);
                var claimValue = _chestRewardManager.GetClaimPendingReward(type);
                var result = await _blockchainManager.ClaimToken(response.Amount, response.TokenType,
                    response.Nonce, response.Details, response.Signature, "wei", 1);
                
                if (result == "") {
                    //_serverManager.General.SendMessageSlack(":coin: User Claim Token :x:", data);
                    throw new Exception("Claim Failed");
                }
                var received = await _serverManager.General.ConfirmApproveClaimSuccess(code);
                
                //Lớn hơn lượng min mới notify trên slack
                if (received >= MinToken) {
                    var blockChain = ServiceLocator.Instance.Resolve<INetworkConfig>();
                    var userAddress = ServiceLocator.Instance.Resolve<IAccountManager>().Account;
                    var url = GetUrl(blockChain.NetworkType, blockChain.BlockchainConfig.Network == "mainnet");
                    var network = $"{blockChain.NetworkName} {blockChain.BlockchainConfig.Network}";
                    
                    var data = new SFSObject();
                    data.PutUtfString("Amount", response.Amount.ToString(CultureInfo.CurrentCulture));
                    data.PutUtfString("Received", received.ToString(CultureInfo.CurrentCulture));
                    data.PutUtfString("Token", type.ToString());
                    data.PutUtfString("Network", network);
                    data.PutUtfString("User", $"<{url}address/{userAddress}|Address>");
                    data.PutUtfString("Transaction", $"<{url}tx/{result}|Transaction>");
                    
                    _serverManager.General.SendMessageSlack(":coin: User Claim Token :white_check_mark:", data);
                }
                
                return received;
            } catch (Exception e) {
                throw new Exception($"{e.Message}");
            }
        }
        
        private string GetUrl(NetworkType type, bool isMainnet) {
            return type switch {
                NetworkType.Binance when isMainnet => UrlBscMain,
                NetworkType.Binance when !isMainnet => UrlBscTest,
                NetworkType.Polygon when isMainnet => UrlPolygonMain,
                NetworkType.Polygon when !isMainnet => UrlPolygonTest,
                _ => UrlBscTest
            };
        }
        
        public async Task<ClaimHeroResponse> ClaimHero() {
            try {
                var claimable = _chestRewardManager.GetChestReward(BlockRewardType.Hero) +
                                _chestRewardManager.GetClaimPendingReward(BlockRewardType.Hero);
                var response = new ClaimHeroResponse(0, false, "Claim Failed");
                if (claimable > 0) {
                    response = await CallClaimHeroApi();
                }
                var pendingHero = await _blockchainManager.GetPendingHero();
                if (pendingHero.pendingHeroes > 0) {
                    var detail = await _blockchainManager.ProcessTokenRequests();
                    var msg = detail.result ? null : "Try again";
                    return new ClaimHeroResponse(response.ClaimedAmount, detail, msg);
                }
                return response;
            } catch (Exception e) {
                throw new Exception($"Claim Failed. Reason: {e.Message}");
            }
        }
        
        private async Task<ClaimHeroResponse> CallClaimHeroApi() {
            try {
                const int code = 2;
                var response = (PolygonApproveClaimResponse)await _serverManager.General.ApproveClaim(code);
                var result = await _blockchainManager.ClaimToken(response.Amount, response.TokenType,
                    response.Nonce, response.Details, response.Signature, "wei", 6);
                if (result != "") {
                    var received = await _serverManager.General.ConfirmApproveClaimSuccess(code);
                    return new ClaimHeroResponse((int)received, true, null);
                }
                throw new Exception("Claim Failed");
            } catch (Exception e) {
                return new ClaimHeroResponse(0, false, e.Message);
            }
        }
    }
}