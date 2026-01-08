using System;

using Data;

using UnityEngine;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public enum ChestShopType {
        OtherChest,
        BronzeChest,
        SilverChest,
        GoldChest,
        PlatinumChest,
        RubyChest,
        DiamondChest,
        OrdinaryChest,
        PremiumChest,
        CosmicChest,
        GalacticChest,
        OrionChest,
        DailyTaskChest
    }

    public enum GemShopType {
        UnknownPack,
        TinyPack,
        RegularPack,
        ProPack,
        DeluxePack,
        SuperDeluxePack,
        HugePack,
        GiantPack
    }

    public enum GoldShopType {
        UnknownPack,
        TinyPack,
        RegularPack,
        ProPack,
        BigPack,
        MassivePack
    }
    
    public enum MaterialShopType {
        UnknownPack,
        TinyPack,
        MediumPack,
        ProPack,
        GiantPack,
        MegaPack,
        UltraPack
    }

    [CreateAssetMenu(fileName = "BLGachaShopRes", menuName = "BomberLand/GachaShopRes")]
    public class BLShopResource : ScriptableObject {
        [Serializable]
        public class ResourcePickerChest {
            public Sprite sprite;
            public string name;
            public Sprite[] animationOpen;
            public Sprite[] animationIdle;
            public int slots;
        }

        [Serializable]
        public class ResourcePickerKey {
            public Sprite sprite;
            public string key;
        }

        [Serializable]
        public class ResourcePickerKeyNumber {
            public Sprite sprite;
            public int key;
        }

        [Serializable]
        public class ResourcePickerIapProduct {
            public Sprite sprite;
            public string key;
        }

        [SerializeField]
        private SerializableDictionaryEnumKey<ChestShopType, ResourcePickerChest> resourceChest;

        [SerializeField]
        private SerializableDictionaryEnumKey<GemShopType, ResourcePickerKey> resourceGem;

        [SerializeField]
        private SerializableDictionaryEnumKey<GoldShopType, ResourcePickerKeyNumber> resourceGold;
        
        [SerializeField]
        private SerializableDictionaryEnumKey<MaterialShopType, ResourcePickerKey> resourceMaterial;

        [SerializeField]
        private SerializableDictionaryEnumKey<SubscriptionType, ResourcePickerIapProduct> resourceIapProduct;

        public Sprite GetImageChestShop(ChestShopType chestShopType) {
            return resourceChest.ContainsKey(chestShopType)
                ? resourceChest[chestShopType].sprite
                : resourceChest[ChestShopType.OtherChest].sprite;
        }

        public ResourcePickerChest GetChestShop(ChestShopType chestShopType) {
            return resourceChest.ContainsKey(chestShopType)
                ? resourceChest[chestShopType]
                : resourceChest[ChestShopType.OtherChest];
        }

        public Sprite GetImageIpaGem(string productId) {
            foreach (var r in resourceGem) {
                if (r.Value.key == productId) {
                    return r.Value.sprite;
                }
            }
            return resourceGem[GemShopType.UnknownPack].sprite;
        }

        public Sprite GetImageIpaGold(int itemId) {
            foreach (var r in resourceGold) {
                if (r.Value.key == itemId) {
                    return r.Value.sprite;
                }
            }
            Debug.LogWarning("GetImageIpaGold not found: " + itemId);
            return resourceGold[GoldShopType.UnknownPack].sprite;
        }
        
        public Sprite GetImageRock(string packageName) {
            foreach (var r in resourceMaterial) {
                if (r.Value.key == packageName) {
                    return r.Value.sprite;
                }
            }
            return resourceMaterial[MaterialShopType.UnknownPack].sprite;
        }

        public Sprite GetImageIpaProduct(string productId) {
            foreach (var r in resourceIapProduct) {
                if (r.Value.key == productId) {
                    return r.Value.sprite;
                }
            }
            return resourceIapProduct[SubscriptionType.Unknown].sprite;
        }
    }
}