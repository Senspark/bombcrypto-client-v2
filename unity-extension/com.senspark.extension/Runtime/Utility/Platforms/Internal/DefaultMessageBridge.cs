using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

namespace Senspark.Platforms.Internal {
    using MessageHandler = Action<string>;

    internal class DefaultMessageBridge : IMessageBridge {
        private int _asyncCallbackCounter;
        private readonly Dictionary<string, MessageHandler> _handlers;
        private readonly IMessageBridgeImpl _impl;

        public DefaultMessageBridge() {
            _asyncCallbackCounter = 0;
            _handlers = new Dictionary<string, MessageHandler>();
            _impl =
#if UNITY_EDITOR
                new EditorMessageBridge();
#elif UNITY_ANDROID
                new AndroidMessageBridge(CallCpp);
#else
                new EditorMessageBridge();
#endif
        }

        public void RegisterHandler(MessageHandler handler, string tag) {
            if (!_handlers.TryAdd(tag, handler)) {
                throw new ArgumentException($"Failed to register handler: {tag}");
            }
        }

        public void UnregisterHandler(string tag) {
            if (!_handlers.ContainsKey(tag)) {
                throw new ArgumentException($"Failed to deregister handler: {tag}");
            }
            _handlers.Remove(tag);
        }

        public string Call(string tag, string message = "") {
            return _impl.Call(tag, message);
        }

        public UniTask<string> CallAsync(string tag, string message = "") {
            var source = new UniTaskCompletionSource<string>();
            var callbackTag = $"{tag}{_asyncCallbackCounter++}";
            RegisterHandler(callbackMessage => {
                UnregisterHandler(callbackTag);
                UniTask.SwitchToMainThread();
                source.TrySetResult(callbackMessage);
            }, callbackTag);
            var request = $"{callbackTag}@@{message}";
            Call(tag, request);
            return source.Task;
        }

        private void CallCpp(string tag, string message) {
            var handler = FindHandler(tag);
            if (handler == null) {
                throw new ArgumentException($"Failed to call handler: {tag}");
            }
            handler(message);
        }

        private MessageHandler FindHandler(string tag) {
            return _handlers.GetValueOrDefault(tag);
        }
    }
}