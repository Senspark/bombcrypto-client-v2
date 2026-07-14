#if UNITY_EDITOR
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using UnityEditor;
using UnityEngine;

namespace App {
    public class EditorBlockchainManager : NullBlockchainManager {
        private const double CoinDecimal = 1e18;
        private static readonly HttpClient Http = new HttpClient();

        private const string P = "BombBridgeDebug.";
        private static readonly string[] NetworkNames = { "bsctestnet", "amoy" };

        private readonly IAccountManager _accountManager;
        private string _proxyUrl;
        private int _networkIndex;
        private string _privateKey;
        private string _configWallet;

        public EditorBlockchainManager(IAccountManager accountManager) {
            _accountManager = accountManager;
            LoadConfig();
        }

        private void LoadConfig() {
            _proxyUrl = EditorPrefs.GetString(P + "proxyUrl", "http://127.0.0.1:8555");
            _networkIndex = EditorPrefs.GetInt(P + "networkIndex", 0);
            _privateKey = EditorPrefs.GetString(P + "privateKey", "");
            _configWallet = EditorPrefs.GetString(P + "wallet", "");
        }

        private string NetworkName => NetworkNames[Mathf.Clamp(_networkIndex, 0, NetworkNames.Length - 1)];

        private string Wallet => !string.IsNullOrEmpty(_accountManager?.Account)
            ? _accountManager.Account
            : _configWallet;

        public override async Task<double> GetBalance(RpcTokenCategory category) {
            var r = await Call("GET_BALANCE", new { category = (int)category, walletAddress = Wallet }, false);
            return r.Value<double>();
        }

        public override async Task<int> GetHeroIdCounter() {
            return (await Call("GET_HERO_ID_COUNTER", null, false)).Value<int>();
        }

        public override async Task<int> GetHeroLimit() {
            return (await Call("GET_HERO_LIMIT", null, false)).Value<int>();
        }

        public override async Task<BHeroPrice> GetHeroPrice() {
            return (await Call("GET_HERO_PRICE", null, false)).ToObject<BHeroPrice>();
        }

        public override async Task<double[,]> GetHeroUpgradeCost() {
            var result = (await Call("GET_HERO_UPGRADE_COST", null, false)).ToObject<double[,]>();
            for (var i = 0; i < result.GetLength(0); ++i) {
                for (var j = 0; j < result.GetLength(1); ++j) {
                    result[i, j] /= CoinDecimal;
                }
            }
            return result;
        }

        public override async Task<AbilityDesign[]> GetHeroAbilityDesigns() {
            var raw = (await Call("GET_HERO_ABILITY_DESIGNS", null, false)).ToObject<double[,]>();
            var result = new AbilityDesign[raw.GetLength(0)];
            for (var i = 0; i < raw.GetLength(0); ++i) {
                result[i] = new AbilityDesign {
                    MinCost = raw[i, 0] / CoinDecimal,
                    MaxCost = raw[i, 1] / CoinDecimal,
                    IncrementalCost = raw[i, 2] / CoinDecimal,
                };
            }
            return result;
        }

        public override async Task<int> GetClaimableHero() {
            return (await Call("GET_CLAIMABLE_HERO", new { walletAddress = Wallet }, false)).Value<int>();
        }

        public override async Task<ProcessToken> GetPendingHero() {
            return (await Call("GET_PENDING_HERO", new { walletAddress = Wallet }, false)).ToObject<ProcessToken>();
        }

        public override async Task<bool> IsSuperBoxEnabled() {
            return (await Call("IS_SUPER_BOX_ENABLED", null, false)).Value<bool>();
        }

        public override async Task<bool> HasPendingHeroRandomization(int heroId) {
            return (await Call("HAS_PENDING_HERO_RANDOMIZE", new { heroId }, false)).Value<bool>();
        }

        public override async Task<bool> BuyHero(int count, BuyHeroCategory category, bool isHeroS) {
            var command = isHeroS ? "BUY_HERO_S" : "BUY_HERO";
            var r = await Call(command, new { walletAddress = Wallet, count, category = (int)category }, true);
            return r.Value<bool>();
        }

