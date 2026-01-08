using System;
using App;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "AirdropRewardTypeRes", menuName = "BomberLand/AirdropRewardTypeRes")]
public class AirdropRewardTypeResource : ScriptableObject {
    [Serializable]
    public class ResourcePicker {
        public string text;
        public TMP_SpriteAsset spriteAsset;
        public Sprite icon;
    }
        
    public SerializableDictionaryEnumKey<BlockRewardType, ResourcePicker> resource;

    public string GetAirdropText(BlockRewardType type) {
        if (resource.ContainsKey(type)) {
            return resource[type].text;
        }
        return "";
    }
    
    public TMP_SpriteAsset GetAirdropSpriteAsset(BlockRewardType type) {
        if (resource.ContainsKey(type)) {
            return resource[type].spriteAsset;
        }
        return null;
    }
    
    public Sprite GetAirdropIcon(BlockRewardType type) {
        if (resource.ContainsKey(type)) {
            return resource[type].icon;
        }
        return null;
    }
}