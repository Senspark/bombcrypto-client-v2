using System;
using System.Threading.Tasks;

using Senspark;

namespace Services {
    public class ItemUseDurationManager : IItemUseDurationManager {
        private TimeSpan _duration;
        private readonly ILogManager _logManager;

        public ItemUseDurationManager(ILogManager logManager) {
            _logManager = logManager;
        }

        public void Destroy() {
        }

        public TimeSpan GetDuration() {
            return _duration;
        }

        public void Initialize(TimeSpan duration) {
            _logManager.Log($"duration: {duration.TotalDays}:{duration.Hours}:{duration.Minutes}");
            _duration = duration;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
    }
}