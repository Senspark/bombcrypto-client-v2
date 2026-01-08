using System.Collections.Generic;

using Senspark;

namespace App {
    [Service(nameof(IHouseStorageManager))]
    public interface IHouseStorageManager : IService {
        void LoadHouseFromServer(IHouseDetails[] houseArray);
        void LoadLockedHousesFromServer(IHouseDetails[] houseArray);
        void LoadHeroInHouseFromServer(Dictionary<int, List<int>> heroInHouse);
        void HeroRestInHouse(int heroId);
        void HeroLeaveHouse(int heroId);
        List<int> GetHeroRestInHouse(int houseId);
        int GetHouseCount();
        int GetLockedHouseCount();
        HouseData GetHouseData(int index);
        HouseData GetLockedHouseData(int index);
        HouseData GetHouseDataFromId(int id);
        float GetHouseChargeFromId(int id);
        List<int> CheckHouseRentExpired();
        int GetIndexFromId(int id);
        HouseData GetActiveHouseData();
        int GetActiveIndex();
        void SetActiveHouse(string genID);
        HouseData GetNextHouseShow(int index, HouseData currentHouse);
        int GetQuantityHouseShow();
        void UpdateHouse(HouseData detail);
        void UpdateLockedHouse(HouseData houseData);
        void UpdateHouseRentTime(int houseId, long endTimeRent);
        int GetHouseSlot();
    }
}