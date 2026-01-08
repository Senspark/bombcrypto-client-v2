using System;
using System.Threading.Tasks;
using App;
using Communicate;
using Communicate.Encrypt;
using Cysharp.Threading.Tasks;
using Exceptions;
using Newtonsoft.Json;
using Scenes.TreasureModeScene.Scripts.Service;
using Senspark;
using Share.Scripts.Communicate.UnityReact;
using Share.Scripts.CustomException;
using UnityEngine;

//*-----------------------------------------------------------
//.   **** LƯU Ý LOGIN CÓ ENCRYPT KIỂU MỚI. ****
//
//  1. Mỗi lần login hay reconnect đều gọi handshake của MasterUnityCommunication để có aes key giao tiếp giữa unity smartfox
//
//. 2. Handshake gồm 2 bước là handshake react và handshake smartfox   (phải làm theo thứ từ unity -> smartfox)
//.    2.1 Handshake react: mục đích để tạo easy key giao tiếp giữa unity - react
//.    2.2 Handshake smartfox: mục đích để lấy jwt login smartfox, và có aes key để giao tiếp giữa unity smartfox
//
//  3. jwt có hiệu lực 5 phút dùng để login mà ko cần verify lại thông tin đăng nhập
//    3.1 bản Web: có react template tự refresh mỗi 5' để khi unity gọi handshake sẽ luôn có jwt mới để unity login
//    3.2 bản mobile: ko có react template nên unity tự tạo object để mỗi 5' tự refresh để đảm bảo lúc nào cũng có jwt mới
//        => bản mobile chỉ có login account senspark và guest nên việc refresh jwt này chỉ để dùng khi cần reconnect,
//           còn nếu đã kết thúc 1 session thì lần đầu tiên login vẫn phải checkProof lại để đảm bảo password là đúng
//
//. 4. Trong MasterUnityCommunication có JwtSession để lưu lại các thông tin của 1 user ở 1 session để tiện sử dụng giữa các
//.    service khác nhau, thông tin này chỉ lưu tạm và tự mất khi refresh page, tắt app hoặc dev tự reset khi logout hoặc login vào acoount khác
//
//. 5. Các bản build ra webGL thì unity -> react -> api
//     Các bản build ra mobile thì unity -> api (Sử dung IMobileRequest)
//
//. 6. Sau khi handshake xong có jwt thì jwt này sẽ phải đc dính kèm xong header mỗi khi gọi api, bao gồm bản webGL vào mobile
//
//  7. Có sự khác nhau giữa jwtRaw và jwtLogin (lưu trong JwtSession)
//.  7.1 jwtRaw: là jwt sau khi đc handshake xong, đc tạo ra từ api chứa address hoặc userName và thời gian hết hạn,...
//.    ko dùng để login, dùng để đính kèm trong header
//.  7.2 jwtLogin là jwt đc unity tạo ra từ jwtRaw , aesKey (unity-smartFox) và các thông tin login cảu account để tạo thành
//.     jwtLogin, dùng để login vào smartfox, smartfox -> api để lấy ra các thông tin mà unity gửi để verify bao gồm cả jwtRaw trong đó,
//.     khi login thì jwt để gửi cho smartfox là jwtLogin đc lấy từ GetJwtForLogin
//
//   8. Mỗi khi login hoặc reLogin để có event USER_INITIALIZED từ smartfox, unity phải đợi event này trong OnExtensionResponse
//.     rồi sau đó mới đc gọi các command của smartfox
//
//*-----------------------------------------------------------

namespace Share.Scripts.Communicate {
    /// <summary>
    /// unityReact: Giao tiếp giữa unity và react (react có thể gọi api)
    /// unitySmartFox: Lấy Jwt login smartFox, encrypt và decrypt data giữa unity và smartFox
    /// mobileRequest: Giao tiếp giữa unity và api trên mobile
    /// </summary>
    public class MasterUnityCommunication : IMasterUnityCommunication {
        private readonly IUnityReactCommunication _unityReact;
        private readonly IUnitySmartFoxCommunication _unitySmartFox;
        private readonly ILogManager _logManager;
        private readonly IMobileRequest _mobileRequest;
        private readonly JwtSession _jwtSession = new();