        public override async Task<bool> UpgradeHero(int baseId, int materialId) {
            var r = await Call("UPGRADE_HERO", new { walletAddress = Wallet, baseId, materialId }, true);
            return r.Value<bool>();
        }

        public override async Task<bool> ClaimHero() {
            return (await Call("CLAIM_HERO", new { walletAddress = Wallet }, true)).Value<bool>();
        }

        public override async Task<HeroProcessTokenResult> ProcessTokenRequests() {
            return (await Call("PROCESS_TOKEN_REQUESTS", new { walletAddress = Wallet }, true))
                .ToObject<HeroProcessTokenResult>();
        }

        public override async Task<bool> RandomizeHeroAbilities(int heroId) {
            return (await Call("RANDOMIZE_HERO_ABILITIES", new { walletAddress = Wallet, heroId }, true)).Value<bool>();
        }

        public override async Task<bool> ProcessHeroRandomizeAbilities(int heroId) {
            return (await Call("PROCESS_HERO_RANDOMIZE_AB", new { walletAddress = Wallet, heroId }, true)).Value<bool>();
        }

        public override async Task<bool> FusionHero(int[] heroIds) {
            return (await Call("FUSION_HERO", new { heroIds }, true)).Value<bool>();
        }

        public override async Task<bool> Fusion(int[] mainHeroIds, int[] secondHeroIds) {
            return (await Call("FUSION", new { mainHeroIds, secondHeroIds }, true)).Value<bool>();
        }

        public override async Task<bool> RepairShield(int idHeroS, int[] idHeroesBurn) {
            return (await Call("REPAIR_SHIELD", new { idHeroS, idHeroesBurn }, true)).Value<bool>();
        }

        public override async Task<int> GetRockAmount() {
            return (await Call("GET_ROCK_AMOUNT", new { walletAddress = Wallet }, false)).Value<int>();
        }

        public override async Task<string> CreateRock(int[] idHeroesBurn) {
            return (await Call("CREATE_ROCK", new { idHeroesBurn }, true)).ToString();
        }

        public override async Task<bool> RepairShieldWithRock(int idHeroS, int amountRock) {
            return (await Call("REPAIR_SHIELD_WITH_ROCK", new { idHeroS, amountRock }, true)).Value<bool>();
        }

        public override async Task<bool> UpgradeShieldLevel(int idHeroS, int amountRock) {
            return (await Call("UPGRADE_SHIELD_LEVEL", new { idHeroS, amountRock }, true)).Value<bool>();
        }

        public override async Task<bool> UpgradeShieldLevelV2(int idHero, int nonce, string signature) {
            return (await Call("UPGRADE_SHIELD_LEVEL_V2", new { idHero, nonce, signature }, true)).Value<bool>();
        }

        public override async Task<int> GetHouseLimit() {
            return (await Call("GET_HOUSE_LIMIT", null, false)).Value<int>();
        }

        public override async Task<double[]> GetHousePrice() {
            var result = (await Call("GET_HOUSE_PRICE", null, false)).ToObject<double[]>();
            for (var i = 0; i < result.Length; ++i) {
                result[i] /= CoinDecimal;
            }
            return result;
        }

        public override async Task<int[]> GetAvailableHouse() {
            return (await Call("GET_AVAILABLE_HOUSE", null, false)).ToObject<int[]>();
        }

        public override async Task<int[]> GetHouseMintLimits() {
            return (await Call("GET_HOUSE_MINT_LIMITS", null, false)).ToObject<int[]>();
        }

        public override async Task<HouseStats[]> GetHouseStats() {
            var entries = (await Call("GET_HOUSE_STATS", null, false)).ToObject<int[,]>();
            var result = new HouseStats[entries.GetLength(0)];
            for (var i = 0; i < entries.GetLength(0); ++i) {
                result[i] = new HouseStats {
                    Recovery = entries[i, 0],
                    Capacity = entries[i, 1],
                };
            }
            return result;
        }

        public override async Task<bool> BuyHouse(int rarity) {
            return (await Call("BUY_HOUSE", new { walletAddress = Wallet, rarity }, true)).Value<bool>();
        }

