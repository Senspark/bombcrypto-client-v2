using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.BomberLand;
using BLPvpMode.Manager.Api;
using BLPvpMode.Manager.Api.Handlers;
using CustomSmartFox;
using Cysharp.Threading.Tasks;
using Senspark;
using JetBrains.Annotations;
using Newtonsoft.Json;
using PvpMode.Services;
using Scenes.TreasureModeScene.Scripts.Service;

using Services;
using Services.Server;
using Services.Server.Handlers;
using Sfs2X.Core;
using Sfs2X.Entities.Data;
using Sfs2X.Util;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using UnityEngine.Assertions;
using Utils;

namespace App {
    public class GameServerManager : ObserverManager<ServerObserver>, IServerManager, IServerDispatcher {
        public bool IsUserInitSuccess { get; set; }
        public ServerConnectionState CurrentState { get; private set; } = ServerConnectionState.LoggedOut;
        public IMarketplace Marketplace { get; }
        public IPvpServerBridge Pvp { get; }
        public IStoryModeServerBridge StoryMode { get; }
        public IPveServerBridge Pve { get; }
        public IBomberLandServerBridge BomberLand { get; }
        public IGeneralServerBridge General { get; }

        public ITHModeV2Manager ThModeV2Manager { get; }
        public IUserTonManager UserTonManager { get; }
        public IUserSolanaManager UserSolanaManager { get; }
        public IDailyTaskManager DailyTaskManager { get; }
        public ICacheRequestManager CacheRequestManager { get; }
        public IServerNotifyManager ServerNotifyManager { get; }

        private readonly bool _enableLog;
        private readonly ILogManager _logManager;
        private readonly ISmartFoxApi _api;
        private readonly IExtResponseEncoder _encoder;
        private readonly IServerBridge _serverBridge;
        private readonly IServerConfig _serverConfig;
        private readonly IServerListener[] _listeners;
        private readonly IExtensionRequestBuilder _requestBuilder;
        private readonly IHasher _hasher;
        private readonly ITaskDelay _taskDelay;
        private readonly IUserAccountManager _userAccountManager;
        private readonly IMasterUnityCommunication _unityCommunication;

        [CanBeNull]
        private IServerHandlerVoid _connectHandler;

        [CanBeNull]
        private IServerHandler<ILoginResponse> _loginHandler;

        private int _requestId;
        private string _userName;
        private int _id;

        public GameServerManager(
            bool enableLog,
            IServerConfig config,
            ILogManager logManager,
            IStorageManager storageManager,
            IPlayerStorageManager playerStorageManager,
            IHouseStorageManager houseStorageManager,
            IChestRewardManager chestRewardManager,
            IClaimTokenServerBridge claimTokenServerBridge,
            ITaskDelay taskDelay,
            ITaskTonManager taskTonManager,
            IOnBoardingManager onBoardingManager,
            IUserAccountManager userAccountManager,
            IMasterUnityCommunication unityCommunication,
            IExtResponseEncoder encoder,
            IServerNotifyManager serverNotifyManager
        ) {
            _enableLog = enableLog;
            _logManager = logManager;
            _serverConfig = config;
            _taskDelay = taskDelay;
            _serverBridge = new ServerBridge(config.UseWebSocket, config.IsEncrypted, true);
            _api = new SmartFoxApi(_serverBridge, encoder, this);
            _encoder = encoder;
            _unityCommunication = unityCommunication;

            _hasher = new Hasher();
            _requestBuilder = new ExtensionRequestBuilder(BuildExtensionRequestParameters);
            _userAccountManager = userAccountManager;

            CacheRequestManager = new DefaultCacheRequestManager(_logManager, _api);
            Pvp = new DefaultPvpServerBridge(this, enableLog, _requestBuilder, _taskDelay);
            StoryMode = new DefaultStoryModeServerBridge(this, this);
            Pve = new DefaultPveServerBridge(
                playerStorageManager,
                houseStorageManager,
                storageManager,
                this
            );
            BomberLand =
                new DefaultBomberLandServerBridge(enableLog, _api, _requestBuilder, _taskDelay, CacheRequestManager, this);
            General = new DefaultGeneralServerBridge(
                enableLog,
                storageManager,
                playerStorageManager,
                houseStorageManager,
                chestRewardManager,
                claimTokenServerBridge,
                _logManager,
                this,
                _api,
                _requestBuilder,
                _taskDelay,
                CacheRequestManager,
                taskTonManager,
                onBoardingManager
            );
            Marketplace = new Services.Server.Marketplace(_logManager, this, General);


            ThModeV2Manager = new THModeV2Manager(this, _logManager);
            UserTonManager = AppConfig.IsTon()
                ? new UserTonManager(this, taskTonManager, General, userAccountManager, _logManager, _encoder)
                : new NullUserTon();
            UserSolanaManager = new DefaultUserSolanaManager(unityCommunication, _logManager, General, Pve, Pvp, _encoder);
            _listeners = new IServerListener[] {
                new AnyExtensionResponseListener(this),
                new CheckConnectionListener(_logManager, Disconnect, OnServerStateChanged),
                new NewMapHandler(this, _logManager),
                ThModeV2Manager,
                UserTonManager,
                UserSolanaManager,
                serverNotifyManager
            };
            Array.ForEach(_listeners, e => _api.AddListener(e));

            // Listen response init user controller thành công từ server
            _serverBridge.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnUserInitializedResponse);
        }

