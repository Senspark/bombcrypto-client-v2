using System.Collections.Generic;

using JetBrains.Annotations;

using Senspark.Internal;

namespace Senspark {
    public static class FirebaseRemoteConfigManager {
        public class Builder {
            [CanBeNull]
            public Dictionary<string, object> Defaults { get; set; }

            /// <summary>
            /// RealTime = true sẽ cập nhật data theo thời gian thực, ngược lại sẽ cập nhật data khi app khởi động vào lần sau
            /// </summary>
            public bool UseRealTime { get; set; } = false;

            /// <summary>
            /// Set = true sẽ chỉ sử dụng config default của local (Chỉ có tác dụng trên Editor)
            /// </summary>
            public bool EditorUseLocalConfigOnly { get; set; } = true;

            [NotNull]
            public IRemoteConfigManager Build() {
#if UNITY_WEBGL
                return BuildWebGL();
#else
                return BuildMobile();
#endif
            }

#if UNITY_WEBGL
            private IRemoteConfigManager BuildWebGL() {
                var bridge = new LocalRemoteConfigBridge(Defaults ?? new Dictionary<string, object>());
                return new BaseRemoteConfigManager(bridge, bridge);
            }
#endif

#if !UNITY_WEBGL
             private IRemoteConfigManager BuildMobile() {
                var defaults = Defaults ?? new Dictionary<string, object>();
                IRemoteConfigBridge defaultBridge = null;
                IRemoteConfigBridge fallbackBridge = new LocalRemoteConfigBridge(defaults);
#if UNITY_EDITOR
                if (EditorUseLocalConfigOnly) {
                    defaultBridge = fallbackBridge;
                }
#endif
                defaultBridge ??= new FirebaseRemoteConfigBridge(defaults, UseRealTime);
                return new BaseRemoteConfigManager(defaultBridge, fallbackBridge);
            }
#endif
        }
    }
}