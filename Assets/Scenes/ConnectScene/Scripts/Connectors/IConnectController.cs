using System.Threading.Tasks;

using App;

namespace Scenes.ConnectScene.Scripts.Connectors {
    public interface IConnectController {
        Task<UserAccount> StartFlow();
    }
}