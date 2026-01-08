using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Communicate;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;

namespace Share.Scripts.Communicate {
    public class MobileRequest: IMobileRequest {
        private const string LocalHost = "http://localhost:8120";
        
        private const string CheckProofPost = "/login/mobile/check_proof";
        private const string CheckProofGuestPost = "/login/mobile/check_proof_guest";
        
        private const string CreateAccountSenspark = "/login/mobile/create_senspark_account";
        
        private const string CheckServerGet = "/login/mobile/check_server";
        private const string ChangeNickNameGet = "/login/mobile/change_nick_name";
        private const string CreateGuestAccountGet = "/login/mobile/create_guest_account";
        
        private const string RefreshJwtUrl = "/login/mobile/refresh";
        
        private readonly string _host;
        private readonly IPublicJwtSession _jwtSession;
        private static MobileRefreshJwtRunner _autoRefreshJwt;
        private static MobileCheckServerRunner _autoCheckServer;

        public MobileRequest(IJwtSession jwtSession, ILogManager logManager) {
            _jwtSession = jwtSession;
            if (Application.isEditor) {
                _host = LocalHost;
            }
            else if (!AppConfig.IsProduction) {
                _host = AppConfig.BaseApiTestHost;
            }
            else {
                _host = AppConfig.BaseApiHost;
            }
            CreateObjectAutoRefreshJwt(jwtSession, logManager);
            CreateObjectAutoCheckServer(logManager);
        }
        
        /// <summary>
        /// Tự refresh jwt mỗi 5p
        /// </summary>
        /// <param name="jwtSession"></param>
        /// <param name="logManager"></param>
        private void CreateObjectAutoRefreshJwt(IJwtSession jwtSession, ILogManager logManager) {
            // Ko xảy ra nhưng để đảm bảo ko tạo lại object này
            if(_autoRefreshJwt != null) {
                return;
            }
            //Tự động gọi api refresh jwt sau mỗi khoảng thời gian
            var objectRun = new GameObject("Auto refresh jwt");
            _autoRefreshJwt = objectRun.AddComponent<MobileRefreshJwtRunner>();
            if(_autoRefreshJwt != null) {
                _autoRefreshJwt.Setup(this, jwtSession, logManager);
                _autoRefreshJwt.Run();
            }
        }
        
        /// <summary>
        /// Check server có maintenance hay không mỗi 5p
        /// </summary>
        /// <param name="logManager"></param>
        private void CreateObjectAutoCheckServer(ILogManager logManager) {
            if(_autoCheckServer != null) {
                return;
            }
            var objectRun = new GameObject("Auto check server");
            _autoCheckServer = objectRun.AddComponent<MobileCheckServerRunner>();
            if(_autoCheckServer != null) {
                _autoCheckServer.Setup(this, logManager);
                _autoCheckServer.Run();
            }
        }
        
        public async UniTask<JwtDataMobile> CheckProof(string userName, string password) {
            var path = $"{_host}{CheckProofPost}";
            var data = new {
                userName,
                password
            };
            var response = await HttpClientHelper.SendPost<JwtDataMobile>(path, data);
            return response;
        }

        public UniTask<JwtDataMobile> RefreshJwtGuest(string userName) {
            var body = new {
                userName
            };
            return RefreshJwt(body);
        }

        public UniTask<JwtDataMobile> RefreshJwtAccount(string userName, string password) {
            var body = new {
                userName,
                password
            };
            return RefreshJwt(body);
        }

        private async UniTask<JwtDataMobile> RefreshJwt(object body) {
            var path = $"{_host}{RefreshJwtUrl}";

            var headers = new Dictionary<string, string> {
                { "Authorization", $"Bearer {_jwtSession.RawJwt}" },
            };
            return await HttpClientHelper.SendPost<JwtDataMobile>(path, body, headers);
        }

        public async UniTask<bool> CheckServer() {
            var path = $"{_host}{CheckServerGet}";
            var result = await HttpClientHelper.SendGet<bool>(path);
            return result;
        }

        public UniTask<bool> ChangeNickName(string userName, string newNickName) {
            var path = $"{_host}{ChangeNickNameGet}";
            var data = new {
                userName = userName,
                newNickName = newNickName
            };
            var headers = new Dictionary<string, string> {
                { "Authorization", $"Bearer {_jwtSession.RawJwt}" },
            };
            return HttpClientHelper.SendPost<bool>(path, data, headers);
        }

        public UniTask<GuestAccountCreated> CreateGuestAccount() {
            var path = $"{_host}{CreateGuestAccountGet}";
            return HttpClientHelper.SendGet<GuestAccountCreated>(path);
        }

        public UniTask<JwtDataMobile> CheckProofGuest(string userName) {
            var path = $"{_host}{CheckProofGuestPost}";
            var data = new {
                userName
            };
            return HttpClientHelper.SendPost<JwtDataMobile>(path, data);
        }

        public UniTask<int> RegisterAccountSenspark(string userName, string password, string email) {
            var path = $"{_host}{CreateAccountSenspark}";
            var data = new {
                userName = userName,
                password = password,
                email = email,
                userNameGuest = _jwtSession.Account.userName
            };
            var headers = new Dictionary<string, string> {
                { "Authorization", $"Bearer {_jwtSession.RawJwt}" },
            };
            return HttpClientHelper.SendPost<int>(path, data, headers);
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {}
    }
}