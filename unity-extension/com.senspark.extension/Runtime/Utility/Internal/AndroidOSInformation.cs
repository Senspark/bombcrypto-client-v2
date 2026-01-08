#if UNITY_ANDROID
using System.Text;

using UnityEngine;

namespace Senspark.Internal {
    /// <summary>
    /// https://developer.android.com/reference/android/os/Build.VERSION.html
    /// </summary>
    public class AndroidVersion {
        private static readonly AndroidJavaClass VersionInfo;

        static AndroidVersion() {
            VersionInfo = new AndroidJavaClass("android.os.Build$VERSION");
        }

        public static string BaseOS =>
            VersionInfo.GetStatic<string>("BASE_OS");

        public static string Codename =>
            VersionInfo.GetStatic<string>("CODENAME");

        public static string Incremental =>
            VersionInfo.GetStatic<string>("INCREMENTAL");

        public static int PreviewSDKInt =>
            VersionInfo.GetStatic<int>("PREVIEW_SDK_INT");

        public static string Release =>
            VersionInfo.GetStatic<string>("RELEASE");

        public static string SDK =>
            VersionInfo.GetStatic<string>("SDK");

        public static int SDKInt =>
            VersionInfo.GetStatic<int>("SDK_INT");

        public static string SecurityPatch =>
            VersionInfo.GetStatic<string>("SECURITY_PATCH");

        public static string AllResult {
            get {
                // Convert to string builder
                var result = new StringBuilder();
                result.Append("BASE_OS: ").Append(BaseOS).AppendLine();
                result.Append("CODENAME: ").Append(Codename).AppendLine();
                result.Append("INCREMENTAL: ").Append(Incremental).AppendLine();
                result.Append("PREVIEW_SDK_INT: ").Append(PreviewSDKInt).AppendLine();
                result.Append("RELEASE: ").Append(Release).AppendLine();
                result.Append("SDK: ").Append(SDK).AppendLine();
                result.Append("SDK_INT: ").Append(SDKInt).AppendLine();
                result.Append("SECURITY_PATCH: ").Append(SecurityPatch).AppendLine();
                return result.ToString();
            }
        }
    }
}
#endif