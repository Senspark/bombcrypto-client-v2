using System;
using System.Collections.Generic;

using UnityEngine;

namespace Senspark.Deeplink {
    public class UnityDeeplinkListener : ObserverManager<DeeplinkObserver>, IDeeplinkListener {
        private readonly List<DeeplinkInfo> _deepLinkInfos = new();

        public UnityDeeplinkListener() {
            Application.deepLinkActivated += OnDeepLinkActivated;

            if (!string.IsNullOrEmpty(Application.absoluteURL)) {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                OnDeepLinkActivated(Application.absoluteURL);
            }
        }

        public void Dispose() {
            Application.deepLinkActivated -= OnDeepLinkActivated;
        }

        private void OnDeepLinkActivated(string url) {
            // Format1: https://example.com/?param1=1&param2=2
            // Format2: scheme://host?param1=1&param2=2

            var info = new DeeplinkInfo();

            var uri = new Uri(url);
            info.Scheme = uri.Scheme;
            info.Host = uri.Host;
            var query = uri.Query;
            var parameters = query.Split('&');
            foreach (var parameter in parameters) {
                var keyValue = parameter.Split('=');
                var key = keyValue[0];
                var value = keyValue[1];
                info.Params.Add(key, value);
            }
            
            _deepLinkInfos.Add(info);
            DispatchEvent(e => e?.OnDeeplinkActivated?.Invoke(info));
        }

        public List<DeeplinkInfo> GetLastData() {
            return _deepLinkInfos;
        }
    }
}