using System.Linq;

using PvpMode.Manager;

namespace BLPvpMode.Data {
    public class BoosterStatus {
        private class BoosterItem {
            public BoosterType Type = BoosterType.Unknown;
            public int Quantity = 0;
        }

        private BoosterItem[] _boosters;

        public BoosterStatus(int size) {
            _boosters = new BoosterItem[size];
            for (var i = 0; i < size; i++) {
                _boosters[i] = new BoosterItem();
            }
        }

        public void UpdatedItem(int index, BoosterType type, int quantity) {
            _boosters[index].Type = type;
            _boosters[index].Quantity = quantity;
        }
        
        public void UpdatedQuantity(BoosterType type, int quantity) {
            for (var idx = 0; idx < _boosters.Length; idx++) {
                var b = _boosters[idx];
                if (b.Type != type) {
                    continue;
                }
                b.Quantity = quantity;
            }
        }

        public bool IsChooseBooster(BoosterType type) {
            return _boosters.Any(iter => iter.Type == type && iter.Quantity > 0);
        }

        public int GetQuantity(BoosterType type) {
            for (var i = 0; i < _boosters.Length; i++) {
                if (_boosters[i].Type == type) {
                    return _boosters[i].Quantity;
                }
            }
            return 0;
        }

        public void RemoveChooseBooster(BoosterType type) {
            for (var i = 0; i < _boosters.Length; i++) {
                if (_boosters[i].Type == type) {
                    _boosters[i].Quantity -= 1;
                    if (_boosters[i].Quantity <= 0) {
                        _boosters[i].Type = BoosterType.Unknown;
                    }
                }
            }
        }

        private BoosterItem[] GetSelectedBoosters() {
            return _boosters.Where(it => it.Type != BoosterType.Unknown).ToArray();
        }

        public int[] GetSelectedBoosterIds() {
            var selectedBoosters = GetSelectedBoosters();
            var result = new int[selectedBoosters.Length];
            for (var i = 0; i < selectedBoosters.Length; i++) {
                result[i] = DefaultBoosterManager.ConvertFromEnum(selectedBoosters[i].Type);
            }
            return result;
        }
    }
}