using System;
using System.Threading.Tasks;

using CustomSmartFox;

using Engine.Utils;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Senspark;

using Share.Scripts;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using UnityEngine;

using Utils;

namespace App {
    public class DefaultAuthManager : IAuthManager {
        private const string HOST_LOCAL = "http://127.0.0.1:8000";
        
        private const string PostRegisterTraditionUrl = "/gateway/auth/tr/register";
        private const string PostLoginTraditionUrl = "/gateway/auth/tr/login";
        private const string GetVerifyJwtTraditionUrl = "/gateway/auth/tr/verify-token";
        private const string GetSignTokenUrl = "/gateway/auth/dapp/token?address=";
        private const string PostVerifyTokenUrl = "/gateway/auth/dapp/verify-signature";
        private const string PostSetAccountUrl = "/gateway/auth/dapp/set-account";
        private const string PostSetNickNameUrl = "/gateway/user/set-nickname";
        private const string PostLoginFacebook = "/gateway/auth/third-party/login/facebook";
        private const string PostLoginApple = "/gateway/auth/third-party/login/apple";
        private const string GetRequestCreateGuestAccount = "/gateway/auth/guest/request-create-account";
        private const string GetForgotPassword = "/gateway/auth/tr/send-mail-reset-password?email=";
        private const string PostResetPassword = "/gateway/auth/tr/reset-password";

        private readonly string _host;
        private readonly ILogManager _logManager;
        private readonly ISignManager _signManager;
        private readonly IExtResponseEncoder _encoder;
        private readonly IMasterUnityCommunication _unityCommunication;
        private UserLoginToken _tokenData;
        private readonly JavascriptProcessor _processor;
        
        public DefaultAuthManager(
            ILogManager logManager,
            ISignManager signManager,
            IExtResponseEncoder encoder,
            IMasterUnityCommunication unityCommunication,
            bool isProduction) {
            _logManager = logManager;
            _signManager = signManager;
            _encoder = encoder;
            _unityCommunication = unityCommunication;
            
            _host = isProduction ? AppConfig.AuthApiHostProduction : AppConfig.AuthApiHostTest;
            
            _processor = JavascriptProcessor.Instance;
            // _host = HOST_LOCAL;
        }
        
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task<UserLoginToken> Register(string username, string password, string email) {
            await RegisterSenspark(username, password, email);
            var jwt = await GetUserJwtTokenByPassword(username, password);
            var tokenData = await VerifyJwtToken(jwt);
            _tokenData = tokenData;
            return tokenData;
        }

        public async Task ForgotPassword(string email) {
            var url = $"{_host}{GetForgotPassword}{email}";
            var (code, resStr) = await Utils.GetWebResponse(_logManager, url);
            
            if (code != 200) {
                var err = ApiError.Parse(resStr);
                throw new Exception($"Request Failed ({code}):{Environment.NewLine}{err.Message}"); 
            }
            var resp = JsonConvert.DeserializeObject<GeneralResponse<bool>>(resStr);
            if (!resp.Data) {
                throw new Exception($"Request Failed");
            }
        }

        public async Task ResetPassword(string token, string newPassword) {
            var url = $"{_host}{PostResetPassword}";
            var body = new JObject {
                {"token", token},
                {"newPassword", newPassword},
            };
            var (code, resStr) = await Utils.PostWebResponse(_logManager, url, body.ToString());
            if (code != 200) {
                var err = ApiError.Parse(resStr);
                throw new Exception($"Request Failed ({code}):{Environment.NewLine}{err.Message}"); 
            }
            var resp = JsonConvert.DeserializeObject<GeneralResponse<bool>>(resStr);
            if (!resp.Data) {
                throw new Exception($"Request Failed");
            }
        }

        public async Task Rename(string jwt, string nickName) {
            var url = $"{_host}{PostSetNickNameUrl}";
            var headerTitle = "Authorization";
            var headerContent = $"Bearer {jwt}";
            var body = new JObject {
                {"nickname", nickName},
            };
            var (code, resStr) =
                await Utils.PostWebResponse(_logManager, url, body.ToString(), headerTitle, headerContent);
            
            if (code != 200) {
                var err = ApiError.Parse(resStr);
                throw new Exception($"Rename Failed ({code}):{Environment.NewLine}{err.Message}"); 
            }
            var resp = JsonConvert.DeserializeObject<GeneralResponse<bool>>(resStr);
            if (!resp.Data) {
                throw new Exception($"Rename Failed");
            }
        }

