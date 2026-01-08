using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

using UnityEngine;

namespace Senspark.Iap.Receipts {
    public class GoogleReceiptParser {
        private const string LogTag = "[Senspark][Google Parser]";
        public static IReceipt Parse(string receipt) {
            try {
                var receiptDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receipt);
                var payloadString = (string) receiptDict["Payload"];
                var transactionId = (string) receiptDict["TransactionID"];
                var payloadDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadString);
                var googleReceipt = (string) payloadDict["json"];
                var googleSignature = (string) payloadDict["signature"];
                var jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(googleReceipt);

                // https://developers.google.com/android-publisher/api-ref/rest/v3/purchases.products
                FastLog.Info($"{LogTag} Receipt: {ConvertDictionaryToString(jsonDict)}");

                var purchaseState = PurchaseState.Purchased;
                if (jsonDict.TryGetValue("purchaseState", out var ps)) {
                    purchaseState = (PurchaseState) (long) ps;
                }

                var consumptionState = ConsumptionState.NotYetConsume;
                if (jsonDict.TryGetValue("consumptionState", out var cs)) {
                    consumptionState = (ConsumptionState) (long) cs;
                }
                var isTestPurchase = false;
                if (jsonDict.TryGetValue("purchaseType", out var value)) {
                    isTestPurchase = (long) value == 0;
                }

                return new Receipt(purchaseState, consumptionState, transactionId, isTestPurchase);
            } catch (Exception e) {
                FastLog.Error($"{LogTag} {e.Message}");
                return new Receipt(PurchaseState.Purchased, ConsumptionState.Consumed, string.Empty, false);
            }
        }
        
        private class Receipt : IReceipt {
            public PurchaseState PurchaseState { get; }
            public ConsumptionState ConsumptionState { get; }
            public string TransactionId { get; }
            public bool IsTestPurchase { get; }

            public Receipt(PurchaseState purchaseState, ConsumptionState consumptionState, string transactionId,
                bool isTestPurchase) {
                PurchaseState = purchaseState;
                ConsumptionState = consumptionState;
                TransactionId = transactionId;
                IsTestPurchase = isTestPurchase;
            }
        }

        private static string ConvertDictionaryToString(Dictionary<string, object> dictionary) {
            var sb = new StringBuilder();

            foreach (var kvp in dictionary) {
                sb.Append(kvp.Key);
                sb.Append(": ");

                if (kvp.Value != null) {
                    sb.Append(kvp.Value);
                } else {
                    sb.Append("null");
                }

                sb.Append(", ");
            }

            // Remove the trailing comma and space
            if (sb.Length > 2) {
                sb.Length -= 2;
            }

            return sb.ToString();
        }
    }
}