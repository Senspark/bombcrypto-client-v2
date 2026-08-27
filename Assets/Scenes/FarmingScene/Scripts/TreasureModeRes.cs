using System;
using System.Collections.Generic;

using App;

using Constant;

using UnityEngine;

namespace Scenes.FarmingScene.Scripts {
    /// <summary>
    /// Sprite catalog rút gọn dành riêng cho Treasure Mode.
    /// Thay cho BLGachaRes — vốn kéo theo 7 ScriptableObject và 599+ texture của mọi mode.
    /// </summary>
    [CreateAssetMenu(fileName = "TreasureModeRes", menuName = "BomberLand/TreasureModeRes")]
    public class TreasureModeRes : ScriptableObject {
        [Serializable]
        public class SpriteEntry {
            public Sprite sprite;
        }

        public SerializableDictionaryEnumKey<GachaChestProductId, SpriteEntry> chestSprites;
        public SerializableDictionaryEnumKey<BlockRewardType, SpriteEntry> blockRewardSprites;

        public Sprite GetSpriteByItemId(GachaChestProductId type) {
            if (chestSprites != null && chestSprites.TryGetValue(type, out var entry) && entry?.sprite) {
                return entry.sprite;
            }
            throw new KeyNotFoundException(
                $"{nameof(TreasureModeRes)}: thiếu sprite cho {nameof(GachaChestProductId)}.{type}");
        }

        public Sprite GetSpriteByRewardType(BlockRewardType rewardType) {
            if (blockRewardSprites != null && blockRewardSprites.TryGetValue(rewardType, out var entry) && entry?.sprite) {
                return entry.sprite;
            }
            throw new KeyNotFoundException(
                $"{nameof(TreasureModeRes)}: thiếu sprite cho {nameof(BlockRewardType)}.{rewardType}");
        }
    }
}
