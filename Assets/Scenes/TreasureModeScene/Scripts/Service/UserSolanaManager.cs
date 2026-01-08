using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using App.BomberLand;
using CustomSmartFox;
using Engine.Manager;
using Game.Dialog;
using PvpMode.Services;
using Scenes.TreasureModeScene.Scripts.Service;
using Senspark;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Share.Scripts.Communicate;
using UnityEngine;

public enum LoginEvent {
    UserInitialized,
}
public class UserSolanaManager : IUserSolanaManager
{
    private readonly IGeneralServerBridge _general;
    private readonly  IPveServerBridge _pve;
    private readonly IPvpServerBridge _pvp;
    private readonly IExtResponseEncoder _encoder;
    private readonly IUserSolanaEventTrigger _eventTrigger;
    private readonly ISolanaReactProcess _solanaReactProcess;
    private readonly ILogManager _logManager;

    public UserSolanaManager(
        IMasterUnityCommunication unityCommunication,
        ILogManager logManager,
        IGeneralServerBridge general,
        IPveServerBridge pve,
        IPvpServerBridge pvp,
        IExtResponseEncoder encoder)
    {
        _general = general;
        _pve = pve;
        _pvp = pvp;
        _encoder = encoder;
        _logManager = logManager;
        _solanaReactProcess = new SolanaReactProcess(unityCommunication, logManager);
        _eventTrigger = new UserSolanaEventTrigger();
    }

    public IUserSolanaEventTrigger EventTrigger => _eventTrigger;

    public void LogoutUserSolana() {
        _solanaReactProcess.LogoutUserSolana();
    }
    
    public async Task<string> GetInvoice(double amount, DepositType depositType) {
        return await _general.GetInvoiceSol(amount, depositType);
    }
    
    public async Task<bool> DepositSol(string invoice, double amount, DepositType depositType) {
        return await _solanaReactProcess.DepositSol(invoice, amount, depositType);
    }
    
    public async Task<ISyncHeroResponse> AddHeroForSolUser() {
        return await _general.AddHeroForAirdropUser();
    }
    
    public async Task<IBuyHeroServerResponse> BuyHeroSol(int quantity, int rewardType, bool setBuyHero = false) {
        return await _general.BuyHeroServer(quantity, rewardType, setBuyHero);
    }
    
    public async Task<IHouseDetails> BuyHouseSol(int rarity, int rewardType) {
        return await _general.BuyHouseServer(rarity, rewardType);
    }
    
    public async Task<IAutoMinePackages> GetAutoMinePriceSol() {
        return await _general.GetAutoMinePrice();
    }
    
    public async Task<IMapDetails> GetMapDetailsSol() {
        return await _pve.GetMapDetails();
    }
    
    public async Task<ICoinLeaderboardConfigResult[]> GetCoinLeaderboardConfigSol() {
        return await _pvp.GetCoinLeaderboardConfig();
    }
    
    public async Task<ICoinRankingResult> GetCoinRankingSol() {
        return await _pvp.GetCoinRanking();
    }
    
    public async Task<ICoinRankingResult> GetAllSeasonCoinRankingSol() {
        return await _pvp.GetAllSeasonCoinRanking();
    }
    
    public Task GetRentHousePackageConfig() {
        return _general.GetRentHousePackageConfig();
    }
    
    public Task<ITreasureHuntConfigResponse> GetTreasureHuntDataConfigSol() {
        return _general.GetTreasureHuntDataConfig();
    }
    
    public async Task<ISyncHouseResponse> SyncHouseSol() {
        return await _general.SyncHouse();
    }
    
    public async Task<IFusionTonHeroResponse> FusionServer(int target, int[] heroList) {
        return await _general.FusionHeroServer(target, heroList);
    }
    
    public async Task<IFusionTonHeroResponse> MultiFusionServer(int target, int[] heroList) {
        return await _general.MultiFusionHeroServer(target, heroList);
    }
    
    public void StartExplodeSol(GameModeType type, HeroId heroId, int bombId, Vector2Int tileLocation, List<Vector2Int> brokenList) {
        _pve.StartExplode(type, heroId, bombId, tileLocation, brokenList);
    }
    
    public async Task<bool> StartAutoMineSol() {
        return await _general.StartAutoMine();
    }
    
    public async Task<IChestReward> BuyAutoMineSol(string packageName, BlockRewardType blockRewardType) {
        return await _general.BuyAutoMine(packageName, blockRewardType);
    }
    
    public void GoHomeSol(HeroId id) {
        _pve.GoHome(id);
    }
    
    public void GoWorkSol(HeroId id) {
        _pve.GoWork(id);
    }
    
    public void GoSleepSol(HeroId id) {
        _pve.GoSleep(id);
    }
    
    public async Task ChangeBomberManStageSol(HeroId[] ids, HeroStage stage) {
        await _pve.ChangeBomberManStage(ids, stage);
    }
    
    public async Task<bool> ActiveBomberSol(HeroId id, int value) {
        return await _pve.ActiveBomber(id, value);
    }
    
    public async Task<bool> ActiveBomberHouseSol(string genId, int houseId) {
        return await _pve.ActiveBomberHouse(genId, houseId);
    }
    
    public async Task<bool> GetActiveBomberSol() {
        return await _pve.GetActiveBomber();
    }
    
    public async Task<IStartPveResponse> StartPvESol(GameModeType type) {
        return await _pve.StartPvE(type);
    }
    
    public async Task<IInvestedDetail> StopPvESol() {
        return await _pve.StopPvE();
    }
    
    public Task<IChestReward> GetChestRewardSol() {
        return _general.GetChestReward();
    }
    
    public Task<bool> ReactiveHouseSol(int houseId) {
        return _general.ReactiveHouse(houseId);
    }

    public void OnExtensionResponse(string cmd, ISFSObject value) {
        if (cmd == SFSDefine.SFSCommand.DEPOSIT_SOL_RESPONSE) {
            try {
                _logManager.Log($"RECEIVED: cmd({cmd}) value({value})");
                EventTrigger.DispatchEvent(e => e.OnDepositResponse?.Invoke(value));
                EventTrigger.DispatchEvent(e => e.OnDepositComplete?.Invoke(true));
            } catch (Exception e) {
                EventTrigger.DispatchEvent(e => e.OnDepositComplete?.Invoke(false));
            }
        }
        if (cmd == SFSDefine.SFSCommand.USER_INITIALIZED) {
            //FIXME: dùng tạm
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            serverManager.IsUserInitSuccess = true;
            _logManager.Log("USER_INITIALIZED success");
            // Dirty fix
            EventManager.Dispatcher(LoginEvent.UserInitialized);
        }
    }
    
    // Dùng cho các command đc server chủ động gửi về trực tiếp client mà ko cần client gọi
    public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
        if (cmd == SFSDefine.SFSCommand.USER_INITIALIZED) {
            //FIXME: dùng tạm
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            serverManager.IsUserInitSuccess = true;
            _logManager.Log("USER_INITIALIZED success");
            // Dirty fix
            EventManager.Dispatcher(LoginEvent.UserInitialized);
        }
    }

    // Dùng cho các command đc server chủ động gửi về trực tiếp client mà ko cần client gọi
    public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        if (cmd == SFSDefine.SFSCommand.DEPOSIT_SOL_RESPONSE) {
            _logManager.Log($"RECEIVED: cmd({cmd}) requestId({requestId}) ERROR({errorCode}) {errorMessage}");
            EventTrigger.DispatchEvent(e => e.OnDepositComplete?.Invoke(false));
        }
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
}
