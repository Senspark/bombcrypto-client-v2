using UnityEngine;

namespace Engine.ScriptableObject {
    [CreateAssetMenu(fileName = "BLHardDropRes", menuName = "BomberLand/HardDropRes")]
    public class BLHardDropRes : UnityEngine.ScriptableObject {

        public SerializableDictionary<int, Sprite> resource;

        public Sprite GetSprite(int tileIndex) {
            return resource[tileIndex];
        }
    }
}