using System;

using Cysharp.Threading.Tasks;

namespace Communicate {
    public interface IJavascriptProcessor {
        UniTask<string> CallReact(string methodTag, string data);
        void RegisterUnityAction(Action<ReactMessage> action);
    }
}