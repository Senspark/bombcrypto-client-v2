using System;

using App;

using Communicate;

using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.Communicate.UnityReact;

namespace Share.Scripts.Communicate.UnitySmartFox.LoginType {
    public class DefaultLoginWith : ILoginWith {
        private readonly ILogManager _logger;
        private readonly ILoginWith _loginWithBridge;
        private readonly IPublicJwtSession _jwtSession;

        public DefaultLoginWith(ILogManager logger, IMobileRequest mobileRequest, IPublicJwtSession jwtSession) {
            _logger = logger;
            _jwtSession = jwtSession;
            // userAccount chưa có => login bằng ví, lấy data từ ví rồi mới tạo userAccount
            _loginWithBridge = GetLoginType(jwtSession.Account, mobileRequest);
        }

        public UniTask<JwtData> GetJwtData(IUnityReactSupportMethod unityReact, HandshakeType type) {
            return _loginWithBridge.GetJwtData(unityReact, type);;
        }

        private ILoginWith GetLoginType(UserAccount userAccount, IMobileRequest mobileRequest) {
            // User mobile (account hoăc guest)
            // User này trên mobile nên ko có react phải gọi api trực tiếp
            // để có thông tin acount, nên lúc này userAccount là tạm
            // chỉ để biết account này là guest hay userName password
            if (AppConfig.IsMobile() && userAccount != null) {
                return new LoginWithMobile(
                    mobileRequest,
                    _jwtSession,
                    _logger);
            }
            if (AppConfig.IsWebAirdrop()) {
                return new LoginWithWallet(_logger);
            }

            // User Wallet (Ton, Solana, Bsc, Polygon)
            // user này phải gọi react mới có 
            // thông tin của account nên lúc này userAccount = null
            if (userAccount == null) {
                if (AppConfig.IsTon()) {
                    return new LoginWithTon(_logger);
                }
                if (AppConfig.IsWebGL() || AppConfig.IsSolana()) {
                    return new LoginWithWallet(_logger);
                }
                throw new ArgumentOutOfRangeException();
            }

            // User Bsc/Polygon Account web
            // user này phải nhập trước username và pass
            // nên lúc này có userAccount tạm lưu name và pass rồi mới gọi react để lấy các thông tin còn lại
            if (userAccount.loginType == App.LoginType.UsernamePassword) {
                return new LoginWithAccount(_jwtSession, _logger);
            }

            // Trường hợp có login truớc đó r nên có lưu account trong playerPref
            // Nên cần check ở đây nữa để đảm bảo ko sót case nào
            if (userAccount.loginType == App.LoginType.Telegram) {
                return new LoginWithTon(_logger);
            }
            if (userAccount.loginType == App.LoginType.Wallet || userAccount.loginType == App.LoginType.Solana) {
                return new LoginWithWallet(_logger);
            }

            _logger.Log("Login type not found");
            throw new ArgumentOutOfRangeException();
        }
    }
}