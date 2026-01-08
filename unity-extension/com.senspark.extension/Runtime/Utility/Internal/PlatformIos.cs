#if UNITY_IOS

using System.Runtime.InteropServices;

using JetBrains.Annotations;

using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Unity.Advertisement.IosSupport;

namespace Senspark.Internal {
    public class PlatformIos : IPlatformImpl {
        // ReSharper disable once InconsistentNaming
        [DllImport("__Internal")]
        [NotNull]
        private static extern string getBundleVersion();

        private TaskCompletionSource<TrackingAuthorizationStatus> _attTcs;

        public string GetVersionCode() {
            return getBundleVersion();
        }

        public async UniTask<TrackingAuthorizationStatus> RequestTrackingAuthorization() {
            var request = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            if (request != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED) {
                return (TrackingAuthorizationStatus)request;
            }

            if (_attTcs != null) {
                return await _attTcs.Task;
            }

            _attTcs = new TaskCompletionSource<TrackingAuthorizationStatus>();
            ATTrackingStatusBinding.RequestAuthorizationTracking(r => {
                _attTcs.TrySetResult((TrackingAuthorizationStatus)r);
            });

            return await _attTcs.Task;
        }

        public string GetSha1() {
            return string.Empty;
        }

        public UniTask<string> GetAdvertisingId() {
            return UniTask.FromResult(string.Empty);
        }

        public bool IsApplicationInstalled(string appId) {
            return false;
        }

        public bool OpenApplication(string appId) {
            return false;
        }

        public UniTask<string> FetchSocket(string hostName, int portNumber, string message) {
            return UniTask.FromResult<string>(string.Empty);
        }

        public UniTask<bool> TestConnection(string hostName, float timeOutSeconds) {
            return UniTask.FromResult(true);
        }

        public void AddNativePlugin(string className) {
        }

        public void RemoveNativePlugin(string className) {
        }
    }
}
#endif // UNITY_IOS