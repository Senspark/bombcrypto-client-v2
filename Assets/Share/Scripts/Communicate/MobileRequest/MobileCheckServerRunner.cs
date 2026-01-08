using System;

using App;

using Communicate;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;

namespace Share.Scripts.Communicate {
    /// <summary>
    /// Tuự động kt server có maintenance hay không
    /// </summary>
    public class MobileCheckServerRunner: MonoBehaviour {
      private IMobileRequest _mobileRequest;
        private ILogManager _logManager;
        private bool _isRunning = false;

        private const int SecondsToCheckProd = 60 * 1000 * 5; // 5 minutes
        private const int SecondsToCheckTest = 60 * 1000 * 1; // 1 minutes

        private readonly int _secondsToCheck = AppConfig.IsProduction ? SecondsToCheckProd : SecondsToCheckTest;

        public void Setup(IMobileRequest mobileRequest, ILogManager logManager) {
            _logManager = logManager;
            _mobileRequest = mobileRequest;
            
            DontDestroyOnLoad(gameObject);
        }

        public void Run() {
            if (_mobileRequest == null) {
                _logManager?.Log("MobileRequest is null");
                return;
            }
            AutoCheckServer().Forget();
        }

        // Not used yet
        public void Stop() {
            _isRunning = false;
        }

        private async UniTask AutoCheckServer() {
            _logManager?.Log("Auto check server maintenance is running");
            _isRunning = true;
            while (_isRunning) {
                try {
                    await UniTask.Delay(_secondsToCheck);
                    if (!_isRunning)
                        break;

                    var result = await _mobileRequest.CheckServer();
                    _logManager?.Log($"Server is maintenance =: {result}");
                    //true => server đang maintenance
                    if (result) {
                        App.Utils.KickToConnectScene();
                        break;
                    }
                } catch (Exception e) {
                    _logManager?.Log($"Auto check server fail: {e.Message}");
                }
            }
        }
    }
}