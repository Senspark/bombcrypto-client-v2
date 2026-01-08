#if UNITY_ANDROID

using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Senspark.Platforms;

using UnityEngine;

namespace Senspark.Internal {
    public class PlatformAndroid : IPlatformImpl {
        private const string KPrefix = "Platform_";
        private const string KOpenApplication = KPrefix + "openApplication";
        private const string KIsApplicationInstalled = KPrefix + "isApplicationInstalled";
        private const string KGetDeviceId = KPrefix + "getDeviceId";
        private const string KFetchSocket = KPrefix + "fetchSocket";
        private const string KTestConnection = KPrefix + "testConnection";
        private const string KGetVersionCode = KPrefix + "getVersionCode";
        private const string SensparkActivity = "com.senspark.core.SensparkActivity";

        private readonly AndroidJavaClass _eeJavaClass = new("com.senspark.core.internal.UnityPluginManager");
        

        private readonly IMessageBridge _bridge;
        private string _versionCode = null;

        public PlatformAndroid(IMessageBridge bridge) {
            _bridge = bridge;
        }

        public string GetVersionCode() {
            if (string.IsNullOrEmpty(_versionCode)) {
                _versionCode = _bridge.Call(KGetVersionCode);
            }
            return _versionCode;
        }

        UniTask<TrackingAuthorizationStatus> IPlatformImpl.RequestTrackingAuthorization() {
            return UniTask.FromResult(TrackingAuthorizationStatus.Authorized);
        }

        public string GetSha1() {
            var unityPlayerClass = new AndroidJavaClass(SensparkActivity);
            var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

            var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            var packageName = Application.identifier;
            var packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 64);

            // Get the signature
            var signatures = packageInfo.Get<AndroidJavaObject>("signatures");
            var arrayClass = new AndroidJavaClass("java.lang.reflect.Array");
            var signatureLength = arrayClass.CallStatic<int>("getLength", signatures);
            var signatureArray = new AndroidJavaObject[signatureLength];
            for (var i = 0; i < signatureLength; i++) {
                var s = arrayClass.CallStatic<AndroidJavaObject>("get", signatures, i);
                signatureArray[i] = s;
            }

            var signature = signatureArray[0];

            // Get the SHA-1 hash
            var signatureByteArray = signature.Call<AndroidJavaObject>("toByteArray");
            var messageDigestClass = new AndroidJavaClass("java.security.MessageDigest");
            var sha1Digest = messageDigestClass.CallStatic<AndroidJavaObject>("getInstance", "SHA-1");
            var digestBytes = sha1Digest.Call<sbyte[]>("digest", signatureByteArray);

            // Convert the bytes to a hex string
            var sha1Signature = ToHexString(digestBytes).ToLowerInvariant();
            return sha1Signature;
        }

        async UniTask<string> IPlatformImpl.GetAdvertisingId() {
            var response = await _bridge.CallAsync(KGetDeviceId);
            await UniTask.SwitchToMainThread();
            return response;
        }

        public bool IsApplicationInstalled(string appId) {
            var response = _bridge.Call(KIsApplicationInstalled, appId);
            return ToBool(response);
        }

        public bool OpenApplication(string appId) {
            var response = _bridge.Call(KOpenApplication, appId);
            return ToBool(response);
        }

        public async UniTask<string> FetchSocket(string hostName, int portNumber, string message) {
            var request = new JObject {
                { "host_name", hostName },
                { "port", portNumber },
                { "message", message }
            };
            var result = await _bridge.CallAsync(KFetchSocket, request.ToString());
            await UniTask.SwitchToMainThread();
            return result;
        }

        public async UniTask<bool> TestConnection(string hostName, float timeOutSeconds) {
            var request = new JObject {
                { "host_name", hostName },
                { "time_out", timeOutSeconds },
            };
            var resp = await _bridge.CallAsync(KFetchSocket, request.ToString());
            await UniTask.SwitchToMainThread();
            return ToBool(resp);
        }

        public void AddNativePlugin(string className) {
            _eeJavaClass.CallStatic("addPlugin", className);
        }

        public void RemoveNativePlugin(string className) {
            _eeJavaClass.CallStatic("removePlugin", className);
        }

        private static string ToHexString(sbyte[] byteArray) {
            var hexChars = new char[byteArray.Length * 2];
            for (var i = 0; i < byteArray.Length; ++i) {
                var value = byteArray[i] & 0xFF;
                hexChars[i * 2] = GetHexChar(value >> 4);
                hexChars[i * 2 + 1] = GetHexChar(value & 0x0F);
            }

            return new string(hexChars);
        }

        private static char GetHexChar(int value) {
            if (value < 10) {
                return (char)('0' + value);
            }

            return (char)('A' + (value - 10));
        }

        private static bool ToBool(string value) {
            return value == "true";
        }

        private static string ToString(bool value) {
            return value ? "true" : "false";
        }
    }
}
#endif // UNITY_ANDROID