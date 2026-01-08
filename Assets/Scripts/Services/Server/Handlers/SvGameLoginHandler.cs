using System;
using System.Threading.Tasks;
using App;
using BLPvpMode.Manager.Api;
using CustomSmartFox;
using Senspark;
using Exceptions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;

using Share.Scripts.Communicate;

using SuperTiled2Unity;
using UnityEngine;
using UnityEngine.Assertions;
using DeviceType = App.DeviceType;
using IServerBridge = BLPvpMode.Manager.Api.IServerBridge;
using Platform = Data.Platform;

namespace Services.Server.Handlers {
    public class SvGameLoginHandler : IServerHandler<ILoginResponse> {
        private readonly ILogManager _logManager;
        private readonly IHasher _hasher;
        private readonly ILoginData _loginData;
        private readonly IExtResponseEncoder _encoder;
        private readonly IServerConfig _serverConfig;
        private readonly int _timeOutMs;
        private readonly ITaskDelay _taskDelay;
        private readonly bool _isForceLogin;

        [CanBeNull]
        private TaskCompletionSource<ILoginResponse> _tcs;

        public SvGameLoginHandler(
            ILogManager logManager,
            IHasher hasher,
            IServerConfig serverConfig,
            ITaskDelay taskDelay,
            ILoginData loginData,
            IExtResponseEncoder encoder,
            bool isForceLogin
        ) {
            _logManager = logManager;
            _hasher = hasher;
            _serverConfig = serverConfig;
            _encoder = encoder;
            _loginData = loginData;
            _taskDelay = taskDelay;
            _timeOutMs = (int)(loginData.TimeOut * 1000);
            _isForceLogin = isForceLogin;
        }

        public async Task<ILoginResponse> Start(IServerBridge bridge) {
            if (_tcs != null) {
                await _tcs.Task;
            }
            try {
                _tcs = new TaskCompletionSource<ILoginResponse>();
                var request = Login(_loginData);
                bridge.Send(request);
                var result = _timeOutMs > 0 ? await _tcs.TimeoutAfter(_timeOutMs, _taskDelay) : await _tcs.Task;
                return result;
            } finally {
                _tcs = null;
            }
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
            _tcs?.TrySetException(new Exception(reason));
        }

        public void OnLogin() {
            // do nothing
        }

        public void OnLoginError(int code, string message) {
            Assert.IsNotNull(_tcs);
            if (code == ErrorCode.USER_BANNED) {
                var be = new BanException(code, message, "User is banned");
                _tcs.TrySetException(be);
                return;
            }
            if (code == ErrorCode.KICK_BY_OTHER_DEVICE) {
                _tcs.TrySetException(new LoginException(LoginException.ErrorType.KickByOtherDevice, message));
                return;
            }
            // Sử dụng custom error AlREADY_LOGIN để đảm bảo ko bị đứng 50% do smartfox ko gửi lỗi USER_ALREADY_LOGGED_IN về
            if (code == ErrorCode.AlREADY_LOGIN) {
                _tcs.TrySetException(new LoginException(LoginException.ErrorType.AlreadyLogin, message));
                return;
            }
            if (code == ErrorCode.WRONG_VERSION) {
                _tcs.TrySetException(new LoginException(LoginException.ErrorType.WrongVersion, message));
                return;
            }

            if (code == ErrorCode.USER_ALREADY_LOGGED_IN) {
                _tcs.TrySetException(new LoginException(LoginException.ErrorType.AlreadyLogin, message));
                return;
            }
            
            _tcs.TrySetException(new Exception($"[{code}]: {message}"));
        }

        public void OnUdpInit(bool success) {
        }

        public void OnPingPong(int lagValue) {
        }

        public void OnRoomVariableUpdate(SFSRoom room) {
        }

        public void OnJoinRoom(SFSRoom room) {
        }

        public void OnExtensionResponse(string cmd, ISFSObject data) {
            if (cmd != SFSDefine.SFSCommand.USER_LOGIN) {
                return;
            }
            if (_tcs == null) {
                // maybe because of time out
                return;
            }
            if (ServerUtils.HasError(data)) {
                var ec = ServerUtils.GetErrorCode(data);

                var ex = new Exception($"Login Failed ({ec})");
                _tcs.TrySetException(ex);
                return;
            }
            var newUser = data.GetBool(SFSDefine.SFSField.NewUser);
            var walletAddress = data.GetUtfString("address");
            var miningToken = data.GetUtfString("token_type");
            var nickName = data.GetUtfString("name");
            var secondUserName = data.GetUtfString("second_username");
            var result = new LoginResponse(newUser, walletAddress, secondUserName, nickName, miningToken);
            _tcs.TrySetResult(result);
        }

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
            if (cmd != SFSDefine.SFSCommand.USER_LOGIN) {
                return;
            }
            if (_tcs == null) {
                // maybe because of time out
                return;
            }

