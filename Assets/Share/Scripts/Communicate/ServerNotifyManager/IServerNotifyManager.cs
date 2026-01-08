using BLPvpMode.Manager.Api;
using Cysharp.Threading.Tasks;
using Senspark;

namespace Share.Scripts.Communicate {
    [Service(nameof(IUserSolanaManager))]
    public interface IServerNotifyManager :  IService, IServerListener, IObserverManager<ServerNotifyObserver> {
        UniTask<T> WaitForEvent<T>(ServerNotifyEvent eventType);
    }
}