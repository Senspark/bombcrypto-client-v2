using System;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using UnityEngine;

namespace App {
    public class EditorBlockchainManager : IBlockchainManager {
        private const double CoinDecimal = 1e18;
        private readonly IAccountManager _accountManager;
        private Task _initializer;
        private string _heroDesignAddress;
        private string _houseDesignAddress;
        private bool _simulated;

        public EditorBlockchainManager(IAccountManager accountManager, bool simulated) {
            _accountManager = accountManager;
            _simulated = simulated;
        }

        public Task<bool> Initialize() {
            _initializer = InitializeImpl();
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        private async Task InitializeImpl() {
            _heroDesignAddress = await GetHeroDesignAddress();
            _houseDesignAddress = await GetHouseDesignAddress();
        }

        private Task<string> ReadHeroTokenContract(string method, params string[] args) {
            var result = string.Empty;
            return Task.FromResult(result);
        }

        private Task<string> ReadHeroDesignContract(string method, params string[] args) {
            var result = string.Empty;
            return Task.FromResult(result);
        }

        private Task<string> ReadHouseTokenContract(string method, params string[] args) {
            var result = string.Empty;
            return Task.FromResult(result);
        }

        private Task<string> ReadHouseDesignContract(string method, params string[] args) {
            var result = string.Empty;
            return Task.FromResult(result);
        }

        private Task<string> GetHeroDesignAddress() {
            var result = string.Empty;
            return Task.FromResult(result);
        }

        private Task<string> GetHouseDesignAddress() {
            var result = string.Empty;
            return Task.FromResult(result);
        }

        public Task<bool> InitBlockchainConfig(NetworkType networkType, bool production) {
            return Task.FromResult(true);
        }

        public Task<double> GetBalance(RpcTokenCategory category) {
            var result = 100d;
            return Task.FromResult(result);
        }

        public async Task<int> GetHeroIdCounter() {
            if (_simulated) {
                return 0;
            }
            
            try {
                var response = await ReadHeroTokenContract("tokenIdCounter");
                var result = int.Parse(response);
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<int> GetHeroLimit() {
            if (_simulated) {
                return 500;
            }

            try {
                var response = await ReadHeroDesignContract("getTokenLimit");
                var result = int.Parse(response);
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public Task<BHeroPrice> GetHeroPrice() {
            return Task.FromResult(new BHeroPrice(45, 10, 10, 10, 10));
        }

        public async Task<double[,]> GetHeroUpgradeCost() {
            if (_simulated) {
                return new double[6, 4];
            }
            
            try {
                var response = await ReadHeroDesignContract("getUpgradeCosts");
                var result = JsonConvert.DeserializeObject<double[,]>(response);
                for (var i = 0; i < result.GetLength(0); ++i) {
                    for (var j = 0; j < result.GetLength(1); ++j) {
                        result[i, j] /= CoinDecimal;
                    }
                }
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<AbilityDesign[]> GetHeroAbilityDesigns() {
            if (_simulated) {
                var design = new AbilityDesign[6];
                Array.Fill(design, new AbilityDesign {
                    MinCost = 5,
                    MaxCost = 10,
                    IncrementalCost = 1,
                });
                return design;
            }
            try {
                var response = await ReadHeroDesignContract("getAbilityDesigns");
                var raw = JsonConvert.DeserializeObject<double[,]>(response);
                var result = new AbilityDesign[raw.GetLength(0)];
                for (var i = 0; i < raw.GetLength(0); ++i) {
                    for (var j = 0; j < raw.GetLength(1); ++j) {
                        raw[i, j] /= CoinDecimal;
                    }
                    result[i] = new AbilityDesign {
                        MinCost = raw[i, 0],
                        MaxCost = raw[i, 1],
                        IncrementalCost = raw[i, 2],
                    };
                }
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<int> GetClaimableHero() {
            if (_simulated) {
                return 0;
            }

            try {
                var response = await ReadHeroTokenContract("getClaimableTokens", _accountManager.Account);
                var result = int.Parse(response);
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public Task<int> GetGiveAwayHero() {
            return Task.FromResult(0);
        }

        public async Task<ProcessToken> GetPendingHero() {
            var processToken = new ProcessToken {
                pendingHeroes = 0,
                pendingHeroesFusion = 0
            };
            if (_simulated) {
                return processToken;
            }

            try {
                var response = await ReadHeroTokenContract("getPendingTokens", _accountManager.Account);
                var result = int.Parse(response);
                return processToken;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public Task<bool> BuyHero(int count, BuyHeroCategory category, bool isHeroS) {
            return Task.FromResult(true);
        }

        public Task<bool> UpgradeHero(int baseId, int materialId) {
            return Task.FromResult(true);
        }

        public Task<bool> ClaimHero() {
            return Task.FromResult(true);
        }

        public Task<bool> ClaimGiveAwayHero() {
            return Task.FromResult(true);
        }

        public Task<HeroProcessTokenResult> ProcessTokenRequests() {
            return Task.FromResult(new HeroProcessTokenResult());
        }

        public Task<bool> HasPendingHeroRandomization(int heroId) {
            return Task.FromResult(false);
        }

        public Task<bool> RandomizeHeroAbilities(int heroId) {
            return Task.FromResult(true);
        }

        public Task<bool> ProcessHeroRandomizeAbilities(int heroId) {
            return Task.FromResult(true);
        }

        public Task<bool> IsSuperBoxEnabled() {
            return Task.FromResult(true);
        }

        public async Task<int> GetHouseLimit() {
            if (_simulated) {
                return 0;
            }

            try {
                var response = await ReadHouseDesignContract("getTokenLimit");
                var result = int.Parse(response);
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<double[]> GetHousePrice() {
            if (_simulated) {
                return new double[6];
            }
            
            try {
                var response = await ReadHouseDesignContract("getMintCosts");
                var entries = JsonConvert.DeserializeObject<double[]>(response);
                var result = entries.Select(entry => entry / CoinDecimal).ToArray();
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<int[]> GetAvailableHouse() {
            if (_simulated) {
                return new int[6];
            }
            
            try {
                var response = await ReadHouseTokenContract("getMintAvailable");
                var result = JsonConvert.DeserializeObject<int[]>(response);
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }
        
        public async Task<int[]> GetHouseMintLimits() {
            if (_simulated) {
                return new int[6];
            }
            
            try {
                var response = await ReadHouseDesignContract("getMintLimits");
                var result = JsonConvert.DeserializeObject<int[]>(response);
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<HouseStats[]> GetHouseStats() {
            if (_simulated) {
                var result = new HouseStats[6];
                result[0] = new HouseStats() {
                    Capacity = 4, Recovery = 120
                };
                result[1] = new HouseStats() {
                    Capacity = 6, Recovery = 300
                };
                result[2] = new HouseStats() {
                    Capacity = 8, Recovery = 480
                };
                result[3] = new HouseStats() {
                    Capacity = 10, Recovery = 660
                };
                result[4] = new HouseStats() {
                    Capacity = 12, Recovery = 840
                };
                result[5] = new HouseStats() {
                    Capacity = 14, Recovery = 1020
                };
                return result;
            }
            
            try {
                var response = await ReadHouseDesignContract("getRarityStats");
                var entries = JsonConvert.DeserializeObject<int[,]>(response);
                var result = new HouseStats[entries.GetLength(0)];
                for (var i = 0; i < entries.GetLength(0); ++i) {
                    result[i] = new HouseStats {
                        Recovery = entries[i, 0],
                        Capacity = entries[i, 1],
                    };
                }
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public Task<bool> BuyHouse(int rarity) {
            return Task.FromResult(true);
        }

        public Task<bool> Deposit(int amount, int category) {
            return Task.FromResult(true);
        }

        public Task<string> Sign(string message, string address) {
            return Task.FromResult(string.Empty);
        }

        public Task<string> ConnectAccount() {
            return Task.FromResult(_accountManager.Account);
        }

        public Task<bool> IsValidChainId() {
            return Task.FromResult(true);
        }

        public Task<bool> FusionHero(int[] heroIds) {
            return Task.FromResult(true);
        }

        public Task<bool> Fusion(int[] mainHeroIds, int[] secondHeroIds) {
            return Task.FromResult(true);
        }

        public Task<bool> RepairShield(int idHeroS, int[] idHeroesBurn) {
            return Task.FromResult(true);
        }

        public Task<bool> GetNFT(int amount, int eventId, int nonce, string signature) {
            return Task.FromResult(true);
        }

        public Task<string> ClaimToken(double amount, int tokenType, int nonce, string[] details, string signature,
            string formatType, int waitConfirmations) {
            return Task.FromResult("");
        }

        public Task<int> GetRockAmount() {
            var result = 1000;
            return Task.FromResult(result);
        }

        public Task<string> CreateRock(int[] idHeroesBurn) {
            return Task.FromResult("");
        }

        public Task<bool> RepairShieldWithRock(int idHeroS, int amountRock) {
            return Task.FromResult(true);
        }

        public Task<bool> UpgradeShieldLevel(int idHeroS, int amountRock) {
            return Task.FromResult(true);
        }
        
        public Task<bool> UpgradeShieldLevelV2(int idHero, int nonce, string signature) {
            return Task.FromResult(true);
        }
        
        public Task<bool> CanUseVoucher(int voucherType) {
            return Task.FromResult(true);
        }

        public Task<bool> BuyHeroUseVoucher(string tokenPay, int voucherType, int heroQuantity, string amount, int nonce, string signature) {
            return Task.FromResult(true);
        }

        public Task<bool> Exchange_BuyBcoin(double amount, BuyBcoinCategory category) {
            return Task.FromResult(true);
        }

        public Task<ExchangeInfo> Exchange_GetInfo() {
            var info = new ExchangeInfo() {
                price = 0.35,
                slippage = 0.55,
                fee = 0.55,
            };
            return Task.FromResult(info);
        }

        public Task<bool> StakeToHero(int id, double amount, string tokenAddress, StakeHeroCategory category) {
            return Task.FromResult(true);
        }

        public Task<bool> WithDrawFromHeroId(int id, double amount, string tokenAddress) {
            return Task.FromResult(true);
        }

        public Task<double> GetStakeFromHeroId(int id, string tokenAddress) {
            return Task.FromResult(0.0);
        }

        public Task<double> GetFeeFromHeroId(int id, string tokenAddress) {
            return Task.FromResult(0.0);
        }

        public Task<bool> DepositTon(string invoice, double amount) {
            return Task.FromResult(true);
        }
        
        public Task<bool> DepositAirdrop(string invoice, string amount, string chainId) {
            return Task.FromResult(true);
        }

        public Task<bool> BirthdayEvent_IsUserUsedDiscount() {
            var result = false;
            return Task.FromResult(result);
        }

        public Task<(DateTime, DateTime)> BirthdayEvent_GetEventLocalTime() {
            var start = DateTime.MinValue;
            var end = DateTime.Now.AddHours(1);
            return Task.FromResult((start, end));
        }

        public Task<bool> BirthdayEvent_BuyHero(BuyHeroCategory category) {
            var result = true;
            return Task.FromResult(result);
        }

        public Task<double> GetTokenBalance(string contractAddress, string abi) {
            var result = 0d;
            return Task.FromResult(result);
        }
    }
}