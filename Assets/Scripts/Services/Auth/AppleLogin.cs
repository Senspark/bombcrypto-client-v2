using System;
using System.Text;
using System.Threading.Tasks;

using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;

using UnityEngine;

using Object = UnityEngine.Object;

namespace App {
    public class AppleLogin {
        private readonly AppleLoginMono _mono;
        private TaskCompletionSource<ThirdPartyLoginResult> _task;

        public AppleLogin() {
            var go = new GameObject("Apple Login");
            _mono = go.AddComponent<AppleLoginMono>();
        }

        public void Destroy() {
            Object.Destroy(_mono.gameObject);
        }

        public async Task<ThirdPartyLoginResult> GetAccessToken() {
#if UNITY_EDITOR
            var id = string.Empty;
            string accessToken = AppConfig.AppleEditorAccessToken;
            return new ThirdPartyLoginResult(accessToken, id);
#elif UNITY_IOS
            if (_task == null) {
                _task = new TaskCompletionSource<ThirdPartyLoginResult>();
                _mono.LoginIn(e => _task.SetResult(e), e => _task.SetException(new Exception(e)));
            }
            var r = await _task.Task;
            return r;
#else
            throw new Exception("Not support on your device");
#endif
        }

        private class AppleLoginMono : MonoBehaviour {
            private IAppleAuthManager _appleAuthManager;
            private Action<ThirdPartyLoginResult> _onSuccessCb;
            private Action<string> _onErrorCb;

            private void Update() {
                _appleAuthManager?.Update();
            }

            public void LoginIn(Action<ThirdPartyLoginResult> accessTokenCb, Action<string> errorCb) {
                if (!AppleAuthManager.IsCurrentPlatformSupported) {
                    errorCb("Not support on your device");
                    return;
                }
                
                _onSuccessCb = accessTokenCb;
                _onErrorCb = errorCb;
                
                var loginArgs = new AppleAuthLoginArgs(LoginOptions.None);
                var deserializer = new PayloadDeserializer();
                _appleAuthManager = new AppleAuthManager(deserializer);
                _appleAuthManager.LoginWithAppleId(loginArgs, OnSignInSuccess, OnSignInError);
            }

            private void OnSignInSuccess(ICredential credential) {
                if (credential is not IAppleIDCredential c) {
                    _onErrorCb("Signin not successful");
                    return;
                }

                var userId = c.User;
                var accessToken = Encoding.UTF8.GetString(c.IdentityToken, 0, c.IdentityToken.Length);
                var authCode = Encoding.UTF8.GetString(c.AuthorizationCode, 0, c.AuthorizationCode.Length);
                _onSuccessCb(new ThirdPartyLoginResult(accessToken, userId));
            }

            private void OnSignInError(IAppleError error) {
                _onErrorCb($"{error.GetAuthorizationErrorCode()}");
            }
        }
    }
}