        public async Task<string> GetUserJwtTokenBySign(int networkChainId) {
            // Lấy walletAddress
            await _signManager.IsValidChainId(networkChainId);
            var walletAddress = await _signManager.ConnectAccount();
            
            // Sign Message
            var url = $"{_host}{GetSignTokenUrl}{walletAddress}";
            var (statusCode, resStr) = await Utils.GetWebResponse(_logManager, url);
            
            var failException = new Exception($"Get sign token failed ({statusCode})");
            var signToken = ParseResponse<string>(statusCode, resStr, failException);
            
            // mang sign token to sign
            var signature = await _signManager.Sign(signToken, walletAddress);
            
            // Get JWT Token
            url = $"{_host}{PostVerifyTokenUrl}";
            var body = new JObject {
                {"address", walletAddress},
                {"signature", signature}
            };
            (statusCode, resStr) = await Utils.PostWebResponse(_logManager, url, body.ToString());
            
            failException = new Exception($"Verify failed ({statusCode})");
            var message = ParseResponse<JObject>(statusCode, resStr, failException);
            var jwt = (string) message["token"];
            return jwt;
        }

        public async Task<UserLoginToken> GetUserLoginDataByPassword(string username, string password) {
            var jwt = await GetUserJwtTokenByPassword(username, password);
            var tokenData = await VerifyJwtToken(jwt);
            _tokenData = tokenData;
            return tokenData;
        }

        public async Task<UserLoginToken> GetUserLoginDataBySign(int networkChainId) {
            var jwt = await GetUserJwtTokenBySign(networkChainId);
            var tokenData = await VerifyJwtToken(jwt);
            _tokenData = tokenData;
            return tokenData;
        }

        public Task<UserLoginToken> GetUserLoginDataByThirdParty(ThirdPartyLogin type) {
            return type switch {
                ThirdPartyLogin.Apple => GetUserLoginDataByApple(),
                ThirdPartyLogin.Telegram => GetUserLoginDataByTelegram(),
                ThirdPartyLogin.Solana => GetUserLoginDataBySolana(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public Task<UserLoginToken> GetUserLoginDataByThirdParty(ThirdPartyLogin type, string accessToken) {
            return type switch {
                ThirdPartyLogin.Apple => GetUserLoginDataByApple(accessToken),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public async Task<bool> SetAccount(string jwt, string userName, string password, string email) {
            var url = $"{_host}{PostSetAccountUrl}";
            var headerTitle = "Authorization";
            var headerContent = $"Bearer {jwt}";
            var body = new JObject {
                {"username", userName},
                {"password", password},
                {"email", email}
            };
            var (statusCode, resStr) =
                await Utils.PostWebResponse(_logManager, url, body.ToString(), headerTitle, headerContent);
            return statusCode == 200;
        }

        public async Task<UserLoginToken> RequestNewGuestAccountUsername() {
            var data = await _unityCommunication.MobileRequest.CreateGuestAccount();
            int.TryParse(data.UserId, out var uid);
            return new UserLoginToken(null, null, uid, false, data.UserName, false);
        }

        public JwtValidateResult ValidateUserLoginToken(string jwt) {
            if (string.IsNullOrWhiteSpace(jwt)) {
                return JwtValidateResult.Invalid;
            }
            try {
                var decoded = JsonWebToken.DecodeToObject<UserLoginTokenDecoded>(jwt, string.Empty, false);
                var now = DateTime.Now.ToEpochSeconds();
                return decoded.EpochExpired > now ? JwtValidateResult.Valid : JwtValidateResult.Expired;
            } catch (Exception e) {
                _logManager.Log(e.Message);
                return JwtValidateResult.Invalid;
            }
        }

        #region PRIVATE
        
        /// <summary>
        /// Register Senspark Account
        /// </summary>
        private async Task RegisterSenspark(string username, string password, string email) {
            var url = $"{_host}{PostRegisterTraditionUrl}";
            var body = new JObject {
                {"username", username},
                {"password", password},
                {"email", email},
            };
            var (code, resStr) = await Utils.PostWebResponse(_logManager, url, body.ToString());
            if (code != 200) {
                var err = ApiError.Parse(resStr);
                throw new Exception($"Register Failed ({code}):{Environment.NewLine}{err.Message}"); 
            }
            var resp = JsonConvert.DeserializeObject<GeneralResponse<bool>>(resStr);
            if (!resp.Data) {
                throw new Exception($"Register Failed");
            }
        }
        
        /// <summary>
        /// Verifies auth token.
        /// </summary>
        private async Task<UserLoginToken> VerifyJwtToken(string token) {
            var url = $"{_host}{GetVerifyJwtTraditionUrl}";
            const string headerTitle = "Authorization";
            var headerContent = $"Bearer {token}";
            var (code, resStr) = await Utils.GetWebResponse(_logManager, url, headerTitle, headerContent);

            var failException = new Exception($"Login failed ({code})");
            var resObj = ParseResponse<JObject>(code, resStr, failException);
            var decoded = resObj.ToObject<UserLoginTokenDecoded>();
            var uw = decoded.IsFiAccount ? decoded.WalletAddress : decoded.UserName;
            return new UserLoginToken(token, null, decoded.UserId, decoded.IsFiAccount, uw, decoded.HasPasscode);
        }
        
        /// <summary>
        /// Gets Auth Token by username and password (Senspark account).
        /// </summary>
        private async Task<string> GetUserJwtTokenByPassword(string username, string password) {
            var url = $"{_host}{PostLoginTraditionUrl}";
            var body = new JObject {
                {"username", username},
                {"password", password}
            };
            var (code, resStr) = await Utils.PostWebResponse(_logManager, url, body.ToString());
            if (code == 401) {
                throw new Exception($"Username or password is incorrect ({code})");
            }
            var failException = new Exception($"Login failed ({code})");
            var message = ParseResponse<JObject>(code, resStr, failException);
            var jwt = (string) message["token"];
            return jwt;
        }
        
        
        private async Task<UserLoginToken> GetUserLoginDataByFacebook(string accessToken) {
            var url = $"{_host}{PostLoginFacebook}";
            var jwt = await GetSensparkJwtByThirdPartyToken(url, accessToken);
            var tokenData = await VerifyJwtToken(jwt);
            var newData = new UserLoginToken(tokenData.JwtToken, accessToken, tokenData.UserId, false,
                tokenData.UsernameOrWallet, tokenData.HasPasscode);
            _tokenData = newData;
            return newData;
        }

        private async Task<UserLoginToken> GetUserLoginDataByApple() {
            var app = new AppleLogin();
            var res = await app.GetAccessToken();
            app.Destroy();
            _logManager.Log($"{res.UserId} {res.AccessToken}");
            return await GetUserLoginDataByApple(res.AccessToken);
        }
        
        private Task<UserLoginToken> GetUserLoginDataByTelegram() {
            // string data = "";
            // if(Application.isEditor) {
            //     data =
            //         "{\"data\":{\"id\":6044343741,\"first_name\":\"Tai\",\"last_name\":\"Doan Anh\",\"username\":\"taidoananh\",\"language_code\":\"en\",\"allows_write_to_pm\":true}," +
            //         "\"user\":\"query_id=AAG9XUVoAgAAAL1dRWirKwsD&user=%7B%22id%22%3A6044343741%2C%22first_name%22%3A%22Sang%22%2C%22last_name%22%3A%22Qu%C3%A1ch%20Ho%C3%A0ng%22%2C%22username%22%3A%22qhsangst%22%2C%22language_code%22%3A%22vi%22%2C%22allows_write_to_pm%22%3Atrue%2C%22photo_url%22%3A%22https%3A%5C%2F%5C%2Ft.me%5C%2Fi%5C%2Fuserpic%5C%2F320%5C%2FFMjbwE5VB0J1D79jZAjtAWxlwKVDFUn7XS2B4Lh_08eDqg5FWhcVNtGhvPK1Iyfs.svg%22%7D&auth_date=1731554655&hash=c3676be7efdb23a62f4c138afcf0a8fe0aa2ab7da332f819c0ee595f893e3299\"," +
            //         "\"token\":\"eyJhbGciOiJIUzI1NiJ9.eyJhZGRyZXNzIjoiMDphMjQxYWU2YmRiMDIzYmEwYzBlMzNhZTkxY2FlNDBlOGMzZTg3ODVhOWIzNzg2ODkwNDc3YTMzZjliODZiNzRjIiwibmV0d29yayI6Ii0yMzkiLCJpYXQiOjE3MjY0NzY2NjEsImV4cCI6MTc1ODAzNDI2MX0.XDeXJ2H6TXfgyE5ucTwNxzoiur6iMImpSgnbQdngahs\"," +
            //         "\"wallet\":\"UQCiQa5r2wI7oMDjOukcrkDow-h4Wps3hokEd6M_m4a3TLkr\"," +
            //         "\"wallet_hex\":\"0:a241ae6bdb023ba0c0e33ae91cae40e8c3e8785a9b3786890477a33f9b86b74c\"}";
            // } 
            // else {
            //     data = await _processor.CallMethod("Get_Data_Telegram");
            // }
            
            var extraData = _unityCommunication.JwtSession.ExtraData;
            var jwt = _unityCommunication.SmartFox.GetJwtForLogin();
            
            
            //walletHex là username dùng để login và lưu data base trên server
            //userDataRawString là toàn bộ data user telegram
            return Task.FromResult(
                new UserLoginToken(
                jwt,
                extraData.WalletHex,
                1,
                false,
                extraData.WalletAddress,
                false
            ));
        }
        
        private Task<UserLoginToken> GetUserLoginDataBySolana() {

            var wallet =  _unityCommunication.JwtSession.ExtraData.WalletAddress;
            //Hiện ko có extra data gì cho login solana
            var jwt =  _unityCommunication.SmartFox.GetJwtForLogin();

            return Task.FromResult(new UserLoginToken(
                jwt,
                "thirdPartyAccessToken",
                1,
                true,
                wallet,
                false
            ));
        }
        
        private async Task<UserLoginToken> GetUserLoginDataByApple(string accessToken) {
            var url = $"{_host}{PostLoginApple}";
            var jwt = await GetSensparkJwtByThirdPartyToken(url, accessToken);
            var tokenData = await VerifyJwtToken(jwt);
            var newData = new UserLoginToken(tokenData.JwtToken, accessToken, tokenData.UserId, false,
                tokenData.UsernameOrWallet, tokenData.HasPasscode);
            _tokenData = newData;
            return newData;
        }
        
        private async Task<string> GetSensparkJwtByThirdPartyToken(string url, string token) {
            var body = new JObject {
                {"input_token", token},
            };
            var (code, resStr) = await Utils.PostWebResponse(_logManager, url, body.ToString());
            var failException = new Exception($"Login failed ({code})");
            var message = ParseResponse<JObject>(code, resStr, failException);
            var jwt = message["token"].Value<string>();
            return jwt;
        }
        
        private static T ParseResponse<T>(long code, string str, Exception failException) {
            if (code != 200) {
                throw failException;
            }
            try {
                var json = JObject.Parse(str);
                return json["message"].Value<T>();
            } catch (Exception e) {
                throw failException;
            }
        }
        
        #endregion

        private class GeneralResponse<T> {
            public readonly int StatusCode;
            public readonly T Data;

            [JsonConstructor]
            public GeneralResponse(
                [JsonProperty("statusCode")] int statusCode,
                [JsonProperty("message")] T data
            ) {
                StatusCode = statusCode;
                Data = data;
            }
        }
        
        [Serializable]
        private class GuestUserData
        {
            [JsonProperty("id")]
            public int userId;
            [JsonProperty("username")]
            public string userName;
        }
        
        [Serializable]
        private class TelegramUserData
        {
            [JsonProperty("id")]
            public long id;
            [JsonProperty("first_name")]
            public string firstName;
            [JsonProperty("last_name")]
            public string lastName;
            [JsonProperty("username")]
            public string userName;
        }
        [Serializable]
        private class TelegramData
        {
            [JsonProperty("data")]
            public TelegramUserData userData;
            
            [JsonProperty("user")]
            public string userDataRawString = "";

            [JsonProperty("token")]
            public string token;
            
            [JsonProperty("wallet")]
            public string walletAddress;
            
            [JsonProperty("wallet_hex")]
            public string walletHex;
        }
        
        public class UserTelegramData
        {
            public string WalletHex { get; set; }

            public UserTelegramData(string walletHex)
            {
                WalletHex = walletHex;
            }
        }
    }
}