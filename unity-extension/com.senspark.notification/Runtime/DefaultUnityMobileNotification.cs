using Cysharp.Threading.Tasks;

using JetBrains.Annotations;

using Senspark.Notification;

using UnityEngine;

namespace Senspark.Notifications {
    /// <summary>
    /// https://docs.unity3d.com/Packages/com.unity.mobile.notifications@2.1/manual/index.html
    /// </summary>
    public class DefaultUnityMobileNotification : AnalyticsUnityMobileNotification {
        protected override IAnalyticsManager AnalyticsManager { get; set; }
        protected override IUnityMobileNotification Bridge { get; set; }

        public DefaultUnityMobileNotification(
            [CanBeNull] IAnalyticsManager analyticsManager,
            Color iconColor,
            bool enableLog
        ) {
            AnalyticsManager = analyticsManager;
            var smallIconName = Configs.IconFileName;
#if UNITY_EDITOR
            Bridge = new NullNotification();
#elif UNITY_ANDROID
            Bridge = new AndroidMobileNotification(iconColor, smallIconName, enableLog);
#elif UNITY_IOS
            Bridge = new IosMobileNotification();
#else
            Bridge = new NullNotification();
#endif
        }

        public override async UniTask Initialize() {
            base.Initialize();
            await Bridge.Initialize();
        }

        public override void Dispose() {
            Bridge.Dispose();
            base.Dispose();
        }
    }
}