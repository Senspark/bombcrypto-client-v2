using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services {
    using Data = IDictionary<int, string>;

    public class SkinChestNameManager : ISkinChestNameManager {
        private readonly Data _data;

        public SkinChestNameManager(Data data) {
            _data = data;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public string GetName(int id) {
            if (!_data.ContainsKey(id)) {
                throw new Exception($"Could not find id: {id}");
            }
            return _data[id];
        }
    }
}