        /// <summary>
        /// Init theo thứ tự mobile request -> unityReact -> unitySmartFox
        /// </summary>
        /// <param name="logManager"></param>
        public MasterUnityCommunication(ILogManager logManager) {
            _logManager = logManager;
            _mobileRequest = new DefaultMobileRequest(_jwtSession, logManager);
            _unityReact = InitializeUnityToReact();
            _unitySmartFox = InitializeUnitySmartFox();
        }
        
        public async UniTask<bool> RequestConnection() {
            try {
                return await _unityReact.RequestConnection();
            } catch (Exception e) {
                _logManager.Log($"RequestConnection error: {e.Message}");
                //Reset account đã lưu
                ResetSession();
                throw new Exception("Something went wrong\nPlease try again");
            }
        }
        
        public async UniTask RequestLoginData() {
            try {
                await _unityReact.RequestLoginData();
            } catch (Exception e) {
                _logManager.Log($"RequestLoginData error: {e.Message}");
                //Reset account đã lưu
                ResetSession();
                throw new Exception("Something went wrong\nPlease try again");
            }
        }

        /// <summary>
        /// B1: Gọi react trước để có aes key để unity, react giao tiếp về sau và setup các method cần thiết
        /// B2: Gọi smartFox để lấy jwt và setup aes key cho unity và smartFox giao tiếp về sau
        /// Note: Mỗi lần login hay reconnect lại đều phải gọi lại method này để tạo cặp key và jwt mới, data cũ sẽ ko login đc
        /// </summary>
        public async UniTask Handshake(HandshakeType type) {
            try {
                //Nếu là mobile thì phải check server có maintenance hay không ở đây
                if (AppConfig.IsMobile()) {
                    //true => server đang maintenance
                    var  result = await _mobileRequest.CheckServer();
                    if (result) {
                        throw new ServerMaintenanceException(10);
                    }
                    _logManager.Log("Server is ok");
                }
                await _unityReact.Handshake();
                await _unitySmartFox.Handshake(type);
#if UNITY_EDITOR
                Debug.Log("JWT: " + _unitySmartFox.GetJwtForLogin());
#endif
            } catch (Exception e) {
                _logManager.Log($"GetJwtFromReact error: {e.Message}");
                if (e is IncorrectPassword  or TonOldDataException or ServerMaintenanceException) {
                    throw;
                }
                if (e is InvalidJwtException) {
                    ResetSession();
                    App.Utils.KickToConnectScene();
                    return;
                }

                //Reset account đã lưu
                ResetSession();
                throw new Exception("Something went wrong\nPlease try again");
            }
        }

        public ISmartFoxSupportMethod SmartFox => _unitySmartFox;
        public IUnityReactSupportMethod UnityToReact => _unityReact.UnityToReact;
        public IReactToUnity ReactToUnity => _unityReact.ReactToUnity;
        public IMobileRequest MobileRequest => _mobileRequest;
        public IPublicJwtSession JwtSession => _jwtSession;
        

        /// <summary>
        /// Có 3 trường hợp cần reset session:
        /// 1. User refresh lại trang web (ton, sol, bsc)
        /// 2. User thoát app vào lại (mobile)
        /// 3. User logout để vào bằng 1 account khác
        /// </summary>
        public void ResetSession() {
            _jwtSession.Reset();
            AppConfig.Reset();
        }

        /// <summary>
        /// Tạo object để giao tiếp giữa unity và react
        /// </summary>
        /// <returns></returns>
        private IUnityReactCommunication InitializeUnityToReact() {
            // unity react hoán đổi vị trí data
            var obfuscate = new PermutationObfuscate(Secret.ReactPermutationOrder32);
            // unity react sử dụng bộ encryption riêng với smartFox
            var encryption = new UnityEncryption(obfuscate);
            return new UnityReactCommunication(encryption, _logManager, _mobileRequest, _jwtSession);
        }

        private IUnitySmartFoxCommunication InitializeUnitySmartFox() {
            if (_unityReact == null) {
                throw new Exception("Unity React not initialized");
            }
            // unity smartFox chèn thêm data
            var obfuscateAppend = new AppendBytesObfuscate(Secret.SmartFoxAppendBytes);
            // unity smartFox sử dụng bộ encryption riêng với react
            var encryption = new UnityEncryption(obfuscateAppend);
            return new UnitySmartFoxCommunication(encryption, _unityReact, _jwtSession, obfuscateAppend, _logManager);
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}

public record DataForReconnect {
    [JsonProperty("jwt")]
    public string Jwt;

    [JsonProperty("serverPublicKey")]
    public string ServerPublicKey;
}