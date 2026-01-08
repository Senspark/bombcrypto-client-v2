using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Dialog.BomberLand.BLGacha {
    [CreateAssetMenu(fileName = "BLTooltip", menuName = "BomberLand/Tooltip")]
    public class BLTooltip : ScriptableObject {
        [Serializable]
        public class TooltipDesc {
            public string desc;
        }

        public SerializableDictionary<int, TooltipDesc> tooltips;

        public string GetRandomTooltip() {
            if (tooltips.Count == 0) {
                return "No tooltips available.";
            }
            
            var keys = new List<int>(tooltips.Keys);
            var randomIndex = Random.Range(0, keys.Count);
            var randomKey = keys[randomIndex];
            return tooltips[randomKey].desc;
        }
    }
}