        public override async Task<bool> Deposit(int amount, int category) {
            return (await Call("DEPOSIT_V2", new { walletAddress = Wallet, amount, category }, true)).Value<bool>();
        }

        public override async Task<ClaimAndProcessResult> ClaimToken(double amount, int tokenType, int nonce,
            string[] details, string signature, string formatType, int waitConfirmations) {
            var r = await Call("CLAIM_TOKEN", new {
                tokenType, amount, nonce, details, signature, formatType, waitConfirmations, walletAddress = Wallet,
            }, true);
            return r.ToObject<ClaimAndProcessResult>();
        }

        public override async Task<bool> CanUseVoucher(int voucherType) {
            return (await Call("CAN_USE_VOUCHER", new { voucherType, walletAddress = Wallet }, false)).Value<bool>();
        }

        public override async Task<StakeResult> StakeToHero(int id, double amount, StakeHeroCategory category) {
            var r = await Call("STAKE_TO_HERO_V2",
                new { walletAddress = Wallet, id, amount, category = (int)category }, true);
            return r.ToObject<StakeResult>();
        }

        public override async Task<StakeResult> WithDrawFromHeroId(int id, double amount, StakeHeroCategory category) {
            var r = await Call("WITHDRAW_FROM_HERO_ID_V2", new { id, amount, category = (int)category }, true);
            return r.ToObject<StakeResult>();
        }

        public override async Task<double> GetStakeFromHeroId(int id, StakeHeroCategory category) {
            return (await Call("GET_STAKE_FROM_HERO_ID_V2", new { id, category = (int)category }, false)).Value<double>();
        }

        public override async Task<double> GetFeeFromHeroId(int id, StakeHeroCategory category) {
            return (await Call("GET_FEE_FROM_HERO_ID_V2", new { id, category = (int)category }, false)).Value<double>();
        }

        public override async Task<string> GetBridgeDeposited(string token) {
            return (await Call("BRIDGE_GET_DEPOSITED", new { walletAddress = Wallet, token }, false)).ToString();
        }

        public override async Task<string> GetBridgeWithdrawn(string token) {
            return (await Call("BRIDGE_GET_WITHDRAWN", new { walletAddress = Wallet, token }, false)).ToString();
        }

        public override async Task<BridgeTxResult> BridgeDeposit(string token, string amountWei) {
            var r = await Call("BRIDGE_DEPOSIT", new { walletAddress = Wallet, token, amountWei }, true);
            return ParseTx(r);
        }

        public override async Task<BridgeTxResult> BridgeWithdraw(string token, string grossWei, string beforeWei,
            string signature) {
            var r = await Call("BRIDGE_WITHDRAW",
                new { walletAddress = Wallet, token, grossWei, beforeWei, signature }, true);
            return ParseTx(r);
        }

        private async Task<JToken> Call(string command, object param, bool needKey) {
            var jo = await PostGame(command, param, needKey);
            return jo["result"];
        }

        private async Task<JObject> PostGame(string command, object param, bool needKey) {
            LoadConfig();
            var body = new JObject {
                ["network"] = NetworkName,
                ["command"] = command,
                ["param"] = param != null ? JObject.FromObject(param) : new JObject(),
            };
            if (needKey) {
                if (string.IsNullOrEmpty(_privateKey)) {
                    throw new Exception(
                        "Bridge: throwaway testnet privateKey is empty (set it in Tools ▸ Bridge Debug Panel).");
                }
                body["privateKey"] = _privateKey;
            }

            var content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
            var resp = await Http.PostAsync(_proxyUrl.TrimEnd('/') + "/game", content);
            var text = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) {
                throw new Exception($"editor-chain-proxy {(int)resp.StatusCode}: {text}");
            }
            return JObject.Parse(text);
        }

        private static BridgeTxResult ParseTx(JToken result) {
            var status = result?["status"];
            return new BridgeTxResult {
                txHash = result?["txHash"]?.ToString() ?? "",
                success = status != null && status.Type == JTokenType.Integer && (int)status == 1,
            };
        }
    }
}
#endif
