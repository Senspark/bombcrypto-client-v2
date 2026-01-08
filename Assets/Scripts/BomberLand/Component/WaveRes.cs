using System;

using UnityEngine;

namespace Animation {
    public enum WaveType {
        Wave1 = 1,
        Wave2 = 2,
        Wave3 = 3,
        Wave4 = 4,
        Wave5 = 5,
    }

    [CreateAssetMenu(fileName = "WaveRes", menuName = "BomberLand/WaveRes")]
    public class WaveRes : UnityEngine.ScriptableObject {
        [Serializable]
        public class ResourcePicker {
            public Sprite[] sprites;
        }

        public SerializableDictionary<WaveType, ResourcePicker> resource;

        public Sprite[] GetSprite(WaveType type) {
            return resource[type].sprites;
        }
    }
}