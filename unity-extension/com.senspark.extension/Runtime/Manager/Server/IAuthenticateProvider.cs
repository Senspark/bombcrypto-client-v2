using Cysharp.Threading.Tasks;

namespace Senspark {
    public interface IAuthenticateProvider {
        /// <summary>
        /// Mã đăng nhập của người dùng
        /// </summary>
        UniTask<string> GetAuthCode();
    }
}