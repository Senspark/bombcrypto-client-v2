using System;
using App;
using Constant;

namespace Data {
    public class InventoryItemData {
        public TimeSpan Duration { get; }
        public DateTime Expire { get; set; }
        public DateTime CreateDate { get; set; }
        public int Id { get; }
        public int ItemId { get; }
        public string ItemName { get; }
        public int ItemType { get; }
        public int Quantity { get; }
        public bool Sellable { get; }
        public string[] Abilities { get; }
        public long ExpirationAfter { get; }
        public bool Equipped { get; }
        public bool IsNew { get; }
        public InventoryItemType InventoryItemType { get; }
        public bool Used { get; set; }

        public InventoryItemData(
            string[] abilities,
            int id,
            int itemId,
            string itemName,
            int itemType,
            int quantity,
            bool sellable,
            long createDate,
            long expirationAfter,
            bool equipped,
            bool isNew,
            string type,
            bool used
        ) {
            Duration = TimeSpan.FromMilliseconds(UnityEngine.Random.Range(0, short.MaxValue));
            Expire = DateTime.Now + Duration;
            CreateDate = DateTimeOffset.FromUnixTimeMilliseconds(createDate).DateTime;
            Id = id;
            ItemId = itemId;
            ItemName = itemName;
            ItemType = itemType;
            Quantity = quantity;
            Sellable = sellable;
            Abilities = abilities;
            ExpirationAfter = expirationAfter;
            Equipped = equipped;
            ItemName = ItemName.AppendTimeDay(expirationAfter);
            IsNew = isNew;
            InventoryItemType = ConvertToInventoryItemType(type);
            Used = used;
        }
        
        private static InventoryItemType ConvertToInventoryItemType(string type) {
            return type switch {
                "BOMB" => InventoryItemType.BombSkin,
                "TRAIL" => InventoryItemType.Trail,
                "WING" => InventoryItemType.Avatar,
                "HERO" => InventoryItemType.Hero,
                "BOOSTER" => InventoryItemType.Booster,
                "MISC" => InventoryItemType.Misc,
                "REWARD" => InventoryItemType.Reward,
                "FIRE" => InventoryItemType.Fire,
                "MYSTERY_BOX" => InventoryItemType.MysteryBox,
                "MATERIAL" => InventoryItemType.Altar,
                "EMOJI" => InventoryItemType.Emoji,
                "AVATAR" => InventoryItemType.AvatarTR,
                _ => throw new Exception($"Invalid Item Type: {type}")
            };
        }
    }
}
