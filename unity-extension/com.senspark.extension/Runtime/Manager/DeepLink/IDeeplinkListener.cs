using System;
using System.Collections.Generic;

namespace Senspark.Deeplink {
    public class DeeplinkObserver {
        public Action<DeeplinkInfo> OnDeeplinkActivated;
    }

    public class DeeplinkInfo {
        public string Scheme = string.Empty;
        public string Host = string.Empty;
        public readonly Dictionary<string, string> Params = new();
    }
    
    public interface IDeeplinkListener : IObserverManager<DeeplinkObserver>, IDisposable {
        List<DeeplinkInfo> GetLastData();
    }
}