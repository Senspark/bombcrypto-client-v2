using JetBrains.Annotations;

using Senspark.Iap.CheatDetection;

namespace Senspark.Iap {
    public static class UnityPurchaseManagerBuilder {
        public class Builder {
            [NotNull]
            public IAnalyticsManager Analytics { get; set; }

            [NotNull]
            public IapData[] IapData { get; set; }

            [CanBeNull]
            public ICheatPunishment CheatPunishment { get; set; }

#if !UNITY_WEBGL
            [CanBeNull]
            public byte[] GoogleTangle { get; set; }
#endif

            public bool ServerVerify { get; set; } = true;

            public float EditorSubscriptionExpiredSeconds { get; set; } = 5 * 60; // 5 Minutes

            public IUnityPurchaseManager Build() {
#if !UNITY_WEBGL
                UnityEngine.Assertions.Assert.IsNotNull(Analytics, $"{nameof(Analytics)} must not be null");
                UnityEngine.Assertions.Assert.IsNotNull(IapData, $"{nameof(IapData)} must not be null");
                return new DefaultUnityPurchaseManager(Analytics, IapData, ServerVerify, GoogleTangle, CheatPunishment,
                    EditorSubscriptionExpiredSeconds);
#else
                return new NullPurchaseManager();
#endif
            }
        }
    }
}