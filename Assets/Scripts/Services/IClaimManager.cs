using System.Threading.Tasks;

using Senspark;

namespace App {
    [Service(nameof(IClaimManager))]
    public interface IClaimManager : IService {
        Task<int> ClaimHero();
    }
}