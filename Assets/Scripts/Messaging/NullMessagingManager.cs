using System;
using System.Threading.Tasks;

namespace Messaging {
    public class NullMessagingManager : IMessagingManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void SetOnTokenReceivedCallback(Action<string> callback) {
        }

        public void Start() {
        }
    }
}