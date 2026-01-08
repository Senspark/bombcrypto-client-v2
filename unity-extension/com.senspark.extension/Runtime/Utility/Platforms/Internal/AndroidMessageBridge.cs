using System;

using UnityEngine;

namespace Senspark.Platforms.Internal {
    using CallCppCallback = Action<string, string>;

    internal class AndroidMessageBridge : IMessageBridgeImpl {
        private const string Package = "com.senspark.core.internal.UnityMessageBridge";
        private readonly AndroidJavaClass _clazz = new(Package);

        public AndroidMessageBridge(CallCppCallback callback) {
            _clazz.CallStatic("initialize", new Callback(callback));
        }

        public string Call(string tag, string message) {
            return _clazz.CallStatic<string>("call", tag, message);
        }

        private class Callback : AndroidJavaProxy {
            private static CallCppCallback _callCppCallback;
            public Callback(CallCppCallback callback) : base($"{Package}$Callback") {
                _callCppCallback = callback;
            }
            
            // ReSharper disable once InconsistentNaming
            private void callCpp(string tag, string message) {
                _callCppCallback(tag, message);
            }
        }
    }
}