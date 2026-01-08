using JetBrains.Annotations;

using UnityEngine;

namespace Senspark.Notifications {
    public static class UnityMobileNotificationBuilder {
        public class Builder {
            [CanBeNull]
            public IAnalyticsManager AnalyticsManager { get; set; }

            public Color IconColor { get; set; } = Color.white;

            public bool EnableLog { get; set; }

            public IUnityMobileNotification Build() {
                return new DefaultUnityMobileNotification(AnalyticsManager, IconColor, EnableLog);
            }
        }
    }
}