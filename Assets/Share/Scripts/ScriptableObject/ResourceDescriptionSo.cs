using System;
using System.Collections;
using System.Collections.Generic;

using Constant;

using Game.Dialog.BomberLand.BLGacha;

using UnityEngine;
[CreateAssetMenu(fileName = "Resource Description", menuName = "BomberLand/GachaRes/Resource Description")]
public class ResourceDescriptionSo : ScriptableObject
{
    public SerializableDictionaryEnumKey<GachaChestProductId, BLGachaRes.ResourceDesc> resourceDescription;
    
    public string GetDescription(int itemId) {
        var productId = (GachaChestProductId)itemId;
        return resourceDescription.TryGetValue(productId, out var value)
            ? value.desc
            : String.Empty;
    }
}
