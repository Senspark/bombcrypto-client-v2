using Newtonsoft.Json;

namespace App {
    public partial class NewServerRequester {
        private struct IAPBuyResult {
            [JsonProperty("ec")]
            public int Code;

            [JsonProperty("es")]
            public string Message;

            [JsonProperty("success")]
            public bool Success;
        }
        
        private class GetNewcomerGiftResult {
            public class NewComerGift {
                [JsonProperty("item_id")]
                public int ItemId;

                [JsonProperty("quantity")]
                public int Quantity;
            }

            [JsonProperty("items")]
            public NewComerGift[] Items;

            [JsonProperty("ec")]
            public int Code;

            [JsonProperty("es")]
            public string Message;
        }
        
        private class Crystal {
            [JsonProperty("item_id")]
            public int ItemId;

            [JsonProperty("quantity")]
            public int Quantity;
        }
        
        private class UpgradeCrystalResult {
            [JsonProperty("ec")]
            public int Code;

            [JsonProperty("es")]
            public string Message;

            [JsonProperty("item_id")]
            public int ItemId;

            [JsonProperty("quantity")]
            public int Quantity;
        }
        
        private class GetOfflineRewardResult {
            public class GetOfflineRewardItem {
                [JsonProperty("item_id")]
                public int ItemId;

                [JsonProperty("quantity")]
                public int Quantity;
            }

            [JsonProperty("last_logout")]
            public long LastLogout;

            [JsonProperty("items")]
            public GetOfflineRewardItem[] Items;
        }
        
        private class ClaimOfflineRewardResult {
            public class ClaimOfflineRewardItem {
                [JsonProperty("item_id")]
                public int ItemId;

                [JsonProperty("quantity")]
                public int Quantity;
            }
            
            [JsonProperty("items")]
            public ClaimOfflineRewardItem[] Items;

            [JsonProperty("offline_hours")]
            public int OfflineHours;
        }
        
        private class GachaChestExtensionPrice {
            [JsonProperty("reward_type")]
            public int RewardType;

            [JsonProperty("quantity")]
            public int Quantity;

            [JsonProperty("price")]
            public int Price;
        }

        private class GachaChestExtensionData {
            [JsonProperty("chest_type")]
            public int ChestType;

            [JsonProperty("items_quantity")]
            public int ItemQuantity;

            [JsonProperty("item_ids")]
            public int[] Items;

            [JsonProperty("prices")]
            public GachaChestExtensionPrice[] Prices;
        }

        private class GetGachaChestShopResult {
            [JsonProperty("data")]
            public GachaChestExtensionData[] Chests;

            [JsonProperty("ec")]
            public int Code;

            [JsonProperty("es")]
            public string Message;
        }
    }
}