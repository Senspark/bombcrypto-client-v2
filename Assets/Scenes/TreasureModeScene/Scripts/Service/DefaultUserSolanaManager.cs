using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using App.BomberLand;
using CustomSmartFox;
using Game.Dialog;
using PvpMode.Services;
using Senspark;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Share.Scripts.Communicate;
using UnityEngine;

namespace Scenes.TreasureModeScene.Scripts.Service {
    public class UserSolanaObserver {
        public Action<bool> OnDepositComplete;
        public Action<ISFSObject> OnDepositResponse;
    }
    
    public class UserSolanaEventTrigger: ObserverManager<UserSolanaObserver>, IUserSolanaEventTrigger{
        
    }
    
    public class DefaultUserSolanaManager : IUserSolanaManager {
        readonly IUserSolanaManager _userSolanaManager;

        public DefaultUserSolanaManager(
            IMasterUnityCommunication unityCommunication,
            ILogManager logManager,
            IGeneralServerBridge general,
            IPveServerBridge pve,
            IPvpServerBridge pvp,
            IExtResponseEncoder encoder)
        {
            if (AppConfig.IsSolana() || AppConfig.IsWebAirdrop()) {
                _userSolanaManager = new UserSolanaManager(unityCommunication, logManager, general, pve, pvp, encoder);
            } 
            else {
                _userSolanaManager = new NullUserSolana();
            }
        }

        public IUserSolanaEventTrigger EventTrigger => _userSolanaManager.EventTrigger;

        public void LogoutUserSolana() {
            _userSolanaManager.LogoutUserSolana();
        }
        
        public async Task<string> GetInvoice(double amount, DepositType depositType) {
            return await _userSolanaManager.GetInvoice(amount, depositType);
        }
        
        public async Task<bool> DepositSol(string invoice, double amount, DepositType depositType) {
            return await _userSolanaManager.DepositSol(invoice, amount, depositType);
        }
        
        public async Task<ISyncHeroResponse> AddHeroForSolUser() {
            return await _userSolanaManager.AddHeroForSolUser();
        }
        
        public async Task<IBuyHeroServerResponse> BuyHeroSol(int quantity, int rewardType, bool setBuyHero = false) {
            return await _userSolanaManager.BuyHeroSol(quantity, rewardType, setBuyHero);
        }
        
        public async Task<IHouseDetails> BuyHouseSol(int rarity, int rewardType) {
            return await _userSolanaManager.BuyHouseSol(rarity, rewardType);
        }
        
        public async Task<IAutoMinePackages> GetAutoMinePriceSol() {
            return await _userSolanaManager.GetAutoMinePriceSol();
        }
        
        public async Task<IMapDetails> GetMapDetailsSol() {
            return await _userSolanaManager.GetMapDetailsSol();
        }
        
        public async Task<ICoinLeaderboardConfigResult[]> GetCoinLeaderboardConfigSol() {
            return await _userSolanaManager.GetCoinLeaderboardConfigSol();
        }
        
        public async Task<ICoinRankingResult> GetCoinRankingSol() {
            return await _userSolanaManager.GetCoinRankingSol();
        }
        
        public async Task<ICoinRankingResult> GetAllSeasonCoinRankingSol() {
            return await _userSolanaManager.GetAllSeasonCoinRankingSol();
        }
        
        public Task GetRentHousePackageConfig() {
            return _userSolanaManager.GetRentHousePackageConfig();
        }
        
        public async Task<ITreasureHuntConfigResponse> GetTreasureHuntDataConfigSol() {
            return await _userSolanaManager.GetTreasureHuntDataConfigSol();
        }
        
        public async Task<ISyncHouseResponse> SyncHouseSol() {
            return await _userSolanaManager.SyncHouseSol();
        }
        
        public async Task<IFusionTonHeroResponse> FusionServer(int target, int[] heroList) {
            return await _userSolanaManager.FusionServer(target, heroList);
        }
        
