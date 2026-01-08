using System.Collections;
using System.Collections.Generic;

using Engine.Entities;

using Game.Dialog.BomberLand.BLGacha;

using UnityEngine;

[CreateAssetMenu(fileName = "Resource Item", menuName = "BomberLand/GachaRes/Resource Item")]
public class ResourceItemSo : ScriptableObject
{
    public SerializableDictionaryEnumKey<ItemType, BLGachaRes.ResourcePicker> resourceItem;

    public Sprite GetSpriteByItemType(ItemType type) {
        return resourceItem[type].sprite;
    }
}
