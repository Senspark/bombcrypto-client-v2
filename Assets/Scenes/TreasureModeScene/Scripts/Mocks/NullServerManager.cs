using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;
using App.BomberLand;

using CustomSmartFox;

using PvpMode.Services;

using Senspark;

using Services.Server;
using Services.Server.ConcreateClasses;

using Sfs2X.Entities.Data;

namespace Scenes.TreasureModeScene.Scripts.Mocks {
    public class NullServerManager : IServerManager {
        Task<bool> IService.Initialize() {
            Pve = new NullPveServerBridge();
            return Task.FromResult(true);
        }

        public bool IsUserInitSuccess { get; set; }
        public ServerConnectionState CurrentState { get; }
        public IMarketplace Marketplace { get; }
        public IPvpServerBridge Pvp => new NullPvpServerBridge();
        public IStoryModeServerBridge StoryMode { get; }
        public IPveServerBridge Pve { get; private set; }
        public IBomberLandServerBridge BomberLand { get; }
        public IGeneralServerBridge General { get; }
        public ITHModeV2Manager ThModeV2Manager => new NullTHModeV2Manager();
        public IUserTonManager UserTonManager { get; }
        public IUserSolanaManager UserSolanaManager { get; }
        public IDailyTaskManager DailyTaskManager { get; }

        public Task Connect(float timeOut) {
            throw new NotImplementedException();
        }

        public Task ReLogin() {
            throw new NotImplementedException();
        }

        public Task ReLogin(ILoginData loginData) {
            throw new NotImplementedException();
        }

        public Task<ILoginResponse> Login(ILoginData loginData, bool isForceLogin) {
            throw new NotImplementedException();
        }

        public Task Logout() {
            throw new NotImplementedException();
        }

        public Task<ISFSObject> SendExtensionRequestAsync(IExtCmd<ISFSObject> extCmd) {
            throw new NotImplementedException();
        }

        // public Task<string> SendExtensionRequestAsync(string cmd, IDictionary<string, object> parameters,
        //     float timeOut = 15) {
        //     throw new NotImplementedException();
        // }

        public void Disconnect() {
            throw new NotImplementedException();
        }

        public void TestKillServer() {
            throw new NotImplementedException();
        }

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

        void IService.Destroy() {
        }

        public int AddObserver(ServerObserver observer) {
            return 0;
        }

        public bool RemoveObserver(int id) {
            return true;
        }

        public void DispatchEvent(Action<ServerObserver> dispatcher) {
            throw new NotImplementedException();
        }
    }
}