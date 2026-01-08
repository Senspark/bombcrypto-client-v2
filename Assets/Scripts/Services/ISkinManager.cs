using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Constant;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(ISkinManager))]
    public interface ISkinManager : IService {
        public class EquippedSkin {
            public int SkinId { get; private set; }
            public int SkinType { get; }

            public EquippedSkin(int skinId, int skinType) {
                SkinId = skinId;
                SkinType = skinType;
            }

            public EquippedSkin(SkinId skinId, SkinType skinType) : this((int) skinId, (int) skinType) { }

            public void UpdateSkin(int skinId) {
                SkinId = skinId;
            }
        }

        public class Skin {
            public int[] Ids { get; }
            public bool IsForever { get; private set; }
            public long ExpirationAfter { get; private set; }
            public bool Equipped { get; private set; }
            public DateTime Expire { get; private set; }
            public int Quantity { get; }
            public int SkinId { get; }
            public string SkinName { get; }
            public bool Used { get; private set; }
            public int ItemType { get; }
            public StatData[] Stats { get; }

            public Skin(
                int[] ids,
                bool isForever,
                long expirationAfter,
                bool equipped,
                DateTime expire,
                int quantity,
                int skinId,
                string skinName,
                bool used,
                int itemType,
                StatData[] stats
            ) {
                Ids = ids;
                IsForever = isForever;
                ExpirationAfter = expirationAfter;
                Equipped = equipped;
                Expire = expire;
                Quantity = quantity;
                SkinId = skinId;
                SkinName = skinName;
                Used = used;
                ItemType = itemType;
                Stats = stats;
                SkinName = SkinName.AppendTimeDay(expirationAfter);
            }

            public void UnEquip() {
                Equipped = false;
            }

            public void UpdateEquipped() {
                Equipped = !Equipped;
                if (Used) {
                    return;
                }
                Used = true;
                if (ExpirationAfter > 0) {
                    Expire = DateTime.UtcNow + TimeSpan.FromMilliseconds(ExpirationAfter);
                } else {
                    Expire = DateTime.UtcNow + ServiceLocator.Instance.Resolve<IItemUseDurationManager>().GetDuration();
                }
            }
        }

        Task EquipSkinAsync(int itemType, IEnumerable<(int, long)> itemList);
        Task<IEnumerable<Skin>> GetSkinsAsync(int skinType);
        Task<IEnumerable<Skin>> GetSkinsEquipped();
        Task<IEnumerable<Skin>> GetSkinsEquipped(EquipmentData[] equipments);
    }
}