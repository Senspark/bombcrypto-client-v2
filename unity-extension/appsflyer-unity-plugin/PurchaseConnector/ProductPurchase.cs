using System.Collections.Generic;
using System;

namespace AppsFlyerConnector {
    [Serializable]
    public class InAppPurchaseValidationResult : EventArgs {
        public bool success;
        public ProductPurchase? productPurchase;
        public ValidationFailureData? failureData;
        public string token;
    }

    [Serializable]
    public class ProductPurchase {
        public string kind;
        public string purchaseTimeMillis;
        public int purchaseState;
        public int consumptionState;
        public string developerPayload;
        public string orderId;
        public int purchaseType = -1;
        public int acknowledgementState;
        public string purchaseToken;
        public string productId;
        public int quantity;
        public string obfuscatedExternalAccountId;
        public string obfuscatedExternalProfil;
        public string regionCode;
    }

    [Serializable]
    public class ValidationFailureData {
        public int status;
        public string description;
    }

    [Serializable]
    public class SubscriptionValidationResult {
        public bool success;
        public SubscriptionPurchase? subscriptionPurchase;
        public ValidationFailureData? failureData;
        public string token;
    }

    [Serializable]
    public class SubscriptionPurchase {
        public string acknowledgementState;
        public CanceledStateContext? canceledStateContext;
        public ExternalAccountIdentifiers? externalAccountIdentifiers;
        public string kind;
        public string latestOrderId;
        public List<SubscriptionPurchaseLineItem> lineItems;
        public string? linkedPurchaseToken;
        public PausedStateContext? pausedStateContext;
        public string regionCode;
        public string startTime;
        public SubscribeWithGoogleInfo? subscribeWithGoogleInfo;
        public string subscriptionState;
        public TestPurchase? testPurchase;
    }

    [Serializable]
    public class CanceledStateContext {
        public DeveloperInitiatedCancellation? developerInitiatedCancellation;
        public ReplacementCancellation? replacementCancellation;
        public SystemInitiatedCancellation? systemInitiatedCancellation;
        public UserInitiatedCancellation? userInitiatedCancellation;
    }

    [Serializable]
    public class ExternalAccountIdentifiers {
        public string externalAccountId;
        public string obfuscatedExternalAccountId;
        public string obfuscatedExternalProfileId;
    }

    [Serializable]
    public class SubscriptionPurchaseLineItem {
        public AutoRenewingPlan? autoRenewingPlan;
        public DeferredItemReplacement? deferredItemReplacement;
        public string expiryTime;
        public OfferDetails? offerDetails;
        public PrepaidPlan? prepaidPlan;
        public string productId;
    }

    [Serializable]
    public class PausedStateContext {
        public string autoResumeTime;
    }

    [Serializable]
    public class SubscribeWithGoogleInfo {
        public string emailAddress;
        public string familyName;
        public string givenName;
        public string profileId;
        public string profileName;
    }

    [Serializable]
    public class TestPurchase {
    }

    [Serializable]
    public class DeveloperInitiatedCancellation {
    }

    [Serializable]
    public class ReplacementCancellation {
    }

    [Serializable]
    public class SystemInitiatedCancellation {
    }

    [Serializable]
    public class UserInitiatedCancellation {
        public CancelSurveyResult? cancelSurveyResult;
        public string cancelTime;
    }

    [Serializable]
    public class AutoRenewingPlan {
        public string? autoRenewEnabled;
        public SubscriptionItemPriceChangeDetails? priceChangeDetails;
    }

    [Serializable]
    public class DeferredItemReplacement {
        public string productId;
    }

    [Serializable]
    public class OfferDetails {
        public List<string>? offerTags;
        public string basePlanId;
        public string? offerId;
    }

    [Serializable]
    public class PrepaidPlan {
        public string? allowExtendAfterTime;
    }

    [Serializable]
    public class CancelSurveyResult {
        public string reason;
        public string reasonUserInput;
    }

    [Serializable]
    public class SubscriptionItemPriceChangeDetails {
        public string expectedNewPriceChargeTime;
        public Money? newPrice;
        public string priceChangeMode;
        public string priceChangeState;
    }

    [Serializable]
    public class Money {
        public string currencyCode;
        public long nanos;
        public long units;
    }
}