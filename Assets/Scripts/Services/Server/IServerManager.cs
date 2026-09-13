using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App.BomberLand;

using CustomSmartFox;

using PvpMode.Services;

using Senspark;

using Services.Server;

using Sfs2X.Entities.Data;

namespace App {
    [Service(nameof(IServerManager))]
    public interface IServerManager : IService, IObserverManager<ServerObserver> {
        bool IsUserInitSuccess { get; set; }
        ServerConnectionState CurrentState { get; }
        IMarketplace Marketplace { get; }
        IPvpServerBridge Pvp { get; }
        IStoryModeServerBridge StoryMode { get; }
        IPveServerBridge Pve { get; }
        IBomberLandServerBridge BomberLand { get; }
        IGeneralServerBridge General { get; }
        ITHModeV2Manager ThModeV2Manager { get; }
        IUserTonManager UserTonManager { get; }
        public IUserSolanaManager UserSolanaManager { get; }
        IDailyTaskManager DailyTaskManager { get; }

        Task Connect(float timeOut);
        Task ReLogin();
        Task ReLogin(ILoginData loginData);
        Task<ILoginResponse> Login(ILoginData loginData, bool isForceLogin);
        Task Logout();
        Task<ISFSObject> SendExtensionRequestAsync(IExtCmd<ISFSObject> extCmd);
        // Task<string> SendExtensionRequestAsync(string cmd, IDictionary<string, object> parameters, float timeOut = 15f);
        void Disconnect();
        void TestKillServer();
        Task WaitForUserInitialized();
        void ClearCache(string requestKey);
        void InitKickUserListener();
    }

    public class ServerObserver {
        public Action<ServerConnectionState> OnServerStateChanged;
        public Action<string, ISFSObject> OnExtensionResponse;
        public Action<ISyncHeroResponse> OnSyncHero;
        public Action<int[], bool> OnNewHeroFi;
        public Action<int[], bool> OnNewHeroServer;
        public Action<ISyncHouseResponse> OnSyncHouse;

        /// P2P rental: event ("RENTED", "CHARGED", "ENDED_*") and house id
        public Action<string, int> OnHouseRentalUpdate;
        public Action<IPveHeroDangerous> OnHeroChangeState;
        public Action<IInvestedDetail> OnInvestedDetail;
        public Action<IChestReward> OnChestReward;
        public Action<int> OnUpdateLatency;
        public Action<IPveExplodeResponse> OnPveExploded;
        public Action<bool> OnNewMapResponse;
        public Action<IPveHeroDangerous, HeroId, bool> OnActiveHero;
        //Event này dùng để remove heroes ra khỏi map đang đào sau khi bị burn do fusion
        public Action<HeroId[]> OnRemoveHeroes;
        public Action<IClubInfo> OnJoinClub;
        public Action<IClubInfo> OnLeaveClub;
        public Action OnBlockchainDataRefreshed;
        public Action<IUpgradeShieldLevelPush> OnUpgradeShieldLevelResponse;
        // Hero data đã được ForceUpdate vào PlayerStorage trước khi event này bắn.
        public Action<IHeroDetails> OnHeroStakePush;
    }

    public interface IUpgradeShieldLevelPush {
        bool Success { get; }
        string ErrorMessage { get; }
        IHeroDetails Hero { get; }
    }

    public enum ServerConnectionState {
        /// <summary>
        /// Chưa đăng nhập, hoặc đã đăng xuất
        /// </summary>
        LoggedOut,

        /// <summary>
        /// Đã Login thành công, đang có Connection tốt
        /// </summary>
        LoggedIn,

        /// <summary>
        /// Bị Disconnect khỏi server, ko phải user tự đăng xuất
        /// </summary>
        LostConnection,
        
        /// <summary>
        /// Will not Reconnection
        /// </summary>
        KickOut,
    }
}