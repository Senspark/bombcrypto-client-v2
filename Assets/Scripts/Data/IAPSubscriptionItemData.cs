using Newtonsoft.Json;

namespace Data {
    public enum SubscriptionType {
        Unknown,
        SubscriptionStandard,
        SubscriptionDeluxe,
        SubscriptionPremium,
    }

    public enum SubscriptionState {
        Invalid,
        Pending,
        Active,
        Expired
    }

    public class IAPSubscriptionUserPackage {
        public long StartTime { get; }
        public long EndTime { get; }
        public SubscriptionState State { get; set; }

        [JsonConstructor]
        public IAPSubscriptionUserPackage(
            [JsonProperty("start_time")] long startTime,
            [JsonProperty("end_time")] long endTime,
            [JsonProperty("state")] string state
        ) {
            StartTime = startTime;
            EndTime = endTime;
            State = state switch {
                "INVALID" => SubscriptionState.Invalid,
                "PENDING" => SubscriptionState.Pending,
                "ACTIVE" => SubscriptionState.Active,
                "EXPIRED" => SubscriptionState.Expired,
                _ => SubscriptionState.Invalid
            };
        }
    }

    public class IAPSubscribeResult {
        public string ProductId { get; }
        public string Token { get; }

        public IAPSubscribeResult(
            [JsonProperty("product_id")] string productId,
            [JsonProperty("token")] string token
        ) {
            ProductId = productId;
            Token = token;
        }
    }

    public class IAPSubscriptionItemData {
        public string ProductId { get; }
        public string ItemName { get; }
        public string Description { get; }
        public IAPSubscriptionUserPackage UserPackage { get; }
        public string ItemPrice { get; set; }

        [JsonConstructor]
        public IAPSubscriptionItemData(
            [JsonProperty("product_id")] string productId,
            [JsonProperty("name")] string itemName,
            [JsonProperty("description")] string description,
            [JsonProperty("user_package")] IAPSubscriptionUserPackage userPackage
        ) {
            ProductId = productId;
            ItemName = itemName;
            Description = description;
            UserPackage = userPackage;
            ItemPrice = "--";
        }

        public static SubscriptionType GetSubscriptionType(string productId) {
            return productId switch {
                "subscription_standard" => SubscriptionType.SubscriptionStandard,
                "subscription_deluxe" => SubscriptionType.SubscriptionDeluxe,
                "subscription_premium" => SubscriptionType.SubscriptionPremium,
                _ => SubscriptionType.Unknown
            };
        }
    }
}