using System.Threading.Tasks;

using Newtonsoft.Json;

using Senspark;

namespace App {
    /// <summary>
    /// Only for BomberLand
    /// </summary>
    [Service(nameof(IAuthManager))]
    public interface IAuthManager : IService {
        Task<UserLoginToken> Register(string username, string password, string email);
        Task ForgotPassword(string email);
        Task ResetPassword(string token, string newPassword);
        Task Rename(string jwt, string nickName);

        /// <summary>
        /// Gets auth token by signing wallet.
        /// </summary>
        Task<string> GetUserJwtTokenBySign(int networkChainId);

        /// <summary>
        /// Logs in with username and password (Senspark account).
        /// </summary>
        Task<UserLoginToken> GetUserLoginDataByPassword(string username, string password);

        /// <summary>
        /// Logs in with wallet.
        /// </summary>
        /// <param name="networkChainId"></param>
        /// <returns></returns>
        Task<UserLoginToken> GetUserLoginDataBySign(int networkChainId);

        Task<UserLoginToken> GetUserLoginDataByThirdParty(ThirdPartyLogin type);
        Task<UserLoginToken> GetUserLoginDataByThirdParty(ThirdPartyLogin type, string accessToken);
        Task<bool> SetAccount(string jwt, string userName, string password, string email);
        Task<UserLoginToken> RequestNewGuestAccountUsername();
        JwtValidateResult ValidateUserLoginToken(string jwt);
    }

    public class UserLoginTokenDecoded {
        [JsonProperty("id")]
        public int UserId { get; private set; }

        [JsonProperty("address")]
        public string WalletAddress { get; private set; }

        [JsonProperty("username")]
        public string UserName { get; private set; }

        [JsonProperty("isFiAccount")]
        public bool IsFiAccount { get; private set; }

        [JsonProperty("hasPasscode")]
        public bool HasPasscode { get; private set; }

        [JsonProperty("iat")]
        public long EpochCreated { get; private set; }

        [JsonProperty("exp")]
        public long EpochExpired { get; private set; }
    }

    public class UserLoginToken {
        public readonly string JwtToken;
        public readonly string ThirdPartyAccessToken;
        public readonly int UserId;
        public readonly bool IsUserFi;
        public readonly string UsernameOrWallet;
        public readonly bool HasPasscode;

        public UserLoginToken(
            string jwtToken,
            string thirdPartyAccessToken,
            int userId,
            bool isUserFi,
            string usernameOrWallet,
            bool hasPasscode
        ) {
            JwtToken = jwtToken;
            ThirdPartyAccessToken = thirdPartyAccessToken;
            UserId = userId;
            IsUserFi = isUserFi;
            UsernameOrWallet = usernameOrWallet;
            HasPasscode = hasPasscode;
        }
    }

    public enum JwtValidateResult {
        Invalid,
        Expired,
        Valid
    }

    public enum ThirdPartyLogin {
        Apple,
        Telegram,
        Solana
    }
}