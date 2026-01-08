using System.Collections.Generic;

namespace Senspark.Deeplink {
    public class MultiDeepLinkListener : ObserverManager<DeeplinkObserver>, IDeeplinkListener {
        private readonly IDeeplinkListener[] _processors;

        public MultiDeepLinkListener(params IDeeplinkListener[] processor) {
            _processors = processor;
        }

        public void Dispose() {
            foreach (var processor in _processors) {
                processor.Dispose();
            }
        }

        public List<DeeplinkInfo> GetLastData() {
            var result = new List<DeeplinkInfo>();
            foreach (var processor in _processors) {
                result.AddRange(processor.GetLastData());
            }
            return result;
        }
    }
}