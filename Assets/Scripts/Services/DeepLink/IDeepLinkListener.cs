using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

namespace Services.DeepLink {
    [Service(nameof(IDeepLinkListener))]
    public interface IDeepLinkListener : IService, IObserverManager<DeepLinkObserver> {
        Dictionary<string, string> GetDeepLinkData();
    }

    public class DeepLinkObserver {
        public Action OnDeepLinkReceived;
    }

    public static class DeepLinkKey {
        public const string Token = "token";
    }
}