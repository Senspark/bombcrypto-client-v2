using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.Purchasing;

namespace Senspark.Iap.Receipts {
    public class EditorReceiptManager {
        private readonly IDataManager _dataManager;

        private const float DefaultSubscriptionDurationSeconds = 5 * 60f;
        private readonly float _subscriptionDurationSeconds;
        private const string KSaveKey = "editor_receipts";
        private readonly List<EditorReceipt> _receipts;

        public EditorReceiptManager(IDataManager dataManager, float subscriptionDurationSeconds) {
            _dataManager = dataManager;
            _subscriptionDurationSeconds = subscriptionDurationSeconds > 0
                ? subscriptionDurationSeconds
                : DefaultSubscriptionDurationSeconds;
            
            try {
                var receipts = _dataManager.Get(KSaveKey, "[]");
                _receipts = JsonConvert.DeserializeObject<List<EditorReceipt>>(receipts);
            } catch (Exception e) {
                Debug.LogError(e.Message);
            }
        }

        public bool HasReceipt(string productId) {
            var r = _receipts.FirstOrDefault(e => e.productId == productId);
            if (r == null) {
                return false;
            }
            var purchaseTime = TimeUtils.ConvertEpochSecondsToLocalDateTime(r.purchaseTime);
            if (r.productType == ProductType.Consumable) {
                return false;
            }
            if (r.productType == ProductType.NonConsumable) {
                return true;
            }
            // Subscription
            var expiredTime = purchaseTime.AddSeconds(_subscriptionDurationSeconds);
            var expired = expiredTime < DateTime.Now;
            if (expired) {
                _receipts.Remove(r);
                Save();
            }
            return !expired;
        }

        public void AddReceipt(string productId, ProductType productType) {
            if (productType == ProductType.Consumable) {
                return;
            }
            
            RemoveReceipt(productId, false);
            _receipts.Add(new EditorReceipt {
                purchaseTime = TimeUtils.ConvertDateTimeToEpochSeconds(DateTime.Now),
                productType = productType,
                productId = productId
            });
            Save();
        }
        
        public void RemoveReceipt(string productId) {
            RemoveReceipt(productId, true);
        }

        private void RemoveReceipt(string productId, bool save) {
            var r = _receipts.FirstOrDefault(e => e.productId == productId);
            if (r != null) {
                _receipts.Remove(r);
            }

            if (save) {
                Save();
            }
        }

        private void Save() {
            var d = JsonConvert.SerializeObject(_receipts);
            _dataManager.Set(KSaveKey, d);
        }
    }

    [Serializable]
    internal class EditorReceipt {
        public long purchaseTime;
        public ProductType productType;
        public string productId;
    }
}