using System.Collections.Generic;

using JetBrains.Annotations;

using Senspark.Internal;

namespace Senspark {
    public static class LocalRemoteConfigManager {
        public class Builder {
            [CanBeNull]
            public Dictionary<string, object> Defaults { get; set; }

            [NotNull]
            public IRemoteConfigManager Build() {
                var bridge = new LocalRemoteConfigBridge(Defaults ?? new Dictionary<string, object>());
                return new BaseRemoteConfigManager(bridge, bridge);
            }
        }
    }
}