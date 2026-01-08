using System;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine.Assertions;

namespace Scenes.ConnectScene.Scripts.Connectors {
    /// <summary>
    /// Logic:
    /// 1. Nếu là Guest: -> Login bình thường
    /// 2. Nếu login sử dụng JWT:
    /// -> Nếu JWT chưa expire thì login bình thường
    /// -> Nếu JWT đã expire thì login lại bằng những cái đang có: username password || third party access token
    /// 3. Nếu ko có phương án nào khả thi: -> User phải chọn lại cách login
    /// </summary>
    public class ConnectContinueLoginController {
        private readonly ILogManager _logManager;
        private readonly IAuthManager _authManager;
        
        private readonly Action<UserAccount> _resolveCb;
        private readonly Action _rejectCb;
        private readonly UserAccount _userAccount;

        public ConnectContinueLoginController(
            ILogManager logManager,
            IAuthManager authManager,
            UserAccount userAccount,
            Action<UserAccount> resolveCb,
            Action rejectCb
        ) {
            _logManager = logManager;
            _userAccount = userAccount?.Clone();
            _authManager = authManager;
            _resolveCb = resolveCb;
            _rejectCb = rejectCb;
        }

        public void ToCheckRememberAccount() {
            var canAutoLogin = _userAccount != null && _userAccount.server != null && _userAccount.rememberMe;
            if (!canAutoLogin) {
                Canceled();
                return;
            }
            var t = _userAccount.loginType;
            switch (t) {
                case LoginType.UsernamePassword: {
                    CheckSensparkAccount();
                    break;
                }
                case LoginType.Apple: {
                    CheckThirdPartyAccount(ThirdPartyLogin.Apple);
                    break;
                }
                case LoginType.Guest: {
                    CheckGuestAccount();
                    break;
                }
                default: {
                    Canceled();
                    break;
                }
            }
        }
        
        private void CheckThirdPartyAccount(ThirdPartyLogin type) {
            var validUserName = !string.IsNullOrWhiteSpace(_userAccount.userName);
            var validJwt = _authManager.ValidateUserLoginToken(_userAccount.jwtToken);
            if (!validUserName) {
                Canceled();
                return;
            }
            switch (validJwt) {
                case JwtValidateResult.Valid:
                    Completed();
                    return;
                case JwtValidateResult.Expired:
                    ReLoginThirdParty(type, _userAccount.thirdPartyAccessToken);
                    break;
                default:
                    Canceled();
                    break;
            }
        }
        
        private void CheckSensparkAccount() {
            var validUserName = !string.IsNullOrWhiteSpace(_userAccount.userName);
            var validPassword = !string.IsNullOrWhiteSpace(_userAccount.password);
            if (!validUserName) {
                Canceled();
                return;
            }
            Completed();
        }
        
        private void CheckGuestAccount() {
            var validUserName = !string.IsNullOrWhiteSpace(_userAccount.userName);
            if (!validUserName) {
                Canceled();
                return;
            }
            Completed();
        }
        
        private void ReLoginThirdParty(ThirdPartyLogin type, string accessToken) {
            UniTask.Void(async () => {
                try {
                    var res = await _authManager.GetUserLoginDataByThirdParty(type, accessToken);
                    _userAccount.id = res.UserId;
                    _userAccount.userName = res.UsernameOrWallet;
                    _userAccount.jwtToken = res.JwtToken;
                    _userAccount.hasPasscode = res.HasPasscode;
                    Completed();
                } catch (Exception e) {
                    _logManager.Log(e.Message);
                    Canceled();
                }
            });
        }

        private void ReLoginSenspark(string userName, string password) {
            UniTask.Void(async () => {
                try {
                    var res = await _authManager.GetUserLoginDataByPassword(userName, password);
                    _userAccount.id = res.UserId;
                    _userAccount.jwtToken = res.JwtToken;
                    _userAccount.walletAddress = res.IsUserFi ? res.UsernameOrWallet : null;
                    _userAccount.isUserFi = res.IsUserFi;
                    _userAccount.hasPasscode = res.HasPasscode;
                    Completed();
                } catch (Exception e) {
                    _logManager.Log(e.Message);
                    Canceled();
                }
            });
        }

        private void Completed() {
            Assert.IsNotNull(_userAccount.userName);
            //Assert.IsTrue(_userAccount.id >= 0);
            Assert.IsNotNull(_userAccount.server);
            _resolveCb(_userAccount);
        }

        private void Canceled() {
            _rejectCb();
        }
    }
}