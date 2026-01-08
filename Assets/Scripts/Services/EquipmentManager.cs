using System;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IEquipmentManager))]
    public interface IEquipmentManager : IService {
        public class Equipment {
            public bool Equipped { get; }
            public int ItemId { get; }

            public long ExpirationAfter { get; }
            public int ItemType { get; }
            public int Id { get; }

            public Equipment(EquipmentData data) {
                Equipped = data.Equipped;
                ItemId = data.ItemId;
                ExpirationAfter = data.ExpirationAfter;
                ItemType = data.ItemType;
                throw new NotImplementedException();
                // Id = data.Id;
            }
        }
    }
}