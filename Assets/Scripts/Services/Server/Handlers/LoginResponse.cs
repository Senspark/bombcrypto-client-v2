using App;

using Data;

namespace Services.Server.Handlers {
    public class LoginResponse : ILoginResponse {
        public bool IsNewUser { get; }
        public string WalletAddress { get; }
        public string SecondUserName { get; }
        public string NickName { get; }
        public string TokenType { get; }
        public string IdTelegram { get; }
        public Platform Platform { get; }

        public LoginResponse(bool isNewUser, string walletAddress, string userName, string nickName,
            string tokenType) {
            IsNewUser = isNewUser;
            WalletAddress = walletAddress;
            SecondUserName = userName;
            NickName = nickName;
            TokenType = tokenType;
            IdTelegram = "";
            Platform = UserAccount.GetPlatform();
        }
    }
    
    public class LoginTelegramResponse : ILoginResponse {
        public bool IsNewUser { get; }
        public string WalletAddress { get; }
        public string SecondUserName { get; }
        public string NickName { get; }
        public string TokenType { get; }
        public string IdTelegram { get; }
        public Platform Platform { get; }

        public LoginTelegramResponse(string idTelegram) {
            IdTelegram = idTelegram;
            Platform = UserAccount.GetPlatform();
            IsNewUser = false;
            WalletAddress = "";
            SecondUserName = "";
            NickName = "";
            TokenType = "";
        }
    }
}