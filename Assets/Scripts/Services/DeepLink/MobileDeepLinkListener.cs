using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Services.DeepLink {
    /// <summary>
    /// https://docs.unity3d.com/Manual/deep-linking-ios.html
    /// https://docs.unity3d.com/Manual/deep-linking-android.html
    /// https://developer.android.com/training/app-links/deep-linking
    /// </summary>
    public class MobileDeepLinkListener : ObserverManager<DeepLinkObserver>, IDeepLinkListener {
        private DeepLinkListener _listener;
        private Dictionary<string, string> _deepLinkData = new();

        public Task<bool> Initialize() {
            var go = new GameObject(nameof(DeepLinkListener));
            _listener = go.AddComponent<DeepLinkListener>();
            _listener.Init(OnReceivedDeepLink);
            Object.DontDestroyOnLoad(go);
            return Task.FromResult(true);
        }

        private void OnReceivedDeepLink(Dictionary<string, string> obj) {
            _deepLinkData = obj;
            DispatchEvent(e => e.OnDeepLinkReceived?.Invoke());
        }

        public void Destroy() {
            if (_listener) {
                Object.Destroy(_listener.gameObject);
                _listener = null;
            }
        }

        public Dictionary<string, string> GetDeepLinkData() {
            return _deepLinkData;
        }

        private class DeepLinkListener : MonoBehaviour {
#if UNITY_EDITOR
            [SerializeField]
            private string testDeepLink = "";
#endif
            private Action<Dictionary<string, string>> _onReceivedDeepLink;

            private void Awake() {
                Application.deepLinkActivated += OnDeepLinkActivated;
                if (!string.IsNullOrEmpty(Application.absoluteURL)) {
                    // Cold start and Application.absoluteURL not null so process Deep Link.
                    OnDeepLinkActivated(Application.absoluteURL);
                }
            }

            public void Init(Action<Dictionary<string, string>> onReceivedDeepLink) {
                _onReceivedDeepLink = onReceivedDeepLink;
            }

            private void OnDeepLinkActivated(string url) {
                try {
                    var from = url.IndexOf('?');
                    if (from < 0) {
                        return;
                    }
                    var queryString = url.Substring(from + 1);
                    var parameterPairs = queryString.Split('&');
                    var dict = new Dictionary<string, string>();

                    foreach (var pair in parameterPairs) {
                        var parts = pair.Split('=');
                        if (parts.Length != 2) {
                            continue;
                        }
                        var k = parts[0];
                        var v = parts[1];
                        dict.Add(k, v);
                    }
                    _onReceivedDeepLink(dict);
                } catch (Exception e) {
                    Debug.LogError(e.Message);
                }
            }

#if UNITY_EDITOR
            [Button]
            private void Test() {
                OnDeepLinkActivated(testDeepLink);
            }
#endif
        }
    }
}