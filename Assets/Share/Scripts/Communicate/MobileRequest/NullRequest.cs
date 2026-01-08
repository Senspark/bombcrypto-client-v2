using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

namespace Share.Scripts.Communicate {
    public class NullRequest: IMobileRequest {
        public UniTask<JwtDataMobile> CheckProof(string userName, string password) {
            throw new System.NotImplementedException();
        }

        public UniTask<JwtDataMobile> RefreshJwtGuest(string userName) {
            throw new System.NotImplementedException();
        }

        public UniTask<JwtDataMobile> RefreshJwtAccount(string userName, string password) {
            throw new System.NotImplementedException();
        }

        public UniTask<bool> CheckServer() {
            throw new System.NotImplementedException();
        }

        public UniTask<bool> ChangeNickName(string userName, string newNickName) {
            throw new System.NotImplementedException();
        }

        public UniTask<GuestAccountCreated> CreateGuestAccount() {
            throw new System.NotImplementedException();
        }

        public UniTask<JwtDataMobile> CheckProofGuest(string userName) {
            throw new System.NotImplementedException();
        }

        public UniTask<int> RegisterAccountSenspark(string userName, string password, string email) {
            throw new System.NotImplementedException();
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {}
    }
}