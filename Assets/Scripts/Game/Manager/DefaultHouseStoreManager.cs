using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Analytics;

using CodeStage.AntiCheat.ObscuredTypes;

using Engine.Utils;

using Senspark;

namespace App {
    public enum HouseType
    {
        TinyHouse,
        MiniHouse,
        LuxHouse,
        PenHouse,
        Villa,
        SuperVilla
    }

    public enum HouseSize
    {
        s6x6,
        s6x10,
        s6x15,
        s6x20,
        s6x25,
        s6x30
    }
    
    public class HouseData
    {
        public ObscuredString genID;
        public ObscuredInt id;
        public ObscuredBool isActive;
        public HouseType HouseType;
        public HouseSize Size;
        public ObscuredFloat Charge;
        public ObscuredInt Slot;
        public ObscuredDouble Price;
        public ObscuredInt Supply;
        public ObscuredInt MintLimits;
        public ObscuredDouble PriceTokenNetwork;
        public ObscuredLong EndTimeRent;

        public HouseData(HouseType type, HouseSize size, float charge, int slot, double price, int supply, int mintLimits, double priceTokenNetwork)
        {
            isActive = false;
            HouseType = type;
            Size = size;
            Charge = charge;
            Slot = slot;
            Price = price;
            Supply = supply;
            MintLimits = mintLimits;
            PriceTokenNetwork = priceTokenNetwork;
        }
    }
    
    public class DefaultHouseStoreManager : IHouseStorageManager {
        private readonly IDataManager _dataManager;
        private readonly IAnalytics _analytics;
        private HouseData[] _houseDatas;
        private HouseData[] _lockedHouseDatas;
        private Dictionary<int, List<int>> _heroInHouse;
        private int _activeIndex = -1;

