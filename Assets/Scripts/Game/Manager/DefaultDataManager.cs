using System.Threading.Tasks;

namespace App {
    public class DefaultDataManager : IDataManager {
        private readonly IDataStorage _storage;

        public DefaultDataManager(IDataStorage storage) {
            _storage = storage;
        }

        public int GetInt(string key, int defaultValue) {
            return _storage.GetInt(key, defaultValue);
        }

        public float GetFloat(string key, float defaultValue) {
            return _storage.GetFloat(key, defaultValue);
        }

        public string GetString(string key, string defaultValue) {
            return _storage.GetString(key, defaultValue);
        }

        public void SetInt(string key, int value) {
            _storage.SetInt(key, value);
        }

        public void SetFloat(string key, float value) {
            _storage.SetFloat(key, value);
        }

        public void SetString(string key, string value) {
            _storage.SetString(key, value);
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}