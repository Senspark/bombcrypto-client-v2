 using System;
 using System.Linq;

 using Senspark;

 using Services;

 using Sfs2X.Entities.Data;

 using Utils;

 namespace Data {
    public class EquipmentData {
        public readonly int[] Ids;
        public readonly bool IsForever;
        public readonly long ExpirationAfter;
        public readonly bool Equipped;
        public readonly DateTime Expire;
        public readonly int ItemId;
        public readonly int ItemType;
        public readonly int Quantity;
        public readonly bool Used;
        public readonly StatData[] Stats;

        private EquipmentData(
            int[] ids,
            bool isForever,
            long expirationAfter,
            bool equipped,
            DateTime expire,
            int itemId,
            int itemType,
            int quantity,
            bool used,
            StatData[] stats
        ) {
            // var logManager = ServiceLocator.Resolve<ILogManager>();
            // logManager.Log($"equipmentId: {itemId}, equipmentType: {itemType}, expire: {expire}, equipped: {equipped}");
            Ids = ids;
            IsForever = isForever;
            ExpirationAfter = expirationAfter;
            Used = used;
            Equipped = equipped;
            Expire = expire;
            ItemId = itemId;
            ItemType = itemType;
            Quantity = quantity;
            Stats = stats;
        }

        public static EquipmentData Parse(ISFSObject sfsObject) {
            var ids = sfsObject.GetIntArray("ids");
            var itemId = sfsObject.GetInt("item_id");
            var used = sfsObject.ContainsKey("expiry_date");
            var abilityManager = ServiceLocator.Instance.Resolve<IAbilityManager>();
            var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            var isForever = sfsObject.ContainsKey("expiration_after") && sfsObject.GetLong("expiration_after") == 0; 
            return new EquipmentData(
                ids,
                isForever,
                sfsObject.ContainsKey("expiration_after") ? sfsObject.GetLong("expiration_after") : 0,
                sfsObject.GetBool("active"),
                DateTime.UnixEpoch.AddMilliseconds(used ? sfsObject.GetLong("expiry_date") : 0),
                itemId,
                sfsObject.GetInt("type"),
                sfsObject.GetInt("quantity"),
                used,
                productItemManager
                    .GetItem(itemId)
                    .Abilities
                    .SelectMany(it => abilityManager.GetStats(it))
                    .Sum()
                    .Select(it => new StatData(it.Key, 0, 0, it.Value))
                    .ToArray()
            );
        }

        public static EquipmentData ParseOtherUserInfo(ISFSObject data) {
            var ids = new int[1]; 
            ids[0] = data.GetInt("id");
            var itemId = data.GetInt("item_id");
            var abilityManager = ServiceLocator.Instance.Resolve<IAbilityManager>();
            var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            return new EquipmentData(
                ids,
                true,
                0,
                true,
                DateTime.UnixEpoch.AddMilliseconds(0),
                itemId,
                data.GetInt("type"),
                1,
                true,
                productItemManager
                    .GetItem(itemId)
                    .Abilities
                    .SelectMany(it => abilityManager.GetStats(it))
                    .Sum()
                    .Select(it => new StatData(it.Key, 0, 0, it.Value))
                    .ToArray()
            );

        }
    }
}