using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

namespace App {
    [Service(nameof(IDailyMissionManager))]
    public interface IDailyMissionManager : IService, IObserverManager<DailyMissionManagerObserver>  {
        Task SyncData();
        Task ClaimLuckyTicketDailyMission(string missionCode);
        List<IDailyMission> GetDailyMission();
    }
    
    public class DailyMissionManagerObserver {
        public System.Action OnDailyMissionChanged;
    }

}
