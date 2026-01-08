using System;
using UnityEngine;

namespace Ton.Leaderboard {
    public enum AirdropRankType {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Mega,
    }

    [CreateAssetMenu(fileName = "AirdropRankTypeRes", menuName = "BomberLand/AirdropRankTypeRes")]
    public class AirdropRankTypeResource : ScriptableObject {
        [Serializable]
        public class ResourcePicker {
            public Sprite icon;
        }
        
        public SerializableDictionaryEnumKey<AirdropRankType, ResourcePicker> resource;

        public Sprite GetAirdropRankTypeIcon(AirdropRankType type) {
            if (resource.ContainsKey(type)) {
                var icon = resource[type].icon;
                icon.texture.filterMode = FilterMode.Point;
                return icon;
            }
            return resource[AirdropRankType.Bronze].icon;
        }
    }
}