using System.Threading.Tasks;

using App;

using BLPvpMode.Manager.Api;

using Game.Dialog;

using Senspark;

[Service(nameof(IUserTonManager))]
public interface IUserTonManager : IService, IServerListener ,IObserverManager<UserTonObserver> {
    Task<string> GetInvoice(double amount, DepositType depositType);
    Task<bool> GetTaskTonDataConfig();
    Task<bool> CompleteTask(int taskId);
    Task<bool> ClaimTask(int taskId);
    Task<App.IReferralData> GetReferralData();
    Task<bool> ClaimReferralReward();
    Task<bool> ReactiveHouse(int houseId);
}