            if (data.Length > 0) {
                try {
                    var (outData, json) = _encoder.DecodeDataToSfsObject(data);
                    var newUser = outData.GetBool(SFSDefine.SFSField.NewUser);
                    var walletAddress = outData.GetUtfString("address");
                    var miningToken = outData.GetUtfString("token_type");
                    var nickName = outData.GetUtfString("name");
                    var secondUserName = outData.GetUtfString("second_username");
                    var result = new LoginResponse(newUser, walletAddress, secondUserName, nickName, miningToken);
                    if (outData.ContainsKey("send_log")) {
                        var serverNotifyManager = ServiceLocator.Instance.Resolve<IServerNotifyManager>();
                        serverNotifyManager.DispatchEvent(e => e.OnClientSendLog?.Invoke());
                    }
                    _tcs.TrySetResult(result);
                } catch (Exception e) {
                    var ex = new Exception("Login Failed\n No data");
                    _tcs.TrySetException(ex);
                }
            } else {
                var ex = new Exception("Login Failed\n No data");
                _tcs.TrySetException(ex);
            }
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        }

        private LoginRequest Login(ILoginData data) {
            var t = data.LoginDataType;
            switch (t) {
                case LoginDataType.Bombcrypto: {
                    var d = (LoginDataBombcrypto)data;
                    return LoginBombcrypto(d.UserName, d.Password, string.Empty, d.Signature, d.LoginType, d.Slogan);
                }
                case LoginDataType.Bomberland: {
                    var d = (LoginDataBomberland)data;
                    return LoginBomberLand(d.LoginType, d.Network, d.JwtToken, d.UserName, d.Slogan, d.DeviceType);
                }
                case LoginDataType.Telegram: {
                    var d = (LoginDataTelegram)data;
                    return LoginTelegram(d.UserName, d.LoginType, d.Token, d.UserData, d.DeviceType, d.ReferralCode,
                        d.Platform);
                }
                case LoginDataType.Solana: {
                    var d = (LoginDataSolana)data;
                    return LoginSolana(d.UserName, d.LoginType, d.Token, d.UserData, d.DeviceType, d.Platform);
                }
                default:
                    throw new ArgumentOutOfRangeException($"{t}/{nameof(LoginDataType)}");
            }
        }

        private LoginRequest LoginBombcrypto(
            string username,
            string password,
            string activationCode,
            string signature,
            LoginType loginType,
            string slogan
        ) {
            signature ??= string.Empty;
            var parameters = new SFSObject();
            var timestamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            var hash = _hasher.GetHash($"{username.ToLower()}|LOGIN|{timestamp}|{_serverConfig.SaltKey}");
            var data = new SFSObject();
            data.PutUtfString(SFSDefine.SFSField.PLAYER_NAME, username);
            data.PutUtfString(SFSDefine.SFSField.PASSWORD, password);
            data.PutInt(SFSDefine.SFSField.VERSION_CODE, _serverConfig.Version);
            data.PutInt(SFSDefine.SFSField.LOGIN_TYPE, (int)loginType);
            data.PutUtfString(SFSDefine.SFSField.SLOGAN, slogan);
            data.PutUtfString(SFSDefine.SFSField.ACTIVATION_CODE, activationCode);
            parameters.PutUtfString(SFSDefine.SFSField.LOGIN_CODE, "" /* FIXME */);
            data.PutInt(SFSDefine.SFSField.PLATFORM, (int)UserAccount.GetPlatform());

            parameters.PutSFSObject("data", data);
            parameters.PutUtfString("hash", hash);
            parameters.PutLong("timestamp", timestamp);
            data.PutUtfString(SFSDefine.SFSField.SIGNATURE, signature);

            _logManager.Log($"Send CMD=LOGIN, data={parameters.ToJson()}");
            return new LoginRequest(username, string.Empty, _serverConfig.Zone, parameters);
        }

        private LoginRequest LoginBomberLand(
            int loginType,
            string network,
            string jwt,
            string username,
            string slogan,
            DeviceType deviceType
        ) {
            network ??= loginType == (int)LoginType.Guest ? "GUEST" : "TR";
            jwt ??= string.Empty;
            username ??= string.Empty;
            var dv = deviceType switch {
                DeviceType.Mobile => "MOBILE",
                DeviceType.Web => "WEB",
                _ => throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null)
            };

            var parameters = new SFSObject();
            var timestamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            var hash = _hasher.GetHash($"{username.ToLower()}|LOGIN|{timestamp}|{_serverConfig.SaltKey}");
            var data = new SFSObject();
            data.PutInt(SFSDefine.SFSField.VERSION_CODE, _serverConfig.Version);
            data.PutUtfString(SFSDefine.SFSField.SLOGAN, slogan);
            data.PutUtfString(SFSDefine.SFSField.PLAYER_NAME, username);
            data.PutInt(SFSDefine.SFSField.LOGIN_TYPE, loginType);
            data.PutInt(SFSDefine.SFSField.PLATFORM, (int)UserAccount.GetPlatform());

