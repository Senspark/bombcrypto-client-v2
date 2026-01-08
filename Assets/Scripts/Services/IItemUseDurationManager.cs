using System;

using Senspark;

namespace Services {
    [Service(nameof(IItemUseDurationManager))]
    public interface IItemUseDurationManager : IService {
        TimeSpan GetDuration();
        void Initialize(TimeSpan duration);
    }
}