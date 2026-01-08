using System;

using Cysharp.Threading.Tasks;

namespace Senspark.Platforms {
    using MessageHandler = Action<string>;

    /// <summary>
    /// Dùng để giao tiếp với Native (Android, iOS)
    /// </summary>
    public interface IMessageBridge {
        void RegisterHandler(MessageHandler handler, string tag);
        void UnregisterHandler(string tag);
        string Call(string tag, string message = "");
        UniTask<string> CallAsync(string tag, string message = "");
    }
}

namespace Senspark.Platforms.Internal {
    internal interface IMessageBridgeImpl {
        string Call(string tag, string message);
    }
}