        public async Task<bool> Initialize() {
            SFSErrorCodes.SetErrorMessage(ErrorCode.SERVER_MAINTENAINCE, "Server maintenance");
            SFSErrorCodes.SetErrorMessage(ErrorCode.WRONG_VERSION, SfsError.WrongVersion);
            SFSErrorCodes.SetErrorMessage(ErrorCode.USER_BANNED, "{0}");
            SFSErrorCodes.SetErrorMessage(ErrorCode.AlREADY_LOGIN, "Your account is already logged in");
            SFSErrorCodes.SetErrorMessage(ErrorCode.KICK_BY_OTHER_DEVICE, "Your account is login on another device");
            SFSErrorCodes.SetErrorMessage(ErrorCode.INVALID_SIGNATURE, "Invalid signature");
            SFSErrorCodes.SetErrorMessage(ErrorCode.INVALID_LOGIN_DATA, "Invalid login data");
            SFSErrorCodes.SetErrorMessage(ErrorCode.LOGGED_IN, "This account is logged in");
            SFSErrorCodes.SetErrorMessage(ErrorCode.UNDER_REVIEW, "This account is under review");
            SFSErrorCodes.SetErrorMessage(ErrorCode.INVALID_USR_PWD, "Invalid username or password");
            SFSErrorCodes.SetErrorMessage(ErrorCode.INVALID_LICENSE, "Invalid license");
            SFSErrorCodes.SetErrorMessage(ErrorCode.INVALID_ACTIVATION_CODE, "Invalid activation code");
            SFSErrorCodes.SetErrorMessage(ErrorCode.LOGIN_FAILED, "Login Failed");
            SFSErrorCodes.SetErrorMessage(ErrorCode.PVP_MATCH_EXPIRED, "Match expired");
            SFSErrorCodes.SetErrorMessage(ErrorCode.PVP_INTERNAL_ERROR, "Internal error");
            SFSErrorCodes.SetErrorMessage(ErrorCode.PVP_MATCH_EXPIRED, "Match expired");
            SFSErrorCodes.SetErrorMessage(ErrorCode.PVP_INVALID_MATCH_HASH, "Invalid hash");
            SFSErrorCodes.SetErrorMessage(ErrorCode.PVP_INVALID_MATCH_SERVER, "Invalid server");

            return true;
        }

        public void Destroy() {
            Array.ForEach(_listeners, e => _api.RemoveListener(e));
            _api.Dispose();
            _serverBridge.Dispose();
        }

        public async Task Connect(float timeOut) {
            _api.Reinitialize();
            _connectHandler ??=
                new ConnectHandler(_logManager, _serverConfig.Host, _serverConfig.Port, false, _taskDelay);
            await _api.Process(_connectHandler);
        }

        public async Task ReLogin() {
            Assert.IsNotNull(_loginHandler);
            await _api.Process(_loginHandler);
            _serverBridge.IsLagMonitorEnabled = true;
        }

        public async Task ReLogin(ILoginData loginData) {
            _loginHandler = new SvGameLoginHandler(_logManager, _hasher, _serverConfig, _taskDelay, loginData, _encoder,
                false);

            ILoginResponse result;

            result = await _api.Process(_loginHandler);
            _serverBridge.IsLagMonitorEnabled = true;
            _userName = result.WalletAddress ?? result.SecondUserName;

            // xóa cache request nếu login user khác lần login trước.
            var userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            if (userAccountManager.IsNewUserLogin) {
                CacheRequestManager.ClearCacheForNewUser();
                userAccountManager.IsNewUserLogin = false;
            }
        }

        public async Task<ILoginResponse> Login(ILoginData loginData, bool isForceLogin) {
            Assert.IsNotNull(_connectHandler);
            if (_loginHandler != null) {
                throw new Exception("Already logged in");
            }
            _loginHandler = new SvGameLoginHandler(_logManager, _hasher, _serverConfig, _taskDelay, loginData, _encoder,
                isForceLogin);

            ILoginResponse result;

            result = await _api.Process(_loginHandler);
            _serverBridge.IsLagMonitorEnabled = true;
            _userName = result.WalletAddress ?? result.SecondUserName;

            // xóa cache request nếu login user khác lần login trước.
            if (AppConfig.IsTon()) {
                // Ton ko sử dụng đc IsNewUserLogin để check nên cứ auto xoá cache
                CacheRequestManager.ClearCacheForNewUser();
            } 
            else {
                var userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
                if (userAccountManager.IsNewUserLogin) {
                    CacheRequestManager.ClearCacheForNewUser();
                    userAccountManager.IsNewUserLogin = false;
                }
            }
            

            return result;
        }

