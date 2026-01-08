using System.Collections.Generic;

namespace Services.IapAds {
    public static class IapConfig {
        public const string STARTER_OFFER = "starter_offer";
        public const string HERO_OFFER = "hero_offer";
        public const string PREMIUM_OFFER = "premium_offer";
        
        public const string TINY_PACK = "tiny_pack";
        public const string REGULAR_PACK = "regular_pack";
        public const string PRO_PACK = "pro_pack";
        public const string DELUXE_PACK = "deluxe_pack";
        public const string SUPER_DELUXE_PACK = "super_deluxe_pack";
        public const string HUGE_PACK = "huge_pack";
        public const string GIANT_PACK = "giant_pack";
        
        public const string SUBSCRIPTION_STANDARD = "subscription_standard";
        public const string SUBSCRIPTION_DELUXE = "subscription_deluxe";
        public const string SUBSCRIPTION_PREMIUM = "subscription_premium";

        public static readonly List<string> GemPackIds = new() {
            TINY_PACK,
            REGULAR_PACK,
            PRO_PACK,
            DELUXE_PACK,
            SUPER_DELUXE_PACK,
            HUGE_PACK,
            GIANT_PACK,
        };

        public static readonly List<string> OfferPackIds = new() {
            STARTER_OFFER,
            HERO_OFFER,
            PREMIUM_OFFER
        };

        public static readonly List<string> SubscriptionPackIds = new() {
            SUBSCRIPTION_STANDARD,
            SUBSCRIPTION_DELUXE,
            SUBSCRIPTION_PREMIUM,
        };


        public static readonly Dictionary<string, float> ProductsPricesUsd = new() {
            [TINY_PACK] = 1.99f,
            [REGULAR_PACK] = 4.99f,
            [PRO_PACK] = 9.99f,
            [DELUXE_PACK] = 24.99f,
            [SUPER_DELUXE_PACK] = 39.99f,
            [HUGE_PACK] = 79.99f,
            [GIANT_PACK] = 159.99f,
            
            [STARTER_OFFER] = 0.99f,
            [HERO_OFFER] = 4.99f,
            [PREMIUM_OFFER] = 3.99f,
        };

        public static readonly Dictionary<string, float> SubscriptionsPricesUsd = new() {
            [SUBSCRIPTION_STANDARD] = 1.99f,
            [SUBSCRIPTION_DELUXE] = 9.99f,
            [SUBSCRIPTION_PREMIUM] = 19.99f,
        };
    }
}