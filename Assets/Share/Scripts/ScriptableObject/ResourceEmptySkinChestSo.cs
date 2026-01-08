using System.Collections;
using System.Collections.Generic;

using Engine.Entities;

using Game.Dialog.BomberLand.BLGacha;

using UnityEngine;
[CreateAssetMenu(fileName = "Resource Empty Skin Chest", menuName = "BomberLand/GachaRes/Resource Empty Skin Chest")]
public class ResourceEmptySkinChestSo : ScriptableObject
{
    public SerializableDictionaryEnumKey<SkinChestType, BLGachaRes.ResourcePicker> resourceEmptySkinChest;
    
    public Sprite GetSpriteByEmptySkinChest(SkinChestType type) {
        if (resourceEmptySkinChest.TryGetValue(type, out var value)) {
            var s = value.sprite;
            if (s != null) {
                return s;
            }
        }
        return resourceEmptySkinChest[SkinChestType.Misc].sprite;
    }
}
