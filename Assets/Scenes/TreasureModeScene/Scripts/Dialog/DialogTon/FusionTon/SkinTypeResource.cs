using System;
using System.Collections.Generic;
using App;
using Engine.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinTypeRes", menuName = "BomberLand/SkinTypeRes")]
public class SkinTypeResource : ScriptableObject
{
    [Serializable]
    public class ResourcePicker {
        public List<SkinTypeRes> skinList = new List<SkinTypeRes>();
    }
    
    [Serializable]
    public class SkinTypeRes {
        public PlayerType playerType;
        public Sprite resultIcon;
        public Sprite materialIcon;
    }
    
    public SerializableDictionaryEnumKey<HeroRarity, ResourcePicker> resource;
    
    public List<SkinTypeRes> GetSkinTypeByRarity(HeroRarity rarity) {
        return resource[rarity].skinList;
    }

    public Sprite GetSkinImgByRarity(HeroRarity rarity, PlayerType playerType) {
        foreach (var item in resource[rarity].skinList) {
            if (item.playerType == playerType) {
                return item.resultIcon;
            }
        }
        return null;
    }
}
