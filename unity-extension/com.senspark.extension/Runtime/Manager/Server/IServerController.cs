using Cysharp.Threading.Tasks;

using Senspark.Server;

namespace Senspark {
    [Service(nameof(IServerController))]
    public interface IServerController {
        UniTask<bool> Initialize();
        void SetUserId(string userId);
        void SetUserName(string userName);
        UniTask<ReadResult> ReadData(string tableName);
        UniTask<WriteResult> WriteData(IDataTable data);
    }
}