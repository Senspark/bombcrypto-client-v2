using System;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IProfileManager))]
    public interface IProfileManager : IService {
        DateTime? GetLoginTime(string hash);
        bool TryGetProfile(string hash, out ProfileData profile);
        void UpdateLoginTime(string hash, DateTime time);
    }
}