            data.PutUtfString("data_type", network);
            data.PutUtfString(SFSDefine.SFSField.LoginTokenData, jwt);
            data.PutUtfString("device_type", dv);

            parameters.PutSFSObject("data", data);
            parameters.PutUtfString("hash", hash);
            parameters.PutLong("timestamp", timestamp);

            _logManager.Log($"Send CMD=LOGIN, data={parameters.ToJson()}");
            var loginUsername = username;
            if (IsNeedAddSuffixName(username, network)) {
                loginUsername = username + network.ToLower();
            }
            return new LoginRequest(loginUsername, "", _serverConfig.Zone, parameters);
        }
        
        // Chỉ add network nếu là ví 
        private bool IsNeedAddSuffixName(string userName, string network) {
            // if (AppConfig.IsWebAirdrop()) {
            //     return true;
            // }
            // if (AppConfig.IsBscOrPolygon()) {
            //     return true;
            // }
            // Account fi cũng sẽ phải thêm suffix network vào cuối
            if (network == "BSC" || network == "POLYGON") {
                return true;
            }
            if (userName.StartsWith("0x") && userName.Length == 42) {
                return true;
            }
            return false;
        }

        private LoginRequest LoginTelegram(
            string userName,
            int loginType,
            string jwt,
            string userData,
            DeviceType deviceType,
            string referralCode,
            Platform platform
        ) {
            var dv = deviceType switch {
                DeviceType.Mobile => "MOBILE",
                DeviceType.Web => "WEB",
                _ => throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null)
            };
            jwt ??= string.Empty;
            userData ??= string.Empty;
            referralCode ??= string.Empty;
            var hexWallet = userData ?? string.Empty;;
            var parameters = new SFSObject();
            var timestamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            var data = new SFSObject();
            data.PutInt(SFSDefine.SFSField.VERSION_CODE, _serverConfig.Version);
            data.PutInt(SFSDefine.SFSField.LOGIN_TYPE, loginType);
            data.PutInt(SFSDefine.SFSField.PLATFORM, (int)platform);
            data.PutUtfString(SFSDefine.SFSField.PLAYER_NAME, userName);
            data.PutUtfString("data_type", nameof(NetworkTypeInServer.TON));
            data.PutUtfString("device_type", dv);
            data.PutUtfString(SFSDefine.SFSField.LoginTokenData, jwt);
            if (!referralCode.IsEmpty()) {
                data.PutUtfString(SFSDefine.SFSField.REFERRAL_CODE, referralCode);
            }
            // kick login ở máy khác, login ở máy này
            if (_isForceLogin) {
                parameters.PutBool("force_login", true);
            }

            parameters.PutSFSObject("data", data);
            parameters.PutLong("timestamp", timestamp);

            _logManager.Log($"Send CMD=LOGIN, data={parameters.ToJson()}");
            //userNameForServer là userName để lưu trong databasa (hex), username là đia chỉ ví hiện cho user trong game
            return new LoginRequest(hexWallet, "", _serverConfig.Zone, parameters);
        }

        private LoginRequest LoginSolana(
            string userName,
            int loginType,
            string jwt,
            string userData,
            DeviceType deviceType,
            Platform platform
        ) {
            var dv = deviceType switch {
                DeviceType.Mobile => "MOBILE",
                DeviceType.Web => "WEB",
                _ => throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null)
            };
            jwt ??= string.Empty;
            userData ??= string.Empty;

            var userNameForServer = userName;

            var parameters = new SFSObject();
            var timestamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            var data = new SFSObject();
            data.PutInt(SFSDefine.SFSField.VERSION_CODE, _serverConfig.Version);
            data.PutInt(SFSDefine.SFSField.LOGIN_TYPE, loginType);
            data.PutInt(SFSDefine.SFSField.PLATFORM, (int)platform);
            data.PutUtfString(SFSDefine.SFSField.TELEGRAM_USER_DATA, "no data");
            data.PutUtfString(SFSDefine.SFSField.PLAYER_NAME, userName);
            data.PutUtfString("data_type", NetworkTypeInServer.SOL.ToString());
            data.PutUtfString("device_type", dv);
            data.PutUtfString(SFSDefine.SFSField.LoginTokenData, jwt);

            // kick login ở máy khác, login ở máy này
            if (_isForceLogin) {
                parameters.PutBool("force_login", true);
            }

            parameters.PutSFSObject("data", data);
            parameters.PutLong("timestamp", timestamp);

            _logManager.Log($"Send CMD=LOGIN, data={parameters.ToJson()}");
            //userNameForServer là userName để lưu trong databasa (hex), username là đia chỉ ví hiện cho user trong game
            return new LoginRequest(userNameForServer, "", _serverConfig.Zone, parameters);
        }
    }
}