using System;
using System.Collections.Generic;
using App;
using Constant;
using Data;
using Engine.Entities;
using Game.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.Dialog {
    public enum BLItemType {
        Bomb,
        Booster,
        Chest,
        Trail,
        Hero,
        Misc,
        Wing,
        OnSell,
        Fire,
        Costume,
        Emoji,
        Avatar,
    }

    public class ItemData {
        public ProductData ProductData { get; }
        public DateTime SellTime { get; }
        public BLItemType ItemType { get; }
        public int ItemId { get; }
        public string ItemName { get; }
        public string Description { get; }
        public int Quantity { get; }
        public float Price { get; }
        public BlockRewardType RewardType { get; }
        public int ProductId { get; }
        public int ProductType { get; }
        public bool Sellable { get; }
        public StatData[] Stats { get; }
        public long ExpirationAfter { get; }
        public float UnitPrice { get; }
        public bool Equipped { get; private set; }
        public bool IsNew { get; private set; }
        public bool Used { get; }
        public DateTime Expire { get; }
        public DateTime CreateDate { get; set; }
        public InventoryItemType InventoryItemType { get; set; }

        public ItemData(
            BLItemType itemType,
            int itemId,
            string itemName,
            int quantity,
            bool sellable,
            StatData[] stats,
            long expirationAfter,
            bool equipped,
            bool isNew
        ) {
            ItemType = itemType;
            ItemId = itemId;
            ItemName = itemName;
            Quantity = quantity;
            Sellable = sellable;
            Stats = stats;
            ExpirationAfter = expirationAfter;
            Equipped = equipped;
            IsNew = isNew;
        }

        public ItemData(
            BLItemType itemType,
            int itemId,
            string itemName,
            int quantity,
            bool sellable,
            StatData[] stats,
            long expirationAfter,
            bool equipped,
            bool isNew,
            bool used,
            DateTime expire,
            DateTime createDate,
            InventoryItemType inventoryItemType
        ) {
            ItemType = itemType;
            ItemId = itemId;
            ItemName = itemName;
            Quantity = quantity;
            Sellable = sellable;
            Stats = stats;
            ExpirationAfter = expirationAfter;
            Equipped = equipped;
            IsNew = isNew;
            Used = used;
            Expire = expire;
            CreateDate = createDate;
            InventoryItemType = inventoryItemType;
        }

        public ItemData(
            BLItemType itemType,
            int itemId,
            float price,
            int productId,
            string itemName,
            int quantity,
            bool sellable,
            float unitPrice,
            int expirationAfter
        ) {
            ItemType = itemType;
            ItemId = itemId;
            Price = price;
            ProductId = productId;
            ItemName = itemName;
            Quantity = quantity;
            Sellable = sellable;
            UnitPrice = unitPrice;
            ExpirationAfter = expirationAfter;
        }

        public ItemData(
            ProductData productData,
            DateTime sellTime,
            BLItemType itemType,
            int itemId,
            string itemName,
            string description,
            int quantity,
            float price,
            BlockRewardType rewardType,
            int productId,
            int productType,
            StatData[] stats,
            int expirationAfter
        ) {
            ProductData = productData;
            SellTime = sellTime;
            ItemType = itemType;
            ItemId = itemId;
            ItemName = itemName;
            Description = description;
            Quantity = quantity;
            Price = price;
            RewardType = rewardType;
            ProductId = productId;
            ProductType = productType;
            Stats = stats;
            ExpirationAfter = expirationAfter;
        }

        public bool ToggleEquipped() {
            Equipped = !Equipped;
            return Equipped;
        }

        public bool DisableIsNew() {
            if (IsNew) {
                IsNew = false;
            }
            return IsNew;
        }

        public static InventoryItemType ConvertToProductType(BLItemType type) {
            return type switch {
                BLItemType.Bomb => InventoryItemType.BombSkin,
                BLItemType.Trail => InventoryItemType.Trail,
                BLItemType.Wing => InventoryItemType.Avatar,
                BLItemType.Hero => InventoryItemType.Hero,
                BLItemType.Booster => InventoryItemType.Booster,
                BLItemType.Fire => InventoryItemType.Fire,
                BLItemType.Emoji => InventoryItemType.Emoji,
                _ => throw new Exception($"Invalid Item Type: {type}")
            };
        }

        public static BLItemType ConvertToBLItemType(InventoryItemType type) {
            return type switch {
                InventoryItemType.BombSkin => BLItemType.Bomb,
                InventoryItemType.Trail => BLItemType.Trail,
                InventoryItemType.Avatar => BLItemType.Wing,
                InventoryItemType.Hero => BLItemType.Hero,
                InventoryItemType.Booster => BLItemType.Booster,
                InventoryItemType.Fire => BLItemType.Fire,
                InventoryItemType.Emoji => BLItemType.Emoji,
                _ => throw new Exception($"Invalid Item Type: {type}")
            };
        }
    }

    public class UIHeroData {
        public ProductHeroData HeroData;
        public FreeTRHeroData FreeTRHeroData;
        public int HeroId;
        public PlayerType HeroType;
        public PlayerColor HeroColor;
        public string HeroName;
        public int Quantity;
        public float Price;
        public BlockRewardType RewardType;
        public bool IsNew;

        public (int value, int max) BombRange;
        public (int value, int max) Speed;
        public (int value, int max) BombNum;
        public (int value, int max) Health;
        public (int value, int max) Damage;

        public bool Sellable;
        public IDictionary<int, int> SkinChests;

        public static UIHeroData ConvertFrom(InventoryHeroData data) {
            var range = (1, 10);
            var speed = (1, 10);
            var count = (1, 10);
            var health = (1, 10);
            var damage = (1, 10);

            foreach (var stat in data.Stats) {
                switch (stat.StatId) {
                    case (int) StatId.Range:
                        range = (stat.Value, stat.Max);
                        break;
                    case (int) StatId.Speed:
                        speed = (stat.Value, stat.Max);
                        break;
                    case (int) StatId.Count:
                        count = (stat.Value, stat.Max);
                        break;
                    case (int) StatId.Health:
                        health = (stat.Value, stat.MaxUpgradeValue);
                        break;
                    case (int) StatId.Damage:
                        damage = (stat.Value, stat.MaxUpgradeValue);
                        break;
                }
            }

            return new UIHeroData() {
                HeroId = data.HeroId,
                HeroType = ConvertFromHeroId(data.HeroId),
                HeroColor = PlayerColor.HeroTr,
                HeroName = data.HeroName,
                Quantity = data.Quantity,
                Price = 0,
                RewardType = BlockRewardType.Gem,
                Sellable = data.Sellable,
                BombRange = range,
                Speed = speed,
                BombNum = count,
                Health = health,
                Damage = damage,
                IsNew = data.IsNew
            };
        }

        public static PlayerType ConvertFromHeroId(int heroId) {
            return heroId switch {
                8 => PlayerType.Poo,
                9 => PlayerType.Knight,
                10 => PlayerType.Man,
                11 => PlayerType.GKu,
                12 => PlayerType.Witch,
                13 => PlayerType.PinkyToon,
                14 => PlayerType.Stickman,
                15 => PlayerType.Ninja,
                16 => PlayerType.Monitor,
                17 => PlayerType.Dragon,
                100 => PlayerType.Santa,
                101 => PlayerType.Miner,
                116 => PlayerType.Calico,
                117 => PlayerType.Kuroneko,
                118 => PlayerType.GoldenKat,
                119 => PlayerType.MrDear,
                120 => PlayerType.TLion,
                127 => PlayerType.Frog,
                128 => PlayerType.DogeTr,
                129 => PlayerType.KingTr,
                130 => PlayerType.Cupid,
                131 => PlayerType.BGuy,
                136 => PlayerType.PinkyBear,
                141 => PlayerType.PinkyNeko,
                143 => PlayerType.Dragoon2,
                144 => PlayerType.FatTiger,
                145 => PlayerType.Hesman,
                _ => PlayerType.Knight
            };
        }
    }

    public abstract class BaseBLItem : MonoBehaviour {
        [SerializeField]
        protected BLTablListAvatar avatar;

        [SerializeField]
        protected Text valueText;

        [SerializeField]
        private Image highLight;

        protected int Index;
        protected Action<int> OnClickCallback;
        public int itemId;

        public void SetSelected(bool value) {
            highLight.gameObject.SetActive(value);
        }

        public void SetInvisible(bool value) {
            gameObject.SetActive(value);
        }

        public void OnClicked() {
            OnClickCallback?.Invoke(Index);
        }

        public abstract void SetInfo<T>(int index,
            T itemData,
            Action<int> callback);
        
        public virtual void UpdateMinPrice(float minPrice) {}
    }
}