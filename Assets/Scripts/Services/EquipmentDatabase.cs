using System.Threading.Tasks;

using Data;

namespace Services {
    public interface IEquipmentDatabase {
        Task<EquipmentData[]> QueryEquipmentAsync();
    }
}