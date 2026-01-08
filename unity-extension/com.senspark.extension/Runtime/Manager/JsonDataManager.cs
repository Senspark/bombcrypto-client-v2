using System.Threading.Tasks;

using Newtonsoft.Json;

using UnityEngine;

namespace Senspark {
    public class JsonDataManager : IDataManager {
        public Task Initialize() {
            return Task.CompletedTask;
        }

        public T Get<T>(string key, T defaultValue) {
            var str = PlayerPrefs.GetString(key, "");
            return str == "" ? defaultValue : JsonConvert.DeserializeObject<T>(str);
        }

        public void Set<T>(string key, T value) {
            var str = JsonConvert.SerializeObject(value);
            PlayerPrefs.SetString(key, str);
        }
    }
}