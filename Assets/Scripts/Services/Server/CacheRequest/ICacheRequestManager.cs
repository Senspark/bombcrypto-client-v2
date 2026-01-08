using System.Threading.Tasks;

using BLPvpMode.Manager.Api;

using Sfs2X.Entities.Data;

namespace App {
    public interface ICacheRequestManager {
        Task<(long, string)> GetWebResponse(string cmd, string path);
        Task<T> ProcessApi<T>(string cmd, ISFSObject data, IServerHandler<T> serverHandler);
        Task<ISFSObject> ProcessApi(string cmd, ISFSObject data, IServerHandler<ISFSObject> serverHandler);
        void ClearCacheForNewUser();
        void ClearCache(string requestKey);
    }
}