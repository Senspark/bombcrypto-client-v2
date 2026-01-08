using Senspark;

namespace App {
    [Service(nameof(IAccountManager))]
    public interface IAccountManager : IService {
        string Account { get; }
    }
}