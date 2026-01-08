using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

namespace App {
    public class DefaultDailyMissionManager :  ObserverManager<DailyMissionManagerObserver>, IDailyMissionManager {
        private readonly IServerManager _serverManager;
        
        private List<IDailyMission> _missions;
        
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public DefaultDailyMissionManager(IServerManager serverManager) {
            _serverManager = serverManager;
        }

        public async Task SyncData() {
            await _serverManager.General.GetDailyMissions();
            var dailyMissions = await _serverManager.General.GetDailyMissions();
            _missions = dailyMissions.Missions;
            DispatchEvent(e => e.OnDailyMissionChanged?.Invoke());
        }

        public async Task ClaimLuckyTicketDailyMission(string missionCode) {
            await _serverManager.General.ClaimLuckyTicketDailyMission(missionCode);
            await _serverManager.General.GetChestReward();
        }
        
        public List<IDailyMission> GetDailyMission() {
            return _missions;
        }
    }
}