using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using BLPvpMode.Manager.Api;
using Game.Dialog;
using PvpMode.Services;
using Scenes.TreasureModeScene.Scripts.Service;
using Senspark;
using UnityEngine;

[Service(nameof(IUserSolanaManager))]
public interface IUserSolanaManager: IService, IServerListener {
    IUserSolanaEventTrigger EventTrigger { get; }
    void LogoutUserSolana();
    Task<string> GetInvoice(double amount, DepositType depositType);
    Task<bool> DepositSol(string invoice, double amount, DepositType depositType);
    Task<ISyncHeroResponse> AddHeroForSolUser();
    Task<IBuyHeroServerResponse> BuyHeroSol(int quantity, int rewardType, bool setBuyHero = false);
    Task<IHouseDetails> BuyHouseSol(int rarity, int rewardType);
    Task<IAutoMinePackages> GetAutoMinePriceSol();
    Task<IMapDetails> GetMapDetailsSol();
    Task<ICoinLeaderboardConfigResult[]> GetCoinLeaderboardConfigSol();
    Task<ICoinRankingResult> GetCoinRankingSol();
    Task<ICoinRankingResult> GetAllSeasonCoinRankingSol();
    Task GetRentHousePackageConfig();
    Task<ITreasureHuntConfigResponse> GetTreasureHuntDataConfigSol();
    Task<ISyncHouseResponse> SyncHouseSol();
    Task<IFusionTonHeroResponse> FusionServer(int target, int[] heroList);
    Task<IFusionTonHeroResponse> MultiFusionServer(int target, int[] heroList);
    void StartExplodeSol(GameModeType type, HeroId heroId, int bombId, Vector2Int tileLocation, List<Vector2Int> brokenList);
    Task<bool> StartAutoMineSol();
    Task<IChestReward> BuyAutoMineSol(string packageName, BlockRewardType blockRewardType);
    void GoHomeSol(HeroId id);
    void GoWorkSol(HeroId id);
    void GoSleepSol(HeroId id);
    Task ChangeBomberManStageSol(HeroId[] ids, HeroStage stage);
    Task<bool> ActiveBomberSol(HeroId id, int value);
    Task<bool> ActiveBomberHouseSol(string genId, int houseId);
    Task<bool> GetActiveBomberSol();
    Task<IStartPveResponse> StartPvESol(GameModeType type);
    Task<IInvestedDetail> StopPvESol();
    Task<IChestReward> GetChestRewardSol();
    Task<bool> ReactiveHouseSol(int houseId);
}

public interface IUserSolanaEventTrigger : IObserverManager<UserSolanaObserver> {

}
