using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using Game.Dialog;
using PvpMode.Services;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using UnityEngine;

namespace Scenes.TreasureModeScene.Scripts.Service {
    public class NullUserSolana : IUserSolanaManager {
        public IUserSolanaEventTrigger EventTrigger { get; }

        public void LogoutUserSolana() {
        }
        
        public Task<string> GetInvoice(double amount, DepositType depositType) {
            return Task.FromResult(string.Empty);
        }
        
        public Task<bool> DepositSol(string invoice, double amount, DepositType depositType) {
            return Task.FromResult(false);
        }
        
        public Task<ISyncHeroResponse> AddHeroForSolUser() {
            throw new NotImplementedException();
        }
        
        public Task<IBuyHeroServerResponse> BuyHeroSol(int quantity, int rewardType, bool setBuyHero = false) {
            throw new NotImplementedException();
        }
        
        public Task<IHouseDetails> BuyHouseSol(int rarity, int rewardType) {
            throw new NotImplementedException();
        }
        
        public Task<IAutoMinePackages> GetAutoMinePriceSol() {
            throw new NotImplementedException();
        }
        
        public Task<IMapDetails> GetMapDetailsSol() {
            throw new NotImplementedException();
        }
        
        public Task<ICoinLeaderboardConfigResult[]> GetCoinLeaderboardConfigSol() {
            throw new NotImplementedException();
        }
        
        public Task<ICoinRankingResult> GetCoinRankingSol() {
            throw new NotImplementedException();
        }
        
        public Task<ICoinRankingResult> GetAllSeasonCoinRankingSol() {
            throw new NotImplementedException();
        }
        
        public Task GetRentHousePackageConfig() {
            throw new NotImplementedException();
        }
        
        public Task<ITreasureHuntConfigResponse> GetTreasureHuntDataConfigSol() {
            throw new NotImplementedException();
        }
        
        public Task<ISyncHouseResponse> SyncHouseSol() {
            throw new NotImplementedException();
        }
        
        public Task<IFusionTonHeroResponse> FusionServer(int target, int[] heroList) {
            throw new NotImplementedException();
        }
        
        public Task<IFusionTonHeroResponse> MultiFusionServer(int target, int[] heroList) {
            throw new NotImplementedException();
        }
        
        public void StartExplodeSol(GameModeType type, HeroId heroId, int bombId, Vector2Int tileLocation,
            List<Vector2Int> brokenList) {
            throw new NotImplementedException();
        }
        
        public Task<bool> StartAutoMineSol() {
            throw new NotImplementedException();
        }
        
        public Task<IChestReward> BuyAutoMineSol(string packageName, BlockRewardType blockRewardType) {
            throw new NotImplementedException();
        }
        
        public void GoHomeSol(HeroId id) {
            throw new NotImplementedException();
        }
        
        public void GoWorkSol(HeroId id) {
            throw new NotImplementedException();
        }
        
        public void GoSleepSol(HeroId id) {
            throw new NotImplementedException();
        }
        
        public Task ChangeBomberManStageSol(HeroId[] ids, HeroStage stage) {
            throw new NotImplementedException();
        }
        
        public Task<bool> ActiveBomberSol(HeroId id, int value) {
            throw new NotImplementedException();
        }
        
        public Task<bool> ActiveBomberHouseSol(string genId, int houseId) {
            throw new NotImplementedException();
        }
        
        public Task<bool> GetActiveBomberSol() {
            throw new NotImplementedException();
        }
        
        public Task<IStartPveResponse> StartPvESol(GameModeType type) {
            throw new NotImplementedException();
        }
        
        public Task<IInvestedDetail> StopPvESol() {
            throw new NotImplementedException();
        }
        
        public Task<IChestReward> GetChestRewardSol() {
            throw new NotImplementedException();
        }
        
        public Task<bool> ReactiveHouseSol(int houseId) {
            throw new NotImplementedException();
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public void OnConnection() {
        }

        public void OnConnectionError(string message) {
        }

        public void OnConnectionRetry() {
        }

        public void OnConnectionResume() {
        }

        public void OnConnectionLost(string reason) {
        }

        public void OnLogin() {
        }

        public void OnLoginError(int code, string message) {
        }

        public void OnUdpInit(bool success) {
        }

        public void OnPingPong(int lagValue) {
        }

        public void OnRoomVariableUpdate(SFSRoom room) {
        }

        public void OnJoinRoom(SFSRoom room) {
        }

        public void OnExtensionResponse(string cmd, ISFSObject value) {
        }
        
        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        }
    }
}