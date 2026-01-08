using JetBrains.Annotations;

namespace Senspark {
    public static class FirebaseLogManager {
        public class Builder {
            [NotNull]
            public ILogManager Build() {
#if !UNITY_WEBGL
                var bridge = new Internal.FirebaseLogBridge();
                return new Internal.BaseLogManager(bridge);
#else
                return new UnityLogManager();
#endif
            }
        }
    }
}