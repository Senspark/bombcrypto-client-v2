using System;
using System.Linq;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using JetBrains.Annotations;

using Senspark.Internal;
using Senspark.Platforms;
using Senspark.Platforms.Internal;
using Senspark.Security;

using UnityEngine;

namespace Senspark {
    public enum TrackingAuthorizationStatus {
        NotDetermined = 0,
        Restricted = 1,
        Denied = 2,
        Authorized = 3,
    }

    public static class Platform {
        // ReSharper disable once InconsistentNaming
        [NotNull]
        private static readonly IPlatformImpl _impl;

        static Platform() {
            MessageBridge = new DefaultMessageBridge();
#if UNITY_EDITOR || UNITY_WEBGL
            _impl = new PlatformEditor();
#elif UNITY_ANDROID
            _impl = new PlatformAndroid(MessageBridge);
#elif UNITY_IOS
            _impl = new PlatformIos();
#endif
        }
        
        public static IMessageBridge MessageBridge { get; private set; }

        [NotNull]
        public static string GetVersionName() {
            return Application.version;
        }

        [NotNull]
        public static string GetVersionCode() {
            return _impl.GetVersionCode();
        }

        public static void SendMail([NotNull] string recipient, [NotNull] string subject, [NotNull] string body) {
            // https://stackoverflow.com/questions/37315362/call-android-email-intent-from-unity-script
            var emailSubject = Uri.EscapeUriString(subject);
            var emailBody = Uri.EscapeUriString(body);
            Application.OpenURL($"mailto:{recipient}?subject={emailSubject}&body={emailBody}");
        }

        public static UniTask<TrackingAuthorizationStatus> RequestTrackingAuthorization() {
            return _impl.RequestTrackingAuthorization();
        }

        public static bool IsValidAppSigningKey(IAppSigningKey appKey) {
#if DEVELOPMENT_BUILD || BUILD_WITH_DEBUG_KEY
            FastLog.Warn($"[Senspark] No need to check App Signing");
            return true;
#else
            FastLog.Warn($"[Senspark] Checking App Signing");

            var sha1 = _impl.GetSha1();
            if (string.IsNullOrEmpty(sha1)) {
                return true;
            }

            var keys = appKey.GetKeys();
            return keys.Contains(sha1);
#endif
        }

        public static UniTask<string> GetAdvertisingId() {
            return _impl.GetAdvertisingId();
        }
        
        public static bool IsApplicationInstalled(string appId) {
            return _impl.IsApplicationInstalled(appId);
        }
        
        public static bool OpenApplication(string appId) {
            return _impl.OpenApplication(appId);
        }
        
        public static UniTask<string> FetchSocket(string hostName, int portNumber, string message) {
            return _impl.FetchSocket(hostName, portNumber, message);
        }
        
        public static UniTask<bool> TestConnection(string hostName, float timeOutSeconds) {
            return _impl.TestConnection(hostName, timeOutSeconds);
        }
        
        public static void AddNativePlugin(string className) {
            _impl.AddNativePlugin(className);
        }
        
        public static void RemoveNativePlugin(string className) {
            _impl.RemoveNativePlugin(className);
        }
    }
}