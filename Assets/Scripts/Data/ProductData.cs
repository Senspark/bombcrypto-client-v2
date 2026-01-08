using System;

using Constant;

using JetBrains.Annotations;

using Newtonsoft.Json;

using Services.Server;

namespace Data {
    public static class MaxPriceConfig {
        public const float Hero = 44;
        public const float Booster1 = 8.3f;
        public const float Booster2 = 16.6f;
        public const float Premium = 120;
        public const float Normal = 40;
    }

    public class ProductDataResponse {
        public int ItemId;
        public PriceData Price;
        public int Quantity;
        public float UnitPrice;
        public int ExpirationAfter;

        [JsonConstructor]
        public ProductDataResponse(
            [JsonProperty("quantity")] int quantity,
            [JsonProperty("item_id")] int itemId,
            [JsonProperty("reward_type")] int rewardType,
            [JsonProperty("price")] float price,
            [JsonProperty("unit_price")] float unitPrice,
            [JsonProperty("expiration_after")] int expirationAfter
        ) {
            ItemId = itemId;
            Price = new PriceData(rewardType, price);
            Quantity = quantity;
            UnitPrice = unitPrice;
            ExpirationAfter = expirationAfter;
        }
    }

    public class ProductData {
        public string[] Abilities;
        public string Description;
        public int ItemId;
        public PriceData Price;
        public int ProductId;
        public string ProductName;
        public int ProductType;
        public int Type;
        public int Quantity;
        public DateTime SellTime;
        public float UnitPrice;
        public PriceShopData[] PriceShop;
        public String Kind;
        public int ExpirationAfter;
        public ItemMarketConfig ItemConfig;

        public ProductData() {
        }

        [JsonConstructor]
        public ProductData(
            [JsonProperty("date_time")] long dateTime,
            [JsonProperty("description_en")] string description,
            [JsonProperty("id")] int id,
            [JsonProperty("item_id")] int itemId,
            [JsonProperty("item_type")] int itemType,
            [JsonProperty("type")] int type,
            [JsonProperty("name")] string name,
            [JsonProperty("price")] float price,
            [JsonProperty("quantity")] int quantity,
            [JsonProperty("reward_type")] int rewardType,
            [JsonProperty("abilities")] string ability,
            [JsonProperty("prices")] PriceShopData[] priceData,
            [JsonProperty("kind")] string kind,
            [JsonProperty("expiration_after")] int expirationAfter
        ) {
            Description = description;
            ItemId = itemId;
            Price = new PriceData(rewardType, price);
            ProductId = id;
            ProductName = name;
            ProductType = itemType;
            Type = type;
            Quantity = quantity;
            SellTime = DateTime.UnixEpoch.AddMilliseconds(dateTime);
            Kind = kind;
            PriceShop = priceData;
            ExpirationAfter = expirationAfter;
        }

        public float GetMaxGemPriceShop() {
            var maxGem = 0f;
            for (var i = 0; i < PriceShop.Length; i++) {
                if (maxGem < PriceShop[i].Price && PriceShop[i].RewardType == "GEM") {
                    maxGem = PriceShop[i].Price;
                }
            }
            if (maxGem == 0) {
                if (Type == (int)InventoryItemType.Booster) {
                    if (ItemId == 21 || ItemId == 23) {
                        maxGem = MaxPriceConfig.Booster2;
                    } else {
                        maxGem = MaxPriceConfig.Booster1;
                    }
                }
                else if (Type == (int)InventoryItemType.Hero) {
                    maxGem = MaxPriceConfig.Hero;
                } else if (Kind == "PREMIUM") {
                    maxGem = MaxPriceConfig.Premium;
                } else {
                    maxGem = MaxPriceConfig.Normal;
                }
            }
            return maxGem;
        }
    }
}