        public DefaultHouseStoreManager(IDataManager dataManager, IAnalytics analytics) {
            _dataManager = dataManager;
            _analytics = analytics;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public void LoadHouseFromServer(IHouseDetails[] details) {
            _houseDatas = new HouseData[details.Length];
            for (var i = 0; i < details.Length; i++) {
                var entry = details[i];
                var houseData = GenerateHouseData(entry.Rarity);
                houseData.genID = entry.Details;
                houseData.id = entry.Id;
                houseData.isActive = entry.IsActive;
                houseData.Charge = entry.Recovery;
                houseData.Slot = entry.Capacity;
                _houseDatas[i] = houseData;
                houseData.EndTimeRent = entry.EndTimeRent;

                if (houseData.isActive) {
                    _activeIndex = i;
                }
            }
        }
        
        public void LoadLockedHousesFromServer(IHouseDetails[] details) {
            _lockedHouseDatas = new HouseData[details.Length];
            for (var i = 0; i < details.Length; i++) {
                var entry = details[i];
                var houseData = GenerateHouseData(entry.Rarity);
                houseData.genID = entry.Details;
                houseData.id = entry.Id;
                houseData.isActive = entry.IsActive;
                houseData.Charge = entry.Recovery;
                houseData.Slot = entry.Capacity;
                _lockedHouseDatas[i] = houseData;
            }
        }

        public void LoadHeroInHouseFromServer(Dictionary<int, List<int>> heroInHouse) {
            _heroInHouse = heroInHouse;
        }

        public void HeroRestInHouse(int heroId) {
            if (_houseDatas == null || _heroInHouse == null) {
                return;
            }
            HeroLeaveHouse(heroId);
            HouseData house = null;
            foreach (var houseData in _houseDatas) {
                if (!houseData.isActive && houseData.EndTimeRent < DateTime.UtcNow.ToEpochMilliseconds())
                {
                    continue;
                }
                if (GetHeroRestInHouse(houseData.id).Count >= houseData.Slot) {
                    continue;
                }
                if (house == null || (houseData.Charge > house.Charge)) {
                    house = houseData;
                }
            }
            if (house != null) {
                if (!_heroInHouse.ContainsKey(house.id)) {
                    _heroInHouse[house.id] = new List<int>();
                }
                _heroInHouse[house.id].Add(heroId);
            }
        }

        public void HeroLeaveHouse(int heroId) {
            if (_heroInHouse == null) {
                return;
            }
            foreach (var house in _heroInHouse)
            {
                if (house.Value.Remove(heroId))
                {
                    break;
                }
            }
        }

        public List<int> GetHeroRestInHouse(int houseId) {
            if (!_heroInHouse.ContainsKey(houseId)) {
                return new List<int>();
            }
            return _heroInHouse[houseId];
        }

        public void UpdateHouse(HouseData detail) {
            var houses = _houseDatas.ToList();
            houses.Add(detail);
            _houseDatas = houses.ToArray();
        }

        public void UpdateHouseRentTime(int houseId, long endTimeRent) {
            var houses = GetHouseDataFromId(houseId);
            houses.EndTimeRent = endTimeRent;
        }

        public int GetHouseCount() {
            return _houseDatas?.Length ?? 0;
        }
        
        public int GetLockedHouseCount() {
            return _lockedHouseDatas?.Length ?? 0;
        }

        public HouseData GetHouseData(int index) {
            if (index < 0 || index >= _houseDatas.Length) {
                return null;
            }

            return _houseDatas?[index];
        }
        
        public HouseData GetLockedHouseData(int index) {
            if (index < 0 || index >= _lockedHouseDatas.Length) {
                return null;
            }

            return _lockedHouseDatas?[index];
        }

        public void UpdateLockedHouse(HouseData houseData) {

            if (_lockedHouseDatas == null) {
                return;
            }
            var tempList = _lockedHouseDatas.ToList();
            tempList.Remove(houseData);
            _lockedHouseDatas = tempList.ToArray();
        }

        public HouseData GetHouseDataFromId(int id) {
            return _houseDatas?.FirstOrDefault(data => data != null && data.id == id);
        }

        public float GetHouseChargeFromId(int id) {
            foreach (var entry in _heroInHouse) {
                if (entry.Value.Contains(id)) {
                    var houseRent = _houseDatas.FirstOrDefault(it => it.id == entry.Key);
                    if (houseRent != null) {
                        return houseRent.Charge;
                    }
                }
            }
            if (GetActiveHouseData() == null) {
                return 0;
            }
            return GetActiveHouseData().Charge;
        }

        public List<int> CheckHouseRentExpired() {
            var result = new List<int>();
            foreach (var houseData in _houseDatas) {
                if (houseData.EndTimeRent < DateTime.UtcNow.ToEpochMilliseconds() && !houseData.isActive) {
                    if (_heroInHouse.ContainsKey(houseData.id)) {
                        result.AddRange(_heroInHouse[houseData.id]);
                    }
                }
            }
            return result;
        }

        public int GetIndexFromId(int id) {
            if (_houseDatas != null) {
                for (var i = 0; i < _houseDatas.Length; i++) {
                    var data = _houseDatas[i];
                    if (data != null && data.id == id) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public HouseData GetActiveHouseData() {
            return _activeIndex >= 0 ? _houseDatas[_activeIndex] : null;
        }

        public int GetActiveIndex() {
            return _activeIndex;
        }

        public void SetActiveHouse(string genID) {
            var activeHouse = GetActiveHouseData();
            for (var i = 0; i < _houseDatas.Length; i++) {
                var house = _houseDatas[i];
                if (house.genID == genID) {
                    // nếu thay đổi house active thì đổi hero từ house cũ sang house mới
                    if (activeHouse != null) {
                        if (_heroInHouse.ContainsKey(activeHouse.id)) {
                            _heroInHouse[house.id] = _heroInHouse[activeHouse.id];
                            _heroInHouse.Remove(activeHouse.id);
                        }
                    }
                    _activeIndex = i;
                    house.isActive = true;
                } else {
                    house.isActive = false;
                }
            }
        }

        // index để xác định qua trái qua phải, -1 là qua trái, 1 là qua phải
        public HouseData GetNextHouseShow(int index, HouseData currentHouse) {
            var nextIndex = GetIndexFromId(currentHouse.id);
            do {
                nextIndex += index;
                if (nextIndex < 0) {
                    nextIndex = _houseDatas.Length - 1;
                }
                if (nextIndex >= _houseDatas.Length) {
                    nextIndex = 0;
                }
                var houseData = _houseDatas[nextIndex];
                if (houseData.isActive || houseData.EndTimeRent > DateTime.UtcNow.ToEpochMilliseconds()) {
                    return houseData;
                }
            } while (_houseDatas[nextIndex].id != currentHouse.id);
            return currentHouse;
        }

        public int GetQuantityHouseShow() {
            return _houseDatas.Count(h => h.isActive || h.EndTimeRent > DateTime.UtcNow.ToEpochMilliseconds());
        }

        public int GetHouseSlot() {
            var activeHouse = GetActiveHouseData();
            var activeHouseSlot = 0;
            if (activeHouse != null) {
                activeHouseSlot = activeHouse.Slot;
            }
            var slotInHouseRent = _houseDatas.Where(h => h.EndTimeRent > DateTime.UtcNow.ToEpochMilliseconds())
                .Sum(h => h.Slot);
            var totalSlot = activeHouseSlot + slotInHouseRent;
            if (totalSlot > 15) {
                totalSlot = 15;
            }
            return totalSlot;
        }
        
        private static HouseData GenerateHouseData(int index) {
            return GetHouseInfo(index);
        }

        public static HouseData GetHouseInfo(int index) {
            var storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            var housePrice = storeManager.HousePrice;
            var housePriceTokenNetwork = storeManager.HousePriceTokenNetwork;
            var minAvailable = storeManager.HouseMinAvailable;
            var charges = storeManager.Charge;
            var slots = storeManager.Slot;
            var mintLimits = storeManager.HouseMintLimits;

            return index switch {
                0 => new HouseData(HouseType.TinyHouse, HouseSize.s6x6, charges[0], slots[0], housePrice[0],
                    minAvailable[0], mintLimits[0], housePriceTokenNetwork != null ? housePriceTokenNetwork[0] : 0),
                1 =>
                new HouseData(HouseType.MiniHouse, HouseSize.s6x10, charges[1], slots[1], housePrice[1],
                    minAvailable[1], mintLimits[1], housePriceTokenNetwork != null ? housePriceTokenNetwork[1] : 0),
                2 =>
                new HouseData(HouseType.LuxHouse, HouseSize.s6x15, charges[2], slots[2], housePrice[2],
                    minAvailable[2], mintLimits[2], housePriceTokenNetwork != null ? housePriceTokenNetwork[2] : 0),
                3 =>
                new HouseData(HouseType.PenHouse, HouseSize.s6x20, charges[3], slots[3], housePrice[3],
                    minAvailable[3], mintLimits[3], housePriceTokenNetwork != null ? housePriceTokenNetwork[3] : 0),
                4 =>
                new HouseData(HouseType.Villa, HouseSize.s6x25, charges[4], slots[4], housePrice[4],
                    minAvailable[4], mintLimits[4], housePriceTokenNetwork != null ? housePriceTokenNetwork[4] : 0),
                5 =>
                new HouseData(HouseType.SuperVilla, HouseSize.s6x30, charges[5], slots[5], housePrice[5],
                    minAvailable[5], mintLimits[5], housePriceTokenNetwork != null ? housePriceTokenNetwork[5] : 0),
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            };
        }

        public static string GetHouseName(HouseType type) {
            return type switch {
                HouseType.TinyHouse => "Tiny House",
                HouseType.MiniHouse => "Mini House",
                HouseType.LuxHouse => "Lux House",
                HouseType.PenHouse => "PentHouse",
                HouseType.Villa => "Villa",
                HouseType.SuperVilla => "Super Villa",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static string GetHouseTrackingName(HouseType type) {
            return type switch {
                HouseType.TinyHouse => "tiny_house",
                HouseType.MiniHouse => "mini_house",
                HouseType.LuxHouse => "lux_house",
                HouseType.PenHouse => "pen_house",
                HouseType.Villa => "villa",
                HouseType.SuperVilla => "super_villa",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static string GetSizeString(HouseSize size) {
            return size switch {
                HouseSize.s6x6 => "6x6",
                HouseSize.s6x10 => "6x10",
                HouseSize.s6x15 => "6x15",
                HouseSize.s6x20 => "6x20",
                HouseSize.s6x25 => "6x25",
                HouseSize.s6x30 => "6x30",
                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
            };
        }
    }
}