        public Task Logout() {
            Disconnect();
            return Task.CompletedTask;
        }
        
        public async Task<T> Send<T>(string cmd, ISFSObject data, ResponseParser<T> responseParser) {
            Assert.IsNotNull(_connectHandler);
            Assert.IsNotNull(_loginHandler);
            var parameters = _requestBuilder.Build(cmd, data);
            var h = new LegacyExtensionHandler<T>(_enableLog, cmd, parameters, _taskDelay, responseParser);
            var result = await CacheRequestManager.ProcessApi(cmd, data, h);
            return result;
        }

        public void SendOnly(string cmd, ISFSObject data) {
            Assert.IsNotNull(_connectHandler);
            Assert.IsNotNull(_loginHandler);
            var parameters = _requestBuilder.Build(cmd, data);
            var h = new LegacyExtensionHandler(_enableLog, cmd, parameters);
            _api.Process(h);
        }

        public async Task<ISFSObject> SendCmd(IExtCmd<ISFSObject> cmd) {
            var handler = new EncryptedExtensionHandlerSfs(
                _encoder,
                _id++,
                cmd.EnableLog,
                cmd.Cmd,
                cmd.ExportData()
            );
            return await _api.Process(handler);
        }

        public async Task<ISFSObject> SendExtensionRequestAsync(IExtCmd<ISFSObject> extCmd) {
            Assert.IsNotNull(_connectHandler);
            Assert.IsNotNull(_loginHandler);
            var handler = new EncryptedExtensionHandlerSfs(
                _encoder,
                _id++,
                extCmd.EnableLog,
                extCmd.Cmd,
                extCmd.ExportData()
            );
            var parameters = extCmd.Data;
            var cmd = extCmd.Cmd;
            if (parameters.ContainsKey("token") || parameters.ContainsKey("ads_id") || parameters.ContainsKey("ads_token")) {
                for (var i = 0; i < 5; i++) {
                    await UniTask.Delay(2000);
                    var iResult = await CacheRequestManager.ProcessApi(cmd, parameters, handler);
                    if (!ServerUtils.HasError(iResult)) {
                        return iResult;
                    }
                }
            }
            var result = await CacheRequestManager.ProcessApi(cmd, parameters, handler);
            return result;
        }

        public void ClearCache(string requestKey) {
            CacheRequestManager.ClearCache(requestKey);
        }

        public void Disconnect() {
            _api.Process(new DisconnectHandler());
            // DispatchEvent(e => e.OnServerStateChanged?.Invoke(ServerConnectionState.LostConnection));
        }

        public void TestKillServer() {
            if (_serverConfig.UseWebSocket) {
                Disconnect();
            } else {
                _api.Process(new KillConnectHandler());
            }
        }

        public async Task WaitForUserInitialized() {
            while (!IsUserInitSuccess) {
                await UniTask.Delay(500);
            }
        }

        private void OnServerStateChanged(ServerConnectionState state) {
            CurrentState = state;
            if (CurrentState is not ServerConnectionState.LoggedIn) {
                IsUserInitSuccess = false;
            }
            DispatchEvent(e => e.OnServerStateChanged?.Invoke(state));
        }

        private ISFSObject BuildExtensionRequestParameters(string command, ISFSObject data) {
            var parameters = new SFSObject();
            var requestId = _requestId++;
            var timestamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            var hash = _hasher.GetHash($"{_userName}|{requestId}|{command}|{timestamp}|{_serverConfig.SaltKey}");
            parameters.PutSFSObject("data", data);
            parameters.PutInt("id", requestId);
            parameters.PutUtfString("hash", hash);
            parameters.PutLong("timestamp", timestamp);
            return parameters;
        }

        private void OnUserInitializedResponse(BaseEvent evt) {
            var cmd = (string)evt.Params["cmd"];
            if (cmd == SFSDefine.SFSCommand.USER_INITIALIZED) {
                IsUserInitSuccess = true;
                _logManager.Log("USER_INITIALIZED success");
                _serverBridge.RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnUserInitializedResponse);
            }
        }

        public void InitKickUserListener() {
            _serverBridge.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnServerKickUser);
        }

        private void OnServerKickUser(BaseEvent evt) {
            var cmd = (string)evt.Params["cmd"];
            if (cmd == SFSDefine.SFSCommand.USER_KICK) {
                _unityCommunication.UnityToReact.SendToReact(ReactCommand.LOGOUT);
            }
        }
    }
}