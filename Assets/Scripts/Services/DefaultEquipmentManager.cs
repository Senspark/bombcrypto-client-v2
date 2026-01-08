using System;
using System.Threading.Tasks;

namespace Services {
    public class DefaultEquipmentManager : IEquipmentManager {
        private IEquipmentManager.Equipment[] _equipments = Array.Empty<IEquipmentManager.Equipment>();

        public void Destroy() {
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
    }
}