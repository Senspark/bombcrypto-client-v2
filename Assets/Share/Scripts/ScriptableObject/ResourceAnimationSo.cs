using System.Collections.Generic;

using Animation;

using Constant;

using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;

using Services;

using UnityEngine;

[CreateAssetMenu(fileName = "Resource Animation", menuName = "BomberLand/GachaRes/Resource Animation")]
public class ResourceAnimationSo : ScriptableObject {
    public SerializableDictionaryEnumKey<GachaChestProductId, BLGachaRes.ResourceAnimationPicker> resourceAnimation;
    public SerializableDictionaryEnumKey<GachaRankChange, BLGachaRes.ResourceAnimationPicker> resourceRankAnimation;

    [SerializeField]
    protected TextAsset textEarlyConfig;

    public BLGachaRes.ResourceAnimationPicker GetAnimation(GachaChestProductId type) {
        return resourceAnimation.TryGetValue(type, out var value)
            ? value
            : resourceAnimation[GachaChestProductId.Unknown];
    }

    public BLGachaRes.ResourceAnimationPicker GetAnimation(GachaRankChange type) {
        return resourceRankAnimation.TryGetValue(type, out var value)
            ? value
            : resourceAnimation[GachaChestProductId.Unknown];
    }

    [Button]
    public void UpdateProductType() {
        var earlyConfigManager = new EarlyConfigManager(null);
        earlyConfigManager.Initialize(textEarlyConfig.text);

        var productList = new Dictionary<GachaChestProductId, InventoryItemType>();
        foreach (var item in earlyConfigManager.Items) {
            productList[(GachaChestProductId)item.ItemId] = (InventoryItemType)item.ItemType;
        }

        foreach (var it in resourceAnimation) {
            if (productList.ContainsKey(it.Key)) {
                it.Value.Type = productList[it.Key];
            } else {
                it.Value.Type = 0;
            }
        }
        Debug.Log($"Update Type Done with {earlyConfigManager.Items.Length} items");
    }

    [Button]
    public void UpdateAnimationPath() {
        Debug.Log($"ResourceChest size is: {resourceAnimation.Count}");
        foreach (var it in resourceAnimation) {
            switch (it.Value.Type) {
                case InventoryItemType.Hero:
                    var charName = UIHeroData.ConvertFromHeroId((int)it.Key);
                    it.Value.AnimationIdle = AnimationResource.LoadSpritesHeroTr(charName);
                    break;
                case InventoryItemType.Avatar:
                    it.Value.AnimationIdle = AnimationResource.LoadSpritesAvatar((int)it.Key);
                    break;
                case InventoryItemType.BombSkin:
                    it.Value.AnimationIdle = AnimationResource.LoadSpriteBomb((int)it.Key);
                    break;
                case InventoryItemType.Fire:
                    it.Value.AnimationIdle = AnimationResource.LoadSpriteExplode((int)it.Key);
                    break;
                case InventoryItemType.Emoji:
                    it.Value.AnimationIdle = AnimationResource.LoadSpriteEmoji((int)it.Key);
                    break;
            }
        }
    }
}