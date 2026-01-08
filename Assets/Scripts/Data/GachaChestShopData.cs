using App;

using Constant;

using Game.Dialog.BomberLand.BLFrameShop;

namespace Data {
    public class GachaChestPrice {
        public BlockRewardType RewardType { get; }
        public int Quantity { get; }
        public int Price { get; }

        public GachaChestPrice(
            int rewardType,
            int quantity,
            int price
        ) {
            RewardType = (BlockRewardType) rewardType;
            Quantity = quantity;
            Price = price;
        }
    }

    public class GachaChestShopData {
        public ChestShopType ChestType { get; }
        public int ItemQuantity { get; }
        public int[] Items { get; }
        public GachaChestPrice[] Prices { get; }

        public GachaChestShopData(
            int chestType,
            int itemQuantity,
            int[] items,
            GachaChestPrice[] prices
        ) {
            ChestType = (ChestShopType) chestType;
            ItemQuantity = itemQuantity;
            Items = items;
            Prices = prices;
        }
    }
}