        public async Task<IFusionTonHeroResponse> MultiFusionServer(int target, int[] heroList) {
            return await _userSolanaManager.MultiFusionServer(target, heroList);
        }
        
        public void StartExplodeSol(GameModeType type, HeroId heroId, int bombId, Vector2Int tileLocation, List<Vector2Int> brokenList) {
            _userSolanaManager.StartExplodeSol(type, heroId, bombId, tileLocation, brokenList);
        }
        
        public async Task<bool> StartAutoMineSol() {
            return await _userSolanaManager.StartAutoMineSol();
        }
        
        public async Task<IChestReward> BuyAutoMineSol(string packageName, BlockRewardType blockRewardType) {
            return await _userSolanaManager.BuyAutoMineSol(packageName, blockRewardType);
        }
        
        public void GoHomeSol(HeroId id) {
            _userSolanaManager.GoHomeSol(id);
        }
        
        public void GoWorkSol(HeroId id) {
            _userSolanaManager.GoWorkSol(id);
        }
        
        public void GoSleepSol(HeroId id) {
            _userSolanaManager.GoSleepSol(id);
        }
        
        public async Task ChangeBomberManStageSol(HeroId[] ids, HeroStage stage) {
            await _userSolanaManager.ChangeBomberManStageSol(ids, stage);
        }
        
        public async Task<bool> ActiveBomberSol(HeroId id, int value) {
            return await _userSolanaManager.ActiveBomberSol(id, value);
        }
        
        public async Task<bool> ActiveBomberHouseSol(string genId, int houseId) {
            return await _userSolanaManager.ActiveBomberHouseSol(genId, houseId);
        }
        
        public async Task<bool> GetActiveBomberSol() {
            return await _userSolanaManager.GetActiveBomberSol();
        }
        
        public async Task<IStartPveResponse> StartPvESol(GameModeType type) {
            return await _userSolanaManager.StartPvESol(type);
        }

        public async Task<IInvestedDetail> StopPvESol() {
            return await _userSolanaManager.StopPvESol();
        }

        public Task<IChestReward> GetChestRewardSol() {
            return _userSolanaManager.GetChestRewardSol();
        }
        
        public Task<bool> ReactiveHouseSol(int houseId) {
            return _userSolanaManager.ReactiveHouseSol(houseId);
        }
        
        public Task<bool> Initialize() {
            return _userSolanaManager.Initialize();
        }

        public void Destroy() {
            _userSolanaManager.Destroy();
        }

        public void OnConnection() {
            _userSolanaManager.OnConnection();
        }

        public void OnConnectionError(string message) {
            _userSolanaManager.OnConnectionError(message);
        }

        public void OnConnectionRetry() {
            _userSolanaManager.OnConnectionRetry();
        }

        public void OnConnectionResume() {
            _userSolanaManager.OnConnectionResume();
        }

        public void OnConnectionLost(string reason) {
            _userSolanaManager.OnConnectionLost(reason);
        }

        public void OnLogin() {
            _userSolanaManager.OnLogin();
        }

        public void OnLoginError(int code, string message) {
            _userSolanaManager.OnLoginError(code, message);
        }

        public void OnUdpInit(bool success) {
            _userSolanaManager.OnUdpInit(success);
        }

        public void OnPingPong(int lagValue) {
            _userSolanaManager.OnPingPong(lagValue);
        }

        public void OnRoomVariableUpdate(SFSRoom room) {
            _userSolanaManager.OnRoomVariableUpdate(room);
        }

        public void OnJoinRoom(SFSRoom room) {
            _userSolanaManager.OnJoinRoom(room);
        }

        public void OnExtensionResponse(string cmd, ISFSObject value) {
            _userSolanaManager.OnExtensionResponse(cmd, value);
        }

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
            _userSolanaManager.OnExtensionResponse(cmd, requestId, data);
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
            _userSolanaManager.OnExtensionError(cmd, requestId, errorCode, errorMessage);
        }
    }
}