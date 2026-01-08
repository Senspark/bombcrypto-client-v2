using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Senspark;

namespace Services.RemoteConfig {
    [Service(nameof(IRemoteConfig))]
    public interface IRemoteConfig : IService {
        UniTask SyncData();
        void SetDefaultValues(Dictionary<string, object> defaultValues);
        int GetInt(string key);
        string GetString(string key);
        bool GetBool(string key);
    }
}