using Senspark.Internal;

namespace Senspark {
    public class UnityLogManager : BaseLogManager {
        public UnityLogManager() : base(new UnityLogBridge()) { }
    }
}