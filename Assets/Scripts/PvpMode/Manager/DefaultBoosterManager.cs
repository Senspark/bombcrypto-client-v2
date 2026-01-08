using System;
using System.Linq;
using System.Threading.Tasks;

using Data;

using Sfs2X.Entities.Data;

namespace PvpMode.Manager {
    public class DefaultBoosterManager : IBoosterManager {
        public static int ConvertFromEnum(BoosterType type) {
            return type switch {
                BoosterType.Key => 18,
                BoosterType.Shield => 19,
                BoosterType.RankGuardian => 20,
                BoosterType.FullRankGuardian => 21,
                BoosterType.CupBonus => 22,
                BoosterType.FullCupBonus => 23,
                BoosterType.BombAddOne => 26,
                BoosterType.SpeedAddOne => 27,
                BoosterType.RangeAddOne => 28,
                _ => throw new Exception($"Invalid BoosterType {type}")
            };
        }

        public static BoosterType ConvertFromId(int boosterId) {
            return boosterId switch {
                18 => BoosterType.Key,
                19 => BoosterType.Shield,
                20 => BoosterType.RankGuardian,
                21 => BoosterType.FullRankGuardian ,
                22 => BoosterType.CupBonus,
                23 => BoosterType.FullCupBonus,
                26 => BoosterType.BombAddOne,
                27 => BoosterType.SpeedAddOne,
                28 => BoosterType.RangeAddOne,
                _ => throw new Exception($"Invalid Booster Id {boosterId}")
            };
        }
        
        private class Booster : IBooster {
            public BoosterType Type { get; }
            public bool IsCombo { get; }
            public IBooster[] ComboItems { get; }
            public int Price { get; }
            public int Quantity { get; }
            public bool Locked { get; }
            public DateTime UnlockTime { get; }

            private Booster(ISFSObject data) {
                Type = (BoosterType) data.GetInt("type");
                IsCombo = data.GetBool("is_combo");
                Price = data.GetInt("price");
                Quantity = data.GetInt("quantity");
                Locked = data.GetBool("locked");
                UnlockTime = DateTime.UnixEpoch.AddMilliseconds(data.GetLong("time"));

                if (!IsCombo) {
                    return;
                }
                var array = data.GetSFSArray("combo_items");
                ComboItems = ParseBoosters(array);
            }

            private static IBooster[] ParseBoosters(ISFSArray array) {
                var boosters = new IBooster[array.Count];
                for (var i = 0; i < array.Count; i++) {
                    boosters[i] = new Booster(array.GetSFSObject(i));
                }
                return boosters;
            }

            public static IBooster[] ParseBoosterShop(ISFSObject data) {
                var array = data.GetSFSArray("boosters");
                return ParseBoosters(array);
            }
        }

        public event IBoosterManager.BoosterChanged EventBoosterChanged; 

        private IBooster[] _shopBoosters;
        private IBooster[] _userBoosters;

        private BoosterData[] _boosterData;
        
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public static IBooster[] ParseBoosters(ISFSObject data) {
            return Booster.ParseBoosterShop(data);
        }

        public void SetShopBoosters(IBooster[] boosters) {
            _shopBoosters = boosters;
        }

        public IBooster GetShopBooster(BoosterType type) {
            return _shopBoosters?.FirstOrDefault(booster => booster.Type == type);
        }

        public void SetUserBoosters(IBooster[] boosters) {
            _userBoosters = boosters;
            EventBoosterChanged?.Invoke(boosters);
        }

        public IBooster GetUserBooster(BoosterType type) {
            return _userBoosters?.FirstOrDefault(booster => booster.Type == type);
        }

        public void SetBoosterData(BoosterData[] boosters) {
            _boosterData = boosters;
        }

        public BoosterData GetDataBooster(BoosterType type) {
            var boosterId = ConvertFromEnum(type);
            return _boosterData?.FirstOrDefault(booster => booster.BoosterId == boosterId);
        }
    }
}