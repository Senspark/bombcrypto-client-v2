using System;
using System.Threading.Tasks;

namespace Messaging {
    public interface IMessagingManager {
        Task<bool> Initialize();
        void SetOnTokenReceivedCallback(Action<string> callback);
    }
}