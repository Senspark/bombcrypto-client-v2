using System.Threading.Tasks;

using UnityEngine;

namespace Services.WebGL {
    public class EditorBridgeUtils: IWebGLBridgeUtils{
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public void CopyToClipboard(string text) {
            Debug.Log("Copy: " + text);
            GUIUtility.systemCopyBuffer = text;
        }

        public void OpenUrl(string url) {
            Application.OpenURL(url);
        }

        public Task<string> GetStartTelegramParams() {
            return Task.FromResult("abcxyz");
        }

        public Task<bool> IsIOSBrowser() {
            return Task.FromResult(false);
        }

        public Task<bool> IsAndroidBrowser() {
            return Task.FromResult(false);
        }
    }
}