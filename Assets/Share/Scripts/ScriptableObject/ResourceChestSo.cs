using App;

using Constant;

using Game.Dialog.BomberLand.BLGacha;

using UnityEngine;

[CreateAssetMenu(fileName = "Resource Chest", menuName = "BomberLand/GachaRes/Resource Chest")]
public class ResourceChestSo : ScriptableObject {
    public SerializableDictionaryEnumKey<GachaChestProductId, BLGachaRes.ResourcePicker> resourceChest;
    public SerializableDictionaryEnumKey<BlockRewardType, BLGachaRes.ResourcePicker> resourceBlockReward;

    public Sprite GetSpriteByItemId(GachaChestProductId type) {
        if (resourceChest.TryGetValue(type, out var value)) {
            var s = value.sprite;
            if (s != null) {
                return s;
            }
        }
        return resourceChest[GachaChestProductId.Unknown].sprite;
    }

    public Sprite GetSpriteByRewardType(BlockRewardType rewardType) {
        if (resourceBlockReward.TryGetValue(rewardType, out var value)) {
            var s = value.sprite;
            if (s != null) {
                return s;
            }
        }
        return resourceChest[GachaChestProductId.Unknown].sprite;
    }
}