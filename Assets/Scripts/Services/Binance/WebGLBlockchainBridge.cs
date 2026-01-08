using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Castle.Core.Internal;

using Communicate;

using Senspark;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR || UNITY_WEBGL
namespace App {
    public class WebGLBlockchainBridge : IBlockchainBridge {
        private const int CoinDecimalDigits = 18;
        private const double CoinDecimal = 1e18;
        private readonly ILogManager _logManager;
        private readonly IMasterUnityCommunication _unityCommunication;
        private readonly JavascriptProcessor _processor;

        public WebGLBlockchainBridge(ILogManager logManager, IMasterUnityCommunication unityCommunication) {
            _logManager = logManager;
            _unityCommunication = unityCommunication;
            _processor = JavascriptProcessor.Instance;
        }

        public async Task<double> GetBalance(RpcTokenCategory category, string walletAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["category"] = (int)category,
                    ["walletAddress"] = walletAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_BALANCE, data);
                if (response.IsNullOrEmpty()) {
                    _logManager.Log("response is null or empty");
                    return 0;
                }
                var result = double.Parse(response, NumberStyles.Number, CultureInfo.InvariantCulture);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<double> GetSensparkBalance(string walletAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_SENSPARK_BALANCE, data);
                if (response.IsNullOrEmpty()) {
                    _logManager.Log("response is null or empty");
                    return 0;
                }
                var result = double.Parse(response) / CoinDecimal;
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<double> GetUsdtBalance(string walletAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_USDT_BALANCE, data);
                if (response.IsNullOrEmpty()) {
                    _logManager.Log("response is null or empty");
                    return 0;
                }
                var result = double.Parse(response) / 1e6;
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<int> GetHeroIdCounter() {
            try {
                _logManager.Log();
                
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_HERO_ID_COUNTER);
                var result = int.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<int> GetHeroLimit() {
            try {
                _logManager.Log();
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_HERO_LIMIT);
                var result = int.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<BHeroPrice> GetHeroPrice() {
            try {
                _logManager.Log();
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_HERO_PRICE);
                var result = JsonConvert.DeserializeObject<BHeroPrice>(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<double[,]> GetHeroUpgradeCost() {
            try {
                _logManager.Log();
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_HERO_UPGRADE_COST);
                var result = JsonConvert.DeserializeObject<double[,]>(response);
                for (var i = 0; i < result.GetLength(0); ++i) {
                    for (var j = 0; j < result.GetLength(1); ++j) {
                        result[i, j] /= CoinDecimal;
                    }
                }
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<AbilityDesign[]> GetHeroAbilityDesigns() {
            try {
                _logManager.Log();
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_HERO_ABILITY_DESIGNS);       
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

        public async Task<int> GetClaimableHero(string walletAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_CLAIMABLE_HERO, data);
                var result = int.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<int> GetGiveAwayHero(string walletAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_GIVE_AWAY_HERO, data);
                if (response.IsNullOrEmpty()) {
                    _logManager.Log("response is null or empty");
                    return 0;
                }
                var result = int.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<ProcessToken> GetPendingHero(string walletAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_PENDING_HERO, data);
                _logManager.Log($"response = {response}");
                var result = JsonConvert.DeserializeObject<ProcessToken>(response);
                _logManager.Log($"result = {result.pendingHeroes},{result.pendingHeroesFusion}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> BuyHero(string walletAddress, int count, BuyHeroCategory category, bool isHeroS) {
            try {
                _logManager.Log();
                var cmd = isHeroS ? BlockChainCommand.BUY_HERO_S : BlockChainCommand.BUY_HERO;
                var data = new JObject {
                    ["walletAddress"] = walletAddress,
                    ["count"] = count,
                    ["category"] = (int)category
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(cmd, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> UpgradeHero(string walletAddress, int baseId, int materialId) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress,
                    ["baseId"] = baseId,
                    ["materialId"] = materialId
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.UPGRADE_HERO, data); 
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> ClaimHero(string walletAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.CLAIM_HERO, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> ClaimGiveAwayHero() {
            try {
                _logManager.Log();
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.CLAIM_GIVE_AWAY_HERO);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<HeroProcessTokenResult> ProcessTokenRequests(string walletAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.PROCESS_TOKEN_REQUESTS, data);
                _logManager.Log($"response = {response}");
                var result = JsonConvert.DeserializeObject<HeroProcessTokenResult>(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> HasPendingHeroRandomization(int heroId) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["heroId"] = heroId
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.HAS_PENDING_HERO_RANDOMIZE, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> RandomizeHeroAbilities(string walletAddress, int heroId) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress,
                    ["heroId"] = heroId
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.RANDOMIZE_HERO_ABILITIES, data);
                _logManager.Log($"response = {response}");
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> ProcessHeroRandomizeAbilities(string walletAddress, int heroId) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress,
                    ["heroId"] = heroId
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.PROCESS_HERO_RANDOMIZE_AB, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> IsSuperBoxEnabled() {
            try {
                _logManager.Log();
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.IS_SUPER_BOX_ENABLED);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<int> GetHouseLimit() {
            try {
                _logManager.Log();
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_HOUSE_LIMIT);
                var result = int.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<double[]> GetHousePrice() {
            try {
                _logManager.Log();
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_HOUSE_PRICE);
                var entries = JsonConvert.DeserializeObject<double[]>(response);
                var result = entries.Select(entry => entry / CoinDecimal).ToArray();
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<int[]> GetAvailableHouse() {
            try {
                _logManager.Log();
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_AVAILABLE_HOUSE);
                var result = JsonConvert.DeserializeObject<int[]>(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<int[]> GetHouseMintLimits() {
            try {
                _logManager.Log();
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_HOUSE_MINT_LIMITS);
                var result = JsonConvert.DeserializeObject<int[]>(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<HouseStats[]> GetHouseStats() {
            try {
                _logManager.Log();
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_HOUSE_STATS);
                var entries = JsonConvert.DeserializeObject<int[,]>(response);
                var result = new HouseStats[entries.GetLength(0)];
                for (var i = 0; i < entries.GetLength(0); ++i) {
                    result[i] = new HouseStats {
                        Recovery = entries[i, 0],
                        Capacity = entries[i, 1],
                    };
                }
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> BuyHouse(string walletAddress, int rarity) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress,
                    ["rarity"] = rarity
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.BUY_HOUSE, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> Deposit(string walletAddress, int amount, int category) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress,
                    ["amount"] = amount,
                    ["category"] = category
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.DEPOSIT_V2, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> FusionHero(int[] heroIds) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["heroIds"] = new JArray(heroIds)
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.FUSION_HERO, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> Fusion(int[] mainHeroIds, int[] secondHeroIds) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["mainHeroIds"] = new JArray(mainHeroIds),
                    ["secondHeroIds"] = new JArray(secondHeroIds)
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.FUSION, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> RepairShield(int idHeroS, int[] idHeroesBurn) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["idHeroS"] = idHeroS,
                    ["idHeroesBurn"] = new JArray(idHeroesBurn)
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.REPAIR_SHIELD, data);
                var result = bool.Parse(response);
                if (!result) {
                    throw new Exception("Burn Failed");
                }
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> GetNFT(int amount, int eventId, int nonce, string signature) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["amount"] = amount,
                    ["eventId"] = eventId,
                    ["nonce"] = nonce,
                    ["signature"] = signature
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_NFT, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<string> ClaimToken(int tokenType, double amount, int nonce, string[] details, string signature,
            string formatType, int waitConfirmations) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["tokenType"] = tokenType,
                    ["amount"] = amount,
                    ["nonce"] = nonce,
                    ["details"] = new JArray(details),
                    ["signature"] = signature,
                    ["formatType"] = formatType,
                    ["waitConfirmations"] = waitConfirmations
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.CLAIM_TOKEN, data);
                var result = response;
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }
        
        public async Task<int> GetRockAmount(string walletAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_ROCK_AMOUNT, data);
                if (response.IsNullOrEmpty()) {
                    _logManager.Log("response is null or empty");
                    return 0;
                }
                _logManager.Log($"{response}");
                int.TryParse(response, out var result);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<string> CreateRock(int[] idHeroesBurn) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["idHeroesBurn"] = new JArray(idHeroesBurn)
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.CREATE_ROCK, data);
                //var result = bool.Parse(response);
                _logManager.Log($"result = {response}");
                return response;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> RepairShieldWithRock(int idHeroS, int amountRock) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["idHeroS"] = idHeroS,
                    ["amountRock"] = amountRock
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.REPAIR_SHIELD_WITH_ROCK, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> UpgradeShieldLevel(int idHeroS, int amountRock) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["idHeroS"] = idHeroS,
                    ["amountRock"] = amountRock
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.UPGRADE_SHIELD_LEVEL, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }
        
        public async Task<bool> UpgradeShieldLevelV2(int idHero, int nonce, string signature) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["idHero"] = idHero,
                    ["nonce"] = nonce,
                    ["signature"] = signature
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.UPGRADE_SHIELD_LEVEL_V2, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }
        
        public async Task<bool> CanUseVoucher(int voucherType, string walletAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["voucherType"] = voucherType,
                    ["walletAddress"] = walletAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.CAN_USE_VOUCHER, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> BuyHeroUseVoucher(string walletAddress, string tokenPay, int voucherType,
            int heroQuantity, string amount, int nonce, string signature) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress,
                    ["tokenPay"] = tokenPay,
                    ["voucherType"] = voucherType,
                    ["heroQuantity"] = heroQuantity,
                    ["amount"] = amount,
                    ["nonce"] = nonce,
                    ["signature"] = signature
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.BUY_HERO_USE_VOUCHER, data);
                _logManager.Log($"response = {response}");
                var result = JsonConvert.DeserializeObject<Message>(response);
                Assert.IsNotNull(result);
                if (!result.code) {
                    throw new Exception(result.message);
                }
                _logManager.Log($"result = {result}");
                return true;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }
        
        public async Task<bool> Exchange_BuyBcoin(double amount, BuyBcoinCategory category, string walletAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["amount"] = amount,
                    ["category"] = (int)category,
                    ["walletAddress"] = walletAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.EXCHANGE_BUY_BCOIN, data);
                _logManager.Log($"response = {response}");
                var result = JsonConvert.DeserializeObject<Message>(response);
                Assert.IsNotNull(result);
                if (!result.code) {
                    throw new Exception(result.message);
                }
                _logManager.Log($"result = {result}");
                return true;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }
        
        public async Task<ExchangeInfo> Exchange_GetInfo() {
            try {
                _logManager.Log();
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.EXCHANGE_GET_INFO);
                _logManager.Log($"response = {response}");
                var result = JsonConvert.DeserializeObject<ExchangeInfo>(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> StakeToHero(string walletAddress, int id, double amount, string tokenAddress, StakeHeroCategory category) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["walletAddress"] = walletAddress,
                    ["id"] = id,
                    ["amount"] = amount,
                    ["tokenAddress"] = tokenAddress,
                    ["category"] = (int)category
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.STAKE_TO_HERO_V2, data);
                var result = bool.Parse(response);
                _logManager.Log($"result stake to hero = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> WithDrawFromHeroId(int id, double amount, string tokenAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["id"] = id,
                    ["amount"] = amount,
                    ["tokenAddress"] = tokenAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.WITHDRAW_FROM_HERO_ID_V2, data);
                var result = bool.Parse(response);
                _logManager.Log($"withdraw from hero = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<double> GetStakeFromHeroId(int id, string tokenAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["id"] = id,
                    ["tokenAddress"] = tokenAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_STAKE_FROM_HERO_ID_V2, data);
                var result = double.Parse(response, NumberStyles.Number, CultureInfo.InvariantCulture) ;
                _logManager.Log($"result staked from hero = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<double> GetFeeFromHeroId(int id, string tokenAddress) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["id"] = id,
                    ["tokenAddress"] = tokenAddress
                };
                var response = await _unityCommunication.UnityToReact.CallBlockChain(BlockChainCommand.GET_FEE_FROM_HERO_ID_V2, data);
                var result = double.Parse(response, NumberStyles.Number, CultureInfo.InvariantCulture) ;
                _logManager.Log($"result staked from hero = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> DepositTon(string invoice, double amount) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["invoice"] = invoice,
                    ["amount"] = amount
                };
                var response = await _unityCommunication.UnityToReact.SendToReact(ReactCommand.DEPOSIT, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }
        
        public async Task<bool> DepositAirdrop(string invoice, string amount, string chainId) {
            try {
                _logManager.Log();
                var data = new JObject {
                    ["invoice"] = invoice,
                    ["amount"] = amount,
                    ["chainId"] = chainId
                };
                var response = await _unityCommunication.UnityToReact.SendToReact(ReactCommand.DEPOSIT_AIRDROP, data);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

#warning This method is not used
        public async Task<bool> BirthdayEvent_IsUserUsedDiscount(string walletAddress) {
            try {
                _logManager.Log();
                var response = await _processor.CallMethod("BirthdayEvent_IsUserUsedDiscount", walletAddress);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }
        
#warning This method is not used
        public async Task<(DateTime, DateTime)> BirthdayEvent_GetLocalTime() {
            try {
                _logManager.Log();
                var response = await _processor.CallMethod("BirthdayEvent_GetTime");
                var result = JsonConvert.DeserializeObject<long[]>(response);
                Assert.IsNotNull(result);
                _logManager.Log($"result = {result}");
                var start = DateTimeOffset.FromUnixTimeSeconds(result[0]).LocalDateTime;
                var end = DateTimeOffset.FromUnixTimeSeconds(result[1]).LocalDateTime;
                return (start, end);
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }
        
#warning This method is not used
        public async Task<bool> BirthdayEvent_BuyHero(BuyHeroCategory category, string walletAddress) {
            try {
                _logManager.Log();
                var response = await _processor.CallMethod("BirthdayEvent_BuyHero", (int) category, walletAddress);
                var result = JsonConvert.DeserializeObject<Message>(response);
                Assert.IsNotNull(result);
                _logManager.Log($"result = {result.code}");
                if (!result.code) {
                    throw new Exception(result.message);
                }
                return result.code;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }
    }
}
#endif