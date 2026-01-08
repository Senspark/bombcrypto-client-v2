using System;
using System.Threading.Tasks;
using GroupMainMenu;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Senspark;
using UnityEngine;
using UnityEngine.Assertions;
using Platform = Data.Platform;

namespace App {
    [Service(nameof(IUserAccountManager))]
    public interface IUserAccountManager : IService {
        [CanBeNull]
        UserAccount GetRememberedAccount();

        void RememberAccount(UserAccount account);
        void RememberUniqueGuestId(string userName, int id);

        bool HasAccount();
        bool HasAccGuest();
        
        /// <returns>string: username; int: userId</returns>
        (string, int) GetRememberUniqueGuestId();

        void EraseData();
        void EraseGuest();

        bool IsTermsServiceAccepted();
        void SetTermsServiceAccepted(bool accepted);
        bool IsLoginOnServerProduction();
        bool IsNewUserLogin { get; set; } // Dùng để xóa cache riêng của mỗi user
        public LoginType LoginType { get; set; }
    }

    public class DefaultUserAccountManager : IUserAccountManager {
        private const string Key = nameof(DefaultUserAccountManager);
        private static readonly string GuestUserNameKey = $"{Key}-GuestUserName";
        private static readonly string GuestUserIdKey = $"{Key}-GuestUserId";
        private static readonly string TermsServiceKey = $"{Key}-TermsService";
        private readonly ILogManager _logManager;
        private static UserAccount _cached;
        public bool IsNewUserLogin { get; set; }
        public LoginType LoginType { get; set; }

        public DefaultUserAccountManager(ILogManager logManager) {
            _cached = null;
            _logManager = logManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
        

        public UserAccount GetRememberedAccount() {
            try {
                if (_cached != null) {
                    return _cached;
                }
                var str = PlayerPrefs.GetString(Key);
                _logManager.Log(str);
                _cached = JsonConvert.DeserializeObject<UserAccount>(str);
                return _cached;
            } catch (Exception e) {
                _logManager.Log(e.Message);
                return null;
            }
        }

        public void RememberAccount(UserAccount account) {
            LoginType = account.loginType;
            var preAccount = GetRememberedAccount();
            if (preAccount != null) {
                if (preAccount.walletAddress != account.walletAddress) {
                    IsNewUserLogin = true;
                }
            }
            
            _cached = account;
            var str = JsonConvert.SerializeObject(account);
            PlayerPrefs.SetString(Key, str);
            if (IsNewUserLogin) {
                PlayerPrefs.DeleteKey(MMHeroChoose.BomberRankKey);
            }
            PlayerPrefs.Save();
        }

        public void EraseData() {
            _cached = null;
            IsNewUserLogin = true;
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.DeleteKey(MMHeroChoose.BomberRankKey);
            PlayerPrefs.Save();
        }

        public void EraseGuest() {
            PlayerPrefs.DeleteKey(GuestUserNameKey);
            PlayerPrefs.DeleteKey(GuestUserIdKey);
            PlayerPrefs.Save();
        }

        public void RememberUniqueGuestId(string userName, int id) {
            PlayerPrefs.SetString(GuestUserNameKey, userName);
            PlayerPrefs.SetInt(GuestUserIdKey, id);
            PlayerPrefs.Save();
        }

        public bool HasAccount() {
            var usr = GetRememberedAccount();
            return usr != null && usr.server != null;
        }

        public bool HasAccGuest() {
            var (guestId, userId) = GetRememberUniqueGuestId();
            return userId > 0;
        }

        public (string, int) GetRememberUniqueGuestId() {
            var username = PlayerPrefs.GetString(GuestUserNameKey);
            var userId = PlayerPrefs.GetInt(GuestUserIdKey, -1);
            return (username, userId);
        }

        public bool IsTermsServiceAccepted() {
            return PlayerPrefs.GetInt(TermsServiceKey, 0) == 1;
        }

        public void SetTermsServiceAccepted(bool accepted) {
            PlayerPrefs.SetInt(TermsServiceKey, accepted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public bool IsLoginOnServerProduction() {
            var usr = GetRememberedAccount();
            Assert.IsNotNull(usr);
            return ServerAddress.IsMainServerAddress(usr.server.Address);
        }
    }

    [Serializable]
    public class UserAccount {
        public int id = -1;
        public string userName;
        public string password;
        public bool rememberMe;
        public string email;

        public LoginType loginType;
        public bool isUserFi;
        public bool hasPasscode;

        public string jwtToken;
        public string walletAddress;
        public NetworkType network;
        public WalletType walletType;

        public string thirdPartyAccessToken;

        public ServerAddress.Info server;
        
        public string referralCode = "";
        public Platform platform = Platform.Unknown;
        public bool skipCheckAccount = false;

        public UserAccount Clone() {
            return new UserAccount {
                id = id,
                userName = userName,
                password = password,
                rememberMe = rememberMe,
                loginType = loginType,
                isUserFi = isUserFi,
                hasPasscode = hasPasscode,
                jwtToken = jwtToken,
                walletAddress = walletAddress,
                network = network,
                walletType = walletType,
                thirdPartyAccessToken = thirdPartyAccessToken,
                server = server,
                referralCode = referralCode
            };
        }

        public static string TryRemoveSuffixInUserName(string userName) {
            if (userName.EndsWith(" (VIP)")) {
                userName = userName.Substring(0, userName.Length - 6);
            }
            // Remove suffixes: " ron", " bas", " RON", " BAS" (case-insensitive)
            string[] suffixes = { "ron", "bas", "RON", "BAS", "vic", "VIC" };
            foreach (var suffix in suffixes) {
                if (userName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) {
                    userName = userName.Substring(0, userName.Length - suffix.Length);
                    break;
                }
            }
            return userName;
        }
        
        public static Platform GetPlatform() {
            var platform = Platform.Unknown;
            
            switch (Application.platform) {
                case RuntimePlatform.WebGLPlayer when !Application.isMobilePlatform && AppConfig.IsTon():
                    platform = Platform.WebTelegram;
                    break;
                case RuntimePlatform.WebGLPlayer when Application.isMobilePlatform && AppConfig.IsTon():
                    platform = Platform.MobileTelegram;
                    break;
                case RuntimePlatform.WebGLPlayer when !AppConfig.IsTon():
                    platform = Platform.WebPC;
                    break;
                case RuntimePlatform.Android when !AppConfig.IsTon():
                    platform = Platform.AndroidNative;
                    break;
                case RuntimePlatform.IPhonePlayer when !AppConfig.IsTon():
                    platform = Platform.IOSNative;
                    break;
                default:
                    platform = Application.isEditor ? Platform.Editor : Platform.WebOther;
                    break;
            }
            return platform;
        }
    }
    
    [Serializable]
    public class LoginAccount {
        public string userName;
        public string password;
        public string network;
    }
}