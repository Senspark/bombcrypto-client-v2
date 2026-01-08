using System;
using System.Threading.Tasks;

using Data;

namespace Services {
    public class NullProfileManager : IProfileManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public DateTime? GetLoginTime(string hash) {
            return null;
        }

        public bool TryGetProfile(string hash, out ProfileData profile) {
            profile = null;
            return false;
        }

        public void UpdateLoginTime(string hash, DateTime time) {
        }
    }
}