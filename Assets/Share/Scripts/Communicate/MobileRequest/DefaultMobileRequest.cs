using System.Threading.Tasks;

using App;

using Communicate;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;

namespace Share.Scripts.Communicate {
    public class DefaultMobileRequest: IMobileRequest {
        private readonly IMobileRequest _mobileRequest;
        
        public DefaultMobileRequest(IJwtSession jwtSession, ILogManager logManager) {
            if (AppConfig.IsMobile() || Application.isEditor) {
                _mobileRequest = new MobileRequest(jwtSession, logManager);
            } 
            else {
                _mobileRequest = new NullRequest();
            }
        }
        public UniTask<JwtDataMobile> CheckProof(string userName, string password) {
            return _mobileRequest.CheckProof(userName, password);
        }

        public UniTask<JwtDataMobile> RefreshJwtGuest(string userName) {
            return _mobileRequest.RefreshJwtGuest(userName);
        }

        public UniTask<JwtDataMobile> RefreshJwtAccount(string userName, string password) {
            return _mobileRequest.RefreshJwtAccount(userName, password);
        }
        

        public UniTask<bool> CheckServer() {
            return _mobileRequest.CheckServer();
        }

        public UniTask<bool> ChangeNickName(string userName, string newNickName) {
            return _mobileRequest.ChangeNickName(userName, newNickName);
        }

        public UniTask<GuestAccountCreated> CreateGuestAccount() {
            return _mobileRequest.CreateGuestAccount();
        }

        public UniTask<JwtDataMobile> CheckProofGuest(string userName) {
            return _mobileRequest.CheckProofGuest(userName);
        }

        public UniTask<int> RegisterAccountSenspark(string userName, string password, string email) {
            return _mobileRequest.RegisterAccountSenspark(userName, password, email);
        }

        public Task<bool> Initialize() {
            return _mobileRequest.Initialize();
        }

        public void Destroy() {
            _mobileRequest.Destroy();
        }
    }
}