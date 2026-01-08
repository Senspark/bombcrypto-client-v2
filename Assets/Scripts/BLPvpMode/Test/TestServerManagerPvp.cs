using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using App.BomberLand;
using CustomSmartFox;
using PvpMode.Services;
using Services;
using Services.Server;
using Sfs2X.Entities.Data;
using IServerDispatcher = App.IServerDispatcher;

namespace BLPvpMode.Test {
    public class TestPvpServerBridge : IPvpServerBridge {
        public TestPvpServerBridge() { }

        public Task<ISyncPvPConfigResult> SyncPvPConfig() {
            throw new NotImplementedException();
        }

        public Task<ISyncPvPHeroesResult> SyncPvPHeroes() {
            throw new NotImplementedException();
        }

        public Task JoinQueue(int mode, string matchId, int heroId, int[] boosters, 
            IPingInfo[] pingInfo, int avatarId, bool test = false) {
            throw new NotImplementedException();
        }

        public Task LeaveQueue() {
            throw new NotImplementedException();
        }

        public Task<IBoosterResult> GetUserBooster() {
            throw new NotImplementedException();
        }

        public Task BuyBooster(int boosterType) {
            throw new NotImplementedException();
        }

        public void ClearCachePvpRanking() {
            throw new NotImplementedException();
        }

        public Task<IPvpRankingResult> GetPvpRanking(int page = 1, int size = 100) {
            throw new NotImplementedException();
        }

        public Task<IPvpOtherUserInfo> GetOtherUserInfo(int userId, string userName) {
            throw new NotImplementedException();
        }

        public Task<ICoinLeaderboardConfigResult[]> GetCoinLeaderboardConfig() {
            throw new NotImplementedException();
        }
        
        public Task<ICoinRankingResult> GetCoinRanking() {
            throw new NotImplementedException();
        }
        
        public Task<ICoinRankingResult> GetAllSeasonCoinRanking() {
            throw new NotImplementedException();
        }

        public Task<IPvpClaimRewardResult> ClaimPvpReward() {
            throw new NotImplementedException();
        }

        public Task<IPvpClaimMatchRewardResult> ClaimMatchReward() {
            throw new NotImplementedException();
        }

        public Task<IPvpHistoryResult> GetPvpHistory(int at = 0, int size = 50) {
            throw new NotImplementedException();
        }

        public Task<IOpenChestResult> OpenChest() {
            throw new NotImplementedException();
        }

        public Task<IGetEquipmentResult> GetEquipment() {
            throw new NotImplementedException();
        }

        public Task Equip(int itemType, IEnumerable<(int, long)> itemList) {
            throw new NotImplementedException();
        }

        public Task<IBonusRewardPvp> GetBonusRewardPvp(string matchId, string adsId) {
            throw new NotImplementedException();
        }
    }

    public class TestServerManagerPvp : IServerManager, IServerDispatcher {
        private IPvpServerBridge _pvpServerBridge;

        public Task<bool> Initialize() {
            _pvpServerBridge = new TestPvpServerBridge();
            return Task.FromResult(true);
        }

        public void Destroy() { }

        public int AddObserver(ServerObserver observer) {
            return 0;
        }

        public bool RemoveObserver(int id) {
            throw new NotImplementedException();
        }

        public void SendLastJoinedRoomExtensionRequest(string cmd, ISFSObject data) {
            throw new NotImplementedException();
        }

        public Task<T> Send<T>(string cmd, ISFSObject data, ResponseParser<T> responseParser) {
            throw new NotImplementedException();
        }

        public void SendOnly(string cmd, ISFSObject data) {
            throw new NotImplementedException();
        }

        public Task<ISFSObject> SendCmd(IExtCmd<ISFSObject> cmd) {
            throw new NotImplementedException();
        }

        public Task<T> SendCmd<T>(IExtCmd<T> cmd) {
            throw new NotImplementedException();
        }

        public void DispatchEvent(Action<ServerObserver> dispatcher) {
            throw new NotImplementedException();
        }

        public bool IsUserInitSuccess { get; set; }
        public ServerConnectionState CurrentState { get; }
        public IMarketplace Marketplace { get; }
        public IPvpServerBridge Pvp => _pvpServerBridge;
        public IStoryModeServerBridge StoryMode { get; }
        public IPveServerBridge Pve { get; }
        public IBomberLandServerBridge BomberLand { get; }
        public IGeneralServerBridge General { get; }
        public ITHModeV2Manager ThModeV2Manager { get; }
        public IUserTonManager UserTonManager { get; }
        public IUserSolanaManager UserSolanaManager { get; }
        public IDailyTaskManager DailyTaskManager { get; }

        public Task Connect() {
            throw new NotImplementedException();
        }

        public Task AutoReconnectAndLogin() {
            throw new NotImplementedException();
        }

        public Task WaitForConnection() {
            throw new NotImplementedException();
        }

        public Task Connect(float timeOut) {
            throw new NotImplementedException();
        }

        public Task ReLogin(ILoginData loginData) {
            throw new NotImplementedException();
        }

        public Task<ILoginResponse> Login(ILoginData loginData, bool isForceLogin) {
            throw new NotImplementedException();
        }

        public Task ReLogin() {
            throw new NotImplementedException();
        }

        public Task<ILoginResponse> Login(string address, string signature, string activationCode, float timeOut) {
            throw new NotImplementedException();
        }

        public Task<ILoginResponse> Login(string username, string parameter, float timeOut) {
            throw new NotImplementedException();
        }

        public Task<ILoginResponse> LoginWithUsername(string username, string password, string activationCode,
            float timeOut) {
            throw new NotImplementedException();
        }

        public Task Logout() {
            throw new NotImplementedException();
        }

        public bool IsConnected { get; }

        public void SendExtensionRequest(string cmd, ISFSObject data) {
            throw new NotImplementedException();
        }

        public Task<ISFSObject> SendExtensionRequestAsync(IExtCmd<ISFSObject> extCmd) {
            throw new NotImplementedException();
        }

        // public Task<string>
        //     SendExtensionRequestAsync(string cmd, IDictionary<string, object> parameters, float timeOut) {
        //     throw new NotImplementedException();
        // }

        public void ProcessEvents() {
            throw new NotImplementedException();
        }

        public void Disconnect() {
            throw new NotImplementedException();
        }

        public void TestKillServer() {
            throw new NotImplementedException();
        }

        public void EnableGetLatency() { }

        public void DisableGetLatency() { }

        public Task WaitForUserInitialized() {
            throw new NotImplementedException();
        }

        public void ClearCache(string requestKey) {
            throw new NotImplementedException();
        }

        public void InitKickUserListener() {
            throw new NotImplementedException();
        }

        public string EncodeLoginData(string loginData) {
            throw new NotImplementedException();
        }
    }
}