using System.Collections.Generic;
using System.Threading.Tasks;

using App;

namespace Scenes.TreasureModeScene.Scripts.Mocks {
    public class NullHouseStorageManager : IHouseStorageManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public void LoadHouseFromServer(IHouseDetails[] houseArray) {
        }
        
        public void LoadLockedHousesFromServer(IHouseDetails[] houseArray) {
        }

        public void LoadHeroInHouseFromServer(Dictionary<int, List<int>> heroInHouse) {
        }

        public void HeroRestInHouse(int heroId) {
        }

        public void HeroLeaveHouse(int heroId) {
        }

        public List<int> GetHeroRestInHouse(int houseId) {
            return new List<int>();
        }

        public int GetHouseCount() {
            return 0;
        }
        
        public int GetLockedHouseCount() {
            return 0;
        }

        public HouseData GetHouseData(int index) {
            return GetActiveHouseData();
        }
        
        public HouseData GetLockedHouseData(int index) {
            return GetActiveHouseData();
        }

        public HouseData GetHouseDataFromId(int id) {
            return GetActiveHouseData();
        }

        public float GetHouseChargeFromId(int id) {
            return 0f;
        }

        public List<int> CheckHouseRentExpired() {
            return new List<int>();
        }

        public int GetIndexFromId(int id) {
            return 0;
        }

        public HouseData GetActiveHouseData() {
            return new HouseData(HouseType.Villa, HouseSize.s6x6, 0, 0, 0, 0, 0, 0);
        }

        public int GetActiveIndex() {
            return 0;
        }

        public void SetActiveHouse(string genID) {
        }

        public HouseData GetNextHouseShow(int index, HouseData currentHouse) {
            return currentHouse;
        }

        public int GetQuantityHouseShow() {
            return 0;
        }

        public void UpdateHouse(HouseData detail) {
        }

        public void UpdateLockedHouse(HouseData houseData) {
            
        }

        public void UpdateHouseRentTime(int houseId, long endTimeRent) {
        }

        public int GetHouseSlot() {
            return 0;
        }
    }
}