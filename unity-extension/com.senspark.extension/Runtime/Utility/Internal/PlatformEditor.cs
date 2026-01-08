#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Senspark.Internal {
    public class PlatformEditor : IPlatformImpl {
        public string GetVersionCode() {
            return Application.platform switch {
#if UNITY_EDITOR
                RuntimePlatform.Android => PlayerSettings.Android.bundleVersionCode.ToString(),
                RuntimePlatform.IPhonePlayer => PlayerSettings.iOS.buildNumber,
#endif // UNITY_EDITOR
                _ => "-1",
            };
        }

        UniTask<TrackingAuthorizationStatus> IPlatformImpl.RequestTrackingAuthorization() {
            return UniTask.FromResult(TrackingAuthorizationStatus.Authorized);
        }

        public string GetSha1() {
            return string.Empty;
        }

        UniTask<string> IPlatformImpl.GetAdvertisingId() {
            return UniTask.FromResult(string.Empty);
        }

        public bool IsApplicationInstalled(string appId) {
            return false;
        }

        public bool OpenApplication(string appId) {
            return false;
        }

        public UniTask<string> FetchSocket(string hostName, int portNumber, string message) {
            return UniTask.FromResult(string.Empty);
        }

        public UniTask<bool> TestConnection(string hostName, float timeOutSeconds) {
            return UniTask.FromResult(true);
        }

        public Task<string> GetAdvertisingId() {
            return Task.FromResult(string.Empty);
        }

        public void AddNativePlugin(string className) {
        }

        public void RemoveNativePlugin(string className) {
        }
    }
}