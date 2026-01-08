using System;
using System.Threading.Tasks;

using App;

using Castle.Core.Internal;

using Senspark;

using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using UnityEngine;

using Utils;

namespace Services.WebGL {
    public class WebGLBridgeUtils : IWebGLBridgeUtils {
        
        private readonly ILogManager _logManager;
        private readonly IMasterUnityCommunication _unityCommunication;

        public WebGLBridgeUtils(ILogManager logManager, IMasterUnityCommunication unityCommunication) {
            _logManager = logManager;
            _unityCommunication = unityCommunication;
        }
        
        public void CopyToClipboard(string text) {
            try {
                if(text.IsNullOrEmpty())
                    return;
                _logManager.Log($"Copy: {text}");
                _unityCommunication.UnityToReact.SendToReact(ReactCommand.COPY_TO_CLIP_BOARD, text);
            } catch (Exception e) {
                _logManager.Log($"CLIENT: Error when copy {text}: {e.Message}");
                GUIUtility.systemCopyBuffer = text;
            }

        }

        public void OpenUrl(string url) {
            try {
                if(url.IsNullOrEmpty())
                    return;
                _logManager.Log($"Open: {url}");
                _unityCommunication.UnityToReact.SendToReact(ReactCommand.OPEN_URL, url);
            } catch (Exception e) {
                _logManager.Log($"CLIENT: Error when open {url}: {e.Message}");
                Application.OpenURL(url);
            }
  
        }

        public async Task<string> GetStartTelegramParams() {
            try {
                _logManager.Log();

                var result = await _unityCommunication.UnityToReact.SendToReact(ReactCommand.GET_START_PARAM);
                _logManager.Log($"GetStartParams result: {result}");
                return result ?? "";
            } catch (Exception e) {
                _logManager.Log($"CLIENT: Error when get start params: {e.Message}");
                return "";
            }
        }

        public async Task<bool> IsIOSBrowser() {
            try {
                _logManager.Log();
                //Chỉ ton mói cần check xem có phải ios ko
                if (!AppConfig.IsTon())
                    return false;
                
                var result = await _unityCommunication.UnityToReact.SendToReact(ReactCommand.IS_IOS_BROWSER);

                _logManager.Log($"Is ios browser: {result}");
                return bool.Parse(result);
            } catch (Exception e) {
                _logManager.Log($"CLIENT: Error when check platform: {e.Message}");
                return false;
            }
        }

        public async Task<bool> IsAndroidBrowser() {
            try {
                _logManager.Log();
                //Chỉ ton mói cần check xem có phải android ko
                if (!AppConfig.IsTon())
                    return false;
                
                var result = await _unityCommunication.UnityToReact.SendToReact(ReactCommand.IS_ANDROID_BROWSER);
                _logManager.Log($"Is ios browser: {result}");
                return result != null && bool.Parse(result);
            } catch (Exception e) {
                _logManager.Log($"CLIENT: Error when check platform: {e.Message}");
                return false;
            }
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}