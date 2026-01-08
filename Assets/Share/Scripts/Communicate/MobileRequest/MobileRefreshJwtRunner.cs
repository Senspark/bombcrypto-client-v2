using System;
using System.Collections;

using App;

using Castle.Core.Internal;

using Communicate;

using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.Dialog;

using UnityEngine;

namespace Share.Scripts.Communicate {
    public class MobileRefreshJwtRunner : MonoBehaviour {
        private IMobileRequest _mobileRequest;
        private IJwtSession _jwtSession;
        private ILogManager _logManager;
        private bool _isRunning = false;

        private const int SecondsToRefreshProd = 60 * 1000 * 5; // 5 minutes
        private const int SecondsToRefreshTest = 60 * 1000 * 1; // 1 minutes

        private readonly int _secondsToRefresh = AppConfig.IsProduction ? SecondsToRefreshProd : SecondsToRefreshTest;

        public void Setup(IMobileRequest mobileRequest, IJwtSession jwtSession, ILogManager logManager) {
            _logManager = logManager;
            _jwtSession = jwtSession;
            _mobileRequest = mobileRequest;
            DontDestroyOnLoad(gameObject);
        }

        public void Run() {
            if (_mobileRequest == null) {
                _logManager?.Log("MobileRequest is null");
                return;
            }
            if (_jwtSession == null) {
                _logManager?.Log("JwtSession is null");
                return;
            }

            AutoRefreshJwt().Forget();
        }

        // Not used yet
        public void Stop() {
            _isRunning = false;
        }

        private async UniTask AutoRefreshJwt() {
            _logManager?.Log("Auto refresh jwt is running");
            _isRunning = true;
            while (_isRunning) {
                try {
                    await UniTask.Delay(_secondsToRefresh);
                    if (!_isRunning)
                        break;

                    if (_jwtSession.Account == null) {
                        _logManager?.Log("Account is null");
                        continue;
                    }

                    if (_jwtSession.Account.jwtToken.IsNullOrEmpty()) {
                        _logManager?.Log("Account may not be logged in");
                        continue;
                    }
                    
                    var isAccount = _jwtSession.Account.loginType == LoginType.UsernamePassword;
                    _logManager?.Log($"Start refresh jwt for {(isAccount ? "Account" : "Guest")}");
                    if (isAccount) {
                        var jwtData = await _mobileRequest.RefreshJwtAccount(_jwtSession.Account.userName,
                            _jwtSession.Account.password);
                        _jwtSession.SetRawJwt(jwtData.Jwt);
                    } else {
                        var jwtData = await _mobileRequest.RefreshJwtGuest(_jwtSession.Account.userName);
                        _jwtSession.SetRawJwt(jwtData.Jwt);
                    }
                } catch (Exception e) {
                    _logManager?.Log($"Auto refresh jwt error: {e.Message}");
                    _logManager?.Log($"Try again");
                }
            }
        }
    }
}