using System;

using Cysharp.Threading.Tasks;

using Senspark;

namespace Services {
    [Service(nameof(IFinanceUserLoader))]
    public interface IFinanceUserLoader : IService {
        UniTask LoadAsync(Action<int, string> updateProgress = null);
    }
}