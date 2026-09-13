using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLPvpMode.Manager.Api;
using CustomSmartFox;
using CustomSmartFox.SolCommands;
using Game.Dialog;
using Senspark;
using Sfs2X.Entities.Data;
using Utils;

namespace App.BomberLand {
    public partial class DefaultGeneralServerBridge : IGeneralServerBridge {
        private readonly IStorageManager _storageManager;
        private readonly IChestRewardManager _chestRewardManager;
        private readonly IBHeroManager _bHeroManager;
        private readonly IHouseStorageManager _houseStorageManager;
        private readonly IClaimTokenServerBridge _claimTokenServerBridge;
        private readonly ILogManager _logManager;
        private readonly IServerDispatcher _serverDispatcher;
        private readonly ISmartFoxApi _api;
        private readonly IExtensionRequestBuilder _requestBuilder;
        private readonly ITaskDelay _taskDelay;
        private readonly ICacheRequestManager _cacheRequestManager;
        private readonly ITaskTonManager _taskTonManager;
        private readonly IOnBoardingManager _onBoardingManager;
        private readonly IDedupServerRequester _dedup;
        private readonly bool _enableLog;

        public DefaultGeneralServerBridge(
            bool enableLog,
            IStorageManager storageManager,
            IBHeroManager bHeroManager,
            IHouseStorageManager houseStorageManager,
            IChestRewardManager chestRewardManager,
            IClaimTokenServerBridge claimTokenServerBridge,
            ILogManager logManager,
            IServerDispatcher serverDispatcher,
            ISmartFoxApi api,
            IExtensionRequestBuilder requestBuilder,
            ITaskDelay taskDelay,
            ICacheRequestManager cacheRequestManager,
            ITaskTonManager taskTonManager,
            IOnBoardingManager onBoardingManager,
            IDedupServerRequester dedup
        ) {
            _enableLog = enableLog;
            _serverDispatcher = serverDispatcher;
            _storageManager = storageManager;
            _bHeroManager = bHeroManager;
            _houseStorageManager = houseStorageManager;
            _logManager = logManager;
            _chestRewardManager = chestRewardManager;
            _claimTokenServerBridge = claimTokenServerBridge;
            _api = api;
            _requestBuilder = requestBuilder;
            _taskDelay = taskDelay;
            _cacheRequestManager = cacheRequestManager;
            _taskTonManager = taskTonManager;
            _onBoardingManager = onBoardingManager;
            _dedup = dedup;
        }
        
        public async Task<IHeroPower[]> GetHeroPower() {
            var data = new SFSObject();
            
            var response = await _serverDispatcher.SendCmd(new CmdGetHeroUpgradePower(data));
            return OnGetHeroPower(response);
        }
        
        public async Task<ISyncHeroResponse> SyncHero(bool notifyNewIds, bool isBuyHero = false, bool forceFresh = false) {
            var data = new SFSObject();
            if (forceFresh) {
                data.PutBool(SFSDefine.SFSField.ForceFresh, true);
            }

            var response = await _serverDispatcher.SendCmd(new CmdSyncBomberMan(data));
            return OnSyncHero(response, notifyNewIds, isBuyHero);
        }

        public ISyncHeroResponse SyncHero(ISFSObject data) {
            var result = OnSyncHero(data, true, false);
            _serverDispatcher.DispatchEvent(observer => observer.OnBlockchainDataRefreshed?.Invoke());
            return result;
        }
        
        public async Task<ISyncHouseResponse> SyncHouse() {
            var data = new SFSObject();
            
            var response = await _serverDispatcher.SendCmd(new CmdSyncHouse(data));
            return OnSyncHouseServer(response);
        }

        public ISyncHouseResponse SyncHouse(ISFSObject data) {
            var result = OnSyncHouseServer(data);
            _serverDispatcher.DispatchEvent(observer => observer.OnBlockchainDataRefreshed?.Invoke());
            return result;
        }
        
        public Task<IChestReward> GetChestReward()
            => _dedup.Dedup<IChestReward>(SFSDefine.SFSCommand.GET_REWARD_V2, async () => {
                var data = new SFSObject();

                var response = await _serverDispatcher.SendCmd(new CmdGetReward(data));
                return OnGetChestReward(response);
            });
        
        public async Task<bool> ReactiveHouse(int houseId) {
            var data = new SFSObject();
            data.PutInt("house_id", houseId);
            
            var response = await _serverDispatcher.SendCmd(new CmdReactiveHouse(data));
            return OnReactiveHouse(response);
        }
        
        private const int ClaimPushTimeoutMs = 90_000;
        private TaskCompletionSource<ISFSObject> _pendingClaimTcs;

        public async Task<IApproveClaimResponse> ApproveClaim(int code) {
            if (_pendingClaimTcs != null) {
                throw new InvalidOperationException("APPROVE_CLAIM_V4 already in-flight");
            }
            _pendingClaimTcs = new TaskCompletionSource<ISFSObject>();
            try {
                var data = new SFSObject().Apply(it => { it.PutInt("block_reward_type", code); });
                var ack = await _serverDispatcher.SendCmd(new CmdApproveClaim(data));
                var status = ack?.GetUtfString("status");
                if (status != "PENDING") {
                    throw new Exception($"Unexpected APPROVE_CLAIM_V4 ack status: {status ?? "<null>"}");
                }
                _logManager.Log($"APPROVE_CLAIM_V4 pending correlationId={ack.GetUtfString("correlationId")}");

                var push = await _pendingClaimTcs.TimeoutAfter(ClaimPushTimeoutMs);
                return OnApproveClaim(push);
            } finally {
                _pendingClaimTcs = null;
            }
        }

        public void OnApproveClaimPush(ISFSObject data) {
            var code = data != null && data.ContainsKey("code") ? data.GetInt("code") : 0;
            if (code != 0) {
                var message = data != null && data.ContainsKey("message") ? data.GetUtfString("message") : "<none>";
                _logManager.Log($"APPROVE_CLAIM_RESPONSE error code={code} message={message}");
                _pendingClaimTcs?.TrySetException(new ClaimServerErrorException(message));
                return;
            }
            _pendingClaimTcs?.TrySetResult(data);
        }

        public void OnUpgradeShieldLevelPush(ISFSObject data) {
            OnUpgradeShieldLevelResponseHandler(data);
        }

        public void OnHeroStakePush(ISFSObject data) {
            OnHeroStakePushHandler(data);
        }

        public void CancelPendingClaim(Exception reason) {
            _pendingClaimTcs?.TrySetException(reason);
        }

        // view-call + sign only, no tx mined.
        private const int BridgeWithdrawTimeoutMs = 15_000;

        public async Task<BridgeWithdrawResult> RequestCrosschainBridgeWithdraw(int blockRewardType, string chain) {
            var data = new SFSObject();
            data.PutInt("block_reward_type", blockRewardType);
            if (!string.IsNullOrEmpty(chain)) {
                data.PutUtfString(SFSDefine.SFSField.Chain, chain);
            }
            var response = await _serverDispatcher.SendCmd(new CmdCrosschainDepositBridgeWithdraw(data))
                .TimeoutAfter(BridgeWithdrawTimeoutMs);
            return OnCrosschainBridgeWithdraw(response);
        }

        // Fire-and-forget bridge activity report (before/after deposit, after withdraw). The server re-emits it
        // so the indexer accelerates its sweep; there's no business result, we just await the ack. Wallet is
        // taken server-side from the session. Callers should not block the on-chain flow on this.
        public async Task NotifyCrosschainBridge(string kind, int blockRewardType, string chain, string txHash = null) {
            var data = new SFSObject();
            data.PutUtfString(SFSDefine.SFSField.Kind, kind);
            data.PutInt("block_reward_type", blockRewardType);
            if (!string.IsNullOrEmpty(chain)) {
                data.PutUtfString(SFSDefine.SFSField.Chain, chain);
            }
            if (!string.IsNullOrEmpty(txHash)) {
                data.PutUtfString(SFSDefine.SFSField.TxHash, txHash);
            }
            await _serverDispatcher.SendCmd(new CmdCrosschainDepositBridgeNotify(data))
                .TimeoutAfter(BridgeWithdrawTimeoutMs);
        }

        // User-relayed native (BNB / POL) withdraw-MAX. Synchronous request-response: the server reads the
        // on-chain counters, runs the row-locked request, signs, and replies on the request itself. The
        // reward type (BNB=BSC, POL=POLYGON) implies the network server-side, so no chain param is sent.
        public async Task<NativeWithdrawResult> RequestNativeWithdraw(int blockRewardType) {
            var data = new SFSObject();
            data.PutInt("block_reward_type", blockRewardType);
            var response = await _serverDispatcher.SendCmd(new CmdWithdrawNative(data))
                .TimeoutAfter(BridgeWithdrawTimeoutMs);
            return new NativeWithdrawResult(response);
        }

        // Client hint to sync native deposits fast (optional — the server also syncs on login and via the
        // background reconciler, so correctness never depends on this arriving). Fire after a deposit or a
        // relayed withdraw so the balance refreshes promptly; the updated rewards[] flow through the normal path.
        public async Task SyncNativeDeposit(int blockRewardType) {
            var data = new SFSObject();
            data.PutInt("block_reward_type", blockRewardType);
            await _serverDispatcher.SendCmd(new CmdSyncNativeDeposit(data))
                .TimeoutAfter(BridgeWithdrawTimeoutMs);
        }

        public async Task<float> ConfirmApproveClaimSuccess(int code) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("block_reward_type", code);
            });

            var response = await _serverDispatcher.SendCmd(new CmdConfirmClaimRewardSuccess(data));
            return OnConfirmApproveClaimSuccess(response);
        }
        
        public async Task<IChestReward> SyncDeposited(DepositSyncTarget target = DepositSyncTarget.Both) {
            var data = new SFSObject();
            if (target != DepositSyncTarget.Both) {
                data.PutUtfString("deposit_sync_target", target == DepositSyncTarget.Bridge ? "BRIDGE" : "OLD");
            }

            var response = await _serverDispatcher.SendCmd(new CmdSyncDeposited(data));
            return OnSyncDeposited(response);
        }

        public IChestReward SyncDeposited(ISFSObject data) {
            var result = OnSyncDeposited(data);
            _serverDispatcher.DispatchEvent(observer => observer.OnBlockchainDataRefreshed?.Invoke());
            return result;
        }

        public void OnHouseRentalUpdatePush(ISFSObject data) {
            OnHouseRentalUpdate(data);
            _serverDispatcher.DispatchEvent(observer => observer.OnBlockchainDataRefreshed?.Invoke());

            // A rented house enters/leaves the player's list along with the
            // contract, and the server is what decides that: a sync brings the
            // list already correct.
            SyncHouse().Forget();
        }
        
        // V3 carries prices[] and is only wired up on BSC / POLYGON; the airdrop chains stay on V2, whose
        // response has no native entry to render anyway.
        public async Task<IAutoMinePackages> GetAutoMinePrice() {
            var data = new SFSObject();

            var response = AppConfig.IsAirDrop()
                ? await _serverDispatcher.SendCmd(new CmdAutoMinePrice(data))
                : await _serverDispatcher.SendCmd(new CmdAutoMinePriceV3(data));
            return OnGetAutoMinePrice(response);
        }

        public async Task<IChestReward> BuyAutoMine(string packageName, BlockRewardType blockRewardType) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("package", packageName);
                it.PutInt("reward_type", ServerRewardTypes.ToWire(blockRewardType));
            });

            var response = await _serverDispatcher.SendCmd(new CmdBuyAutoMine(data));
            return OnBuyAutoMine(response);
        }

        public async Task<IRockPackage> BuyRockPack(string packageName, BlockRewardType blockRewardType) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("package", packageName);
                it.PutInt("reward_type", ServerRewardTypes.ToWire(blockRewardType));
            });

            var response = await _serverDispatcher.SendCmd(new CmdBuyRock(data));
            return OnBuyRockPack(response);
        }

        public async Task<IChestReward> BuyGemByNativeToken(string productId) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("product_id", productId);
            });

            var response = await _serverDispatcher.SendCmd(new CmdBuyGemByNativeToken(data));
            return OnBuyGemByNativeToken(response);
        }
        
        public async Task<bool> StartAutoMine() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdStartAutoMine(data));
            return OnStartAutoMine(response);
        }
        
        public async Task RepairShield(HeroId heroId, BlockRewardType blockRewardType) {
            var data = new SFSObject().Apply(it => {
                var rewardType = blockRewardType switch {
                    BlockRewardType.Senspark => 7,
                    BlockRewardType.BCoinDeposited => 4,
                    BlockRewardType.Rock => 23,
                    _ => throw new Exception("Invalid Request")
                };
                it.PutInt("hero_id", heroId.Id);
                it.PutInt(SFSDefine.SFSField.HeroType, (int)heroId.Type);
                it.PutInt("reward_type", rewardType);
            });

            var response = await _serverDispatcher.SendCmd(new CmdRepairShield(data));
            OnRepairShield(response);
        }

        public async Task<IRepairShieldBatchResponse> RepairShieldBatch(int[] heroIds, BlockRewardType blockRewardType) {
            var data = new SFSObject().Apply(it => {
                var rewardType = blockRewardType switch {
                    BlockRewardType.Senspark => 7,
                    BlockRewardType.BCoinDeposited => 4,
                    BlockRewardType.Rock => 23,
                    _ => throw new Exception("Invalid Request")
                };
                it.PutIntArray("hero_list", heroIds);
                it.PutInt("reward_type", rewardType);
            });

            var response = await _serverDispatcher.SendCmd(new CmdRepairShieldBatch(data));
            return OnRepairShieldBatch(response);
        }
        
        public async Task<bool> ChangeMiningToken(string type, double bcoinInWallet) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("token_type", type);
                it.PutFloat("bcoin_in_wallet", (float)bcoinInWallet);
            });

            var response = await _serverDispatcher.SendCmd(new CmdChangeMiningToken(data));
            return OnChangeMiningToken(response);
        }
        
        public async Task<IAirDropResponse> GetAirDrop() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetAirDrop(data));
            return OnGetAirDrop(response);
        }

        public async Task<IAirDropClaimResponse> ClaimAirDrop(string nftCode) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("code", nftCode);
            });

            var response = await _serverDispatcher.SendCmd(new CmdClaimAirDrop(data));
            return OnClaimAirDrop(response);
        }

        public async Task<bool> ConfirmClaimAirDrop(string nftCode) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("code", nftCode);
            });

            var response = await _serverDispatcher.SendCmd(new CmdConfirmClaimAirDrop(data));
            return OnConfirmClaimAirDrop(response);
        }
        
        public async Task<bool> UserRename(string newName, BlockRewardType type) {
            if (newName.Length < 4 || newName.Length > 10) {
                throw new Exception("Invalid name");
            }
            var data = new SFSObject().Apply(it => {
                var rewardType = type switch {
                    BlockRewardType.BCoin => 1,
                    BlockRewardType.BCoinDeposited => 1,
                    BlockRewardType.Senspark => 7,
                    _ => throw new Exception("Invalid Request")
                };
                it.PutUtfString("name", newName);
                it.PutInt("reward_type", rewardType);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdUserRename(data));
            return OnUserRename(response);
        }

        public async Task<bool> LinkUser(UserAccount account) {
            try {
                var data = new SFSObject().Apply(it => {
                    it.PutUtfString("username", account.userName);
                    it.PutUtfString("password", account.password);
                    it.PutUtfString("email", account.email);
                });

                var response = await _serverDispatcher.SendCmd(new CmdUserLink(data));
                return OnLinkUser(response);
            } catch (Exception e) {
                _logManager.Log($"Link account error: {e.Message}");
                return false;
            }

        }
        
        public async Task<double> Stake(float bcoin, BlockRewardType type, bool stakeAll) {
            if (bcoin <= 0) {
                throw new Exception("Invalid value");
            }
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("block_reward_type", RewardUtils.ConvertToBlockRewardType(type));
                it.PutFloat("amount", bcoin);
                it.PutBool("all_in", stakeAll);
            });

            var response = await _serverDispatcher.SendCmd(new CmdUserStake(data));
            return OnUserStake(response);
        }
        
        public async Task<IStakeResult> GetStakeInfo() {
            return await UserWithdrawStakeCmd(false);
        }

        public async Task<IStakeResult> WithdrawStake() {
            return await UserWithdrawStakeCmd(true);
        }

        private async Task<IStakeResult> UserWithdrawStakeCmd(bool isWithdraw) {
            var data = new SFSObject().Apply(it => {
                it.PutBool("is_withdraw", isWithdraw);
            });

            var response = await _serverDispatcher.SendCmd(new CmdUserWithdrawStake(data));
            return OnUserWithdrawStake(response);
        }
        
        public async Task<IVipStakeResponse> UserStakeVip() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdUserStakeVip(data));
            return OnUserStakeVip(response);
        }
        
        public async Task<IVipStakeResponse> UserClaimStakeVip(VipRewardType type) {
            var data = new SFSObject().Apply(it => {
                var (p1, p2) = type switch {
                    VipRewardType.Hero => ("REWARD", "BOMBERMAN"),
                    VipRewardType.Shield => ("BOOSTER", "SHIELD"),
                    VipRewardType.ConquestCard => ("BOOSTER", "CONQUEST_CARD"),
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
                it.PutUtfString("rewardType", p1);
                it.PutUtfString("type", p2);
            });

            var response = await _serverDispatcher.SendCmd(new CmdUserClaimStakeVip(data));
            return OnUserClaimStakeVip(response);
        }
        
        public async Task<ILuckyRewardsResponse> StartLuckWheel(int amount) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("round", amount);
            });

            var response = await _serverDispatcher.SendCmd(new CmdStartLuckyWheel(data));
            return OnStartLuckyWheel(response);
        }
        
        public async Task<IDailyMissionListResponse> GetDailyMissions() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetDailyMission(data));
            return OnGetDailyMissions(response);
        }
        
        public async Task<bool> ClaimLuckyTicketDailyMission(string missionCode) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("code", missionCode);
            });

            var response = await _serverDispatcher.SendCmd(new CmdClaimLuckyTicketDailyMission(data));
            return OnClaimLuckyTicketDailyMission(response);
        }
        
        public async Task<IEmailResponse> GetEmail() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetEmail(data));
            return OnGetEmail(response);
        }

        public async Task<bool> RegisterEmail(string email) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("email", email);
            });

            var response = await _serverDispatcher.SendCmd(new CmdRegisterEmail(data));
            return OnRegisterEmail(response);
        }
        
        public async Task<bool> VerifyEmail(int verifyCode) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("verifyCode", verifyCode);
            });

            var response = await _serverDispatcher.SendCmd(new CmdVerifyEmail(data));
            return OnVerifyEmail(response);
        }
        
        public async Task<IChestReward> BuyTicket(BlockRewardType currency, int amount, BlockRewardType ticketType) {
            var data = new SFSObject().Apply(it => {
                var t = currency switch {
                    BlockRewardType.BCoinDeposited => 4,
                    BlockRewardType.Senspark => 7,
                    _ => throw new Exception("Failed"),
                };
                it.PutInt("type", t);
                it.PutInt("number", amount);
            });
            IExtCmd<ISFSObject> cmd = ticketType switch {
                BlockRewardType.BossTicket => new CmdBuyBossTicket(data),
                BlockRewardType.PvpTicket => new CmdBuyPvpTicket(data),
                _ => throw new Exception("Wrong ticket type")
            };

            var response = await _serverDispatcher.SendCmd(cmd);
            return OnBuyTicket(response);
        }
        
        public async Task BuyLuckyTicket(int amount) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("quantity", amount);
            });

            var response = await _serverDispatcher.SendCmd(new CmdBuyLuckyTicket(data));
            OnBuyLuckyTicket(response);
        }
        
        public async Task<IServerConfigResponse> GetConfig() {
            var data = new SFSObject();

            var serverManager = (IServerManager)_serverDispatcher;
            var response = await _serverDispatcher.SendCmd(new CmdGetStartGameConfig(data));
            return OnGetConfig(response);
        }
        
        public async Task<float> PreviewToken(float balance, NetworkType networkType, int tokenType) {
            var data = new SFSObject().Apply(it => {
                it.PutFloat("balance", balance);
                it.PutInt("network_type", (int)networkType);
                it.PutInt("token_type", tokenType);
            });

            var response = await _serverDispatcher.SendCmd(new CmdPreviewToken(data));
            return OnPreviewOrSwapToken(response);
        }

        public async Task<float> SwapToken(float balance, NetworkType networkType, int tokenType) {
            var data = new SFSObject().Apply(it => {
                it.PutFloat("balance", balance);
                it.PutInt("network_type", (int)networkType);
                it.PutInt("token_type", tokenType);
            });

            var response = await _serverDispatcher.SendCmd(new CmdSwapToken(data));
            return OnPreviewOrSwapToken(response);
        }

        public async Task<int> GetSwapTokenConfig() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetSwapTokenConfig(data));
            return OnGetSwapTokenConfig(response);
        }
        
        public void KeepJoiningPvPQueue() {
            var data = new SFSObject();

            var response = _serverDispatcher.SendCmd(new CmdKeepJoiningPvpQueue(data));
        }

        public async Task<IMinStakeHeroManager> GetMinStakeHero() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetMinStakeHero(data));
            return OnGetMinStakeManager(response);
        }

        public async Task<IRepairShieldConfig> GetRepairShieldConfig() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetRepairShieldConfig(data));
            return OnGetRepairShieldConfig(response);
        }

        public async Task GetRockPackConfig() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetRockPackConfigV3(data));
            OnGetRockPackConfig(response);
        }

        public async Task GetRentHousePackageConfig() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetRentHousePackageConfig(data));
            OnGetRentHousePackageConfig(response);
        }
        
        public async Task<ITreasureHuntConfigResponse> GetTreasureHuntDataConfig() {
            var data = new SFSObject();
            
            var response = await _serverDispatcher.SendCmd(new CmdGetDataThConfig(data));
            return OnGetTreasureHuntDataConfig(response);
        }

        public async Task<IBurnHeroConfig> GetBurnHeroConfig() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetBurnHeroConfig(data));
            return OnGetBurnHeroConfig(response);
        }

        public async Task BurnHero(string tx, HeroId[] heroIds) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("tx", tx);
                it.PutIntArray("listIdHero", heroIds.Select(hero => hero.Id).ToArray());
            });

            var response = await _serverDispatcher.SendCmd(new CmdCreateRock(data));
            OnBurnHero(response);
        }

        public async Task<IUpgradeShieldConfig> GetUpgradeShieldConfig() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetUpgradeShieldConfig(data));
            return OnGetUpgradeShieldConfig(response);
        }

        public async Task<IUpgradeShieldResponse> UpgradeLevelShield(HeroId heroId) {
            var data = new SFSObject();
            data.PutInt("heroId", heroId.Id);
#if UNITY_EDITOR
            data.PutBool("debug_fake_push", true);
#endif

            var response = await _serverDispatcher.SendCmd(new CmdUpgradeShieldLevelV3(data));
            return OnUpgradeLevelShield(response);
        }
        
        public void UpdateUserReward(ISFSObject data) {
            ParseChestReward(data);
        }
        
        public async Task<IBuyHeroServerResponse> BuyHeroServer(int quantity, int rewardType, bool isBuyHero) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("quantity", quantity);
                it.PutInt("reward_type", rewardType);
            });

            var response = await _serverDispatcher.SendCmd(new CmdBuyHeroServer(data));
            return OnBuyHeroServer(response, isBuyHero);
        }
        
        public async Task<IHouseDetails> BuyHouseServer(int rarity, int rewardType) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("rarity", rarity);
                it.PutInt("reward_type", rewardType);
            });
        
            var response = await _serverDispatcher.SendCmd(new CmdBuyHouseServer(data));
            return OnBuyHouseServer(response);
        }
        
        public async Task<IOfflineReward> GetOfflineReward() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetOfflineThModeReward(data));
            return OnGetOfflineReward(response);
        }
        
        public IReferralData InitReferralData(ISFSObject data) {
            return OnGetReferralData(data);
        }

        public async Task<IClubInfo> GetClubInfo() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetClubInfo(data));
            return OnGetClubInfo(response);
        }

        public async Task<IClubInfo> GetClubInfo(long clubId) {
            var data = new SFSObject().Apply(it => {
                it.PutLong("club_id", clubId);
            });

            var response = await _serverDispatcher.SendCmd(new CmdGetClubInfo(data));
            return OnGetClubInfo(response);
        }

        public async Task<IClubRank[]> GetAllClub() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetAllClub(data));
            return OnGetListClubRank(response);
        }

        public async Task<IClubInfo> JoinClub(long clubId) {
            var data = new SFSObject().Apply(it => {
                it.PutLong("club_id", clubId);
            });

            var response = await _serverDispatcher.SendCmd(new CmdJoinClub(data));
            return OnJoinClubInfo(response);
        }
        
        public async Task<IClubInfo> JoinAnotherClub(string clubName) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("club_name", clubName);
            });

            var response = await _serverDispatcher.SendCmd(new CmdJoinAnotherClub(data));
            return OnJoinClubInfo(response);
        }
        
        public async Task<IClubInfo> CreateClub(string clubName) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("club_name", clubName);
            });

            var response = await _serverDispatcher.SendCmd(new CmdCreateClub(data));
            return OnGetClubInfo(response);
        }

        public async Task LeaveClub() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdLeaveClub(data));
            OnLeaveClubInfo(response);
        }

        public async Task<IClubBidPrice[]> GetBidPrice(long clubId) {
            var data = new SFSObject().Apply(it => {
                it.PutLong("club_id", clubId);
            });

            var response = await _serverDispatcher.SendCmd(new CmdGetBidPrice(data));
            return OnGetBidPrice(response);
        }

        public async Task<IClubRank[]> GetTopBidClub() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetTopBidClub(data));
            return OnGetListClubRank(response);
        }

        public async Task<IClubBidPrice[]> BoostClub(long clubId, int packageId) {
            var data = new SFSObject().Apply(it => {
                it.PutLong("club_id", clubId);
                it.PutInt("package_id", packageId);
            });

            var response = await _serverDispatcher.SendCmd(new CmdBoostClub(data));
            return OnGetBidPrice(response);
        }

        public async Task<string> GetInvoiceSol(double amount, DepositType depositType) {
            var data = new SFSObject().Apply(it => {
                it.PutDouble("amount", amount);
                it.PutInt("deposit_type", (int) depositType);
            });

            var response =  await _serverDispatcher.SendCmd(new CmdGetInvoiceDeposit(data));
            return OnDepositSolana(response);
        }

        public async Task<ISyncHeroResponse> AddHeroForAirdropUser() {
            var data = new SFSObject();
            var response = await _serverDispatcher.SendCmd(new CmdAddHero(data));
            return OnSyncHeroServer(response);
        }
        
        public async Task<IFusionTonHeroResponse> FusionHeroServer(int target, int[] heroList) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("target", target);
                it.PutIntArray("hero_list", heroList);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdFusionHero(data));
            var res = OnFusionServerHero(response);
            var heroType = GetHeroType();
            var heroIds = heroList.Select(id => new HeroId(id, heroType));
            _serverDispatcher.DispatchEvent(e => e.OnRemoveHeroes?.Invoke(heroIds.ToArray()));
            return res;
        }
        
        public async Task<IFusionTonHeroResponse> MultiFusionHeroServer(int target, int[] heroList) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("target", target);
                it.PutIntArray("hero_list", heroList);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdMultiFusionHero(data));
            var res = OnMultiFusionServerHero(response);
            var heroType = GetHeroType();
            var heroIds = heroList.Select(id => new HeroId(id, heroType));
            _serverDispatcher.DispatchEvent(e => e.OnRemoveHeroes?.Invoke(heroIds.ToArray()));
            return res;
        }
        
        private HeroAccountType GetHeroType() {
            //DevHoang: Add new airdrop
            if (AppConfig.IsTon()) {
                return HeroAccountType.Ton;
            }
            if (AppConfig.IsSolana()) {
                return HeroAccountType.Sol;
            }
            if (AppConfig.IsRonin()) {
                return HeroAccountType.Ron;
            }
            if (AppConfig.IsBase()) {
                return HeroAccountType.Bas;
            }
            if (AppConfig.IsViction()) {
                return HeroAccountType.Vic;
            }
            return HeroAccountType.Nft;
        }

        public async Task GetHeroOldSeason() {
            var data = new SFSObject();
            var response = await _serverDispatcher.SendCmd(new CmdGetHeroOldSeason(data));
            OnGetHeroOldSeason(response);
        }
        public async Task GetOnBoardingConfig() {
            var data = new SFSObject();
            var response = await _serverDispatcher.SendCmd(new CmdGetOnBoardingConfig(data));
            OnGetOnBoardingConfig(response);
        }
        
        public async Task UpdateUserOnBoarding(int step, int claimed = 0) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("step", step);
                if (claimed != 0) {
                    it.PutInt("claimed", claimed);
                }
            });
            var response = await _serverDispatcher.SendCmd(new CmdUpdateUserOnBoarding(data));
            OnUpdateUserOnBoarding(response);
        }

        public async Task<long> RentHouse(int houseId, int numDays) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("house_id", houseId);
                it.PutInt("num_days", numDays);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdRentHouse(data));
            var endTimeRent = OnRentHouse(response);
            _houseStorageManager.UpdateHouseRentTime(houseId, endTimeRent);
            return endTimeRent;
        }

        public void SendClientLog(List<ClientLogEntry> logData) {
            var logArray = new SFSArray();
            foreach (var log in logData) {
                var logEntry = new SFSObject().Apply(it => {
                    it.PutInt("type", (int)log.LogType);
                    it.PutUtfString("message", log.LogMessage);
                });
                logArray.AddSFSObject(logEntry);
            }
            
            var data = new SFSObject().Apply(it => {
                it.PutSFSArray("data", logArray);
            });

            _serverDispatcher.SendCmd(new CmdSendClientLog(data));
        }
        
        public async Task<string> GetInvoice(double amount, DepositType depositType) {
            var data = new SFSObject();
            data.PutInt("deposit_type", (int) depositType);
            IExtCmd<ISFSObject> cmd = null;
            switch (depositType) {
                //DevHoang: Add new airdrop
                case DepositType.RON_DEPOSIT:
                    cmd = new CmdGetInvoiceDepositRon(data);
                    break;
                case DepositType.BAS_DEPOSIT:
                    cmd = new CmdGetInvoiceDepositBas(data);
                    break;
                case DepositType.VIC_DEPOSIT:
                    cmd = new CmdGetInvoiceDepositVic(data);
                    break;
            }
            var response = await _serverDispatcher.SendCmd(cmd);
            return OnDepositRon(response);
        }
    }
}