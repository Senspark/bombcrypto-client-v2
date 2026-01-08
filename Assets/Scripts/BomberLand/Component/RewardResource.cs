using System;
using UnityEngine;

namespace BomberLand.Component {
    public enum RewardSourceType {
        Gold,
        BronzeChest,
        SilverChest,
        GoldChest,
        PlatinumChest,
        Rank
    }

    public class RewardResource : MonoBehaviour {
        [SerializeField]
        private SerializableDictionaryEnumKey<RewardSourceType, SkinPicker> resourceSkin;

        [Serializable]
        public class SkinPicker {
            public Sprite sprite;
        }

        public static RewardSourceType ConvertIdToEnum(int id) {
            // Must match BLOCK_REWARD_TYPE (server).
            return id switch {
                17 => RewardSourceType.Gold,
                20 => RewardSourceType.BronzeChest,
                21 => RewardSourceType.SilverChest,
                22 => RewardSourceType.GoldChest,
                19 => RewardSourceType.PlatinumChest,
                _ => throw new Exception($"Invalid reward id: {id}"),
            };
        }

        public static RewardSourceType ConvertStringToEnum(string rewardName) {
            return rewardName switch {
                "GOLD" => RewardSourceType.Gold,
                "BRONZE_CHEST" => RewardSourceType.BronzeChest,
                "SILVER_CHEST" => RewardSourceType.SilverChest,
                "GOLD_CHEST" => RewardSourceType.GoldChest,
                "PLATINUM_CHEST" => RewardSourceType.PlatinumChest,
                "RANK" => RewardSourceType.Rank,
                _ => throw new Exception($"Invalid Reward Name: {rewardName}")
            };
        }

        public Sprite GetSprite(RewardSourceType type) {
            return resourceSkin[type].sprite;
        }

        public Sprite GetSprite(string rewardName) {
            return resourceSkin[ConvertStringToEnum(rewardName)].sprite;
        }

        public Sprite GetSprite(int id) {
            return resourceSkin[ConvertIdToEnum(id)].sprite;
        }
    }
}