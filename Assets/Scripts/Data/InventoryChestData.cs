using System;
using System.Collections.Generic;

using Senspark;

using Game.Dialog.BomberLand.BLFrameShop;

using Services;

namespace Data {
    public enum InventoryChestStage {
        Unknown,
        SlotEmpty,
        SlotVip,
        SlotCanBuy,
        WaitActive,
        CountDown,
        CanOpen
    }

    public class InventoryChestData {
        public string SlotType { get; }
        public bool IsOwner { get; }
        public int Price { get; }
        public int SlotNumber { get; }
        public bool HasChestData { get; }
        public int ChestId { get; }
        public string ChestName { get; }
        public int ChestType { get; }
        public bool Counting { get; }
        public InventoryChestItemData[] DropRate { get; }
        public TimeSpan Duration { get; }
        public DateTime OpenTime { get; }
        public TimeSpan SkipTimePerAds { get; }
        public int SkipOpenTimeGemRequire { get; }

        public InventoryChestData(
            InventoryManager.InventorySlotChestData data,
            IGachaChestNameManager chestNameManager,
            IGachaChestItemManager chestItemManager
        ) {
            SlotType = data.SlotType;
            Price = data.Price;
            IsOwner = data.IsOwner;
            SlotNumber = data.SlotNumber;
            var chest = data.Chest;
            if (chest.ChestId == 0 && chest.ChestType == 0 && chest.TotalOpenTime == 0) {
                HasChestData = false;
                return;
            }
            HasChestData = true;
            ChestId = chest.ChestId;
            ChestName = chestNameManager.GetChestName(chest.ChestType);
            ChestType = chest.ChestType;
            Counting = chest.RemainingTime != -1;
            var items = chestItemManager.GetItems((ChestShopType) chest.ChestType);
            var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            var dropItems = new List<InventoryChestItemData>();
            foreach (var item in items) {
                var it = productItemManager.GetItem(item);
                dropItems.Add(new InventoryChestItemData(it.Description, it.ItemId, it.Name));
            }
            DropRate = dropItems.ToArray();
            Duration = TimeSpan.FromMilliseconds(chest.TotalOpenTime);
            OpenTime = DateTime.Now.Add(TimeSpan.FromMilliseconds(chest.RemainingTime));
            SkipTimePerAds = TimeSpan.FromMilliseconds(chest.SkipTimePerAds);
            SkipOpenTimeGemRequire = chest.SkipOpenTimeGemRequire;
        }

        public InventoryChestData(
            int chestId,
            string chestName,
            int chestType,
            bool counting,
            InventoryChestItemData[] dropRate,
            TimeSpan duration,
            DateTime openTime
        ) {
            ChestId = chestId;
            ChestName = chestName;
            ChestType = chestType;
            Counting = counting;
            DropRate = dropRate;
            Duration = duration;
            OpenTime = openTime;
        }

        public InventoryChestStage GetChestStage() {
            if (!HasChestData) {
                return SlotType switch {
                    "FREE" => InventoryChestStage.SlotEmpty,
                    "VIP" => InventoryChestStage.SlotVip,
                    "BUY" => IsOwner ? InventoryChestStage.SlotEmpty : InventoryChestStage.SlotCanBuy,
                    _ => InventoryChestStage.Unknown
                };
            }
            if (!Counting) {
                return InventoryChestStage.WaitActive;
            }
            if ((OpenTime - DateTime.Now).TotalMilliseconds > 0) {
                return InventoryChestStage.CountDown;
            }
            return InventoryChestStage.CanOpen;
        }

        public TimeSpan GetTimeCountDown() {
            var ms = (OpenTime - DateTime.Now).TotalMilliseconds;
            if (ms < 0) {
                ms = 0;
            }
            return TimeSpan.FromMilliseconds(ms);
        }
    }
}