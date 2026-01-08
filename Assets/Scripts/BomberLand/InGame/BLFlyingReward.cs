using System;

using BLPvpMode.Engine;
using BLPvpMode.Engine.Entity;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.InGame {
    public class BLFlyingReward : MonoBehaviour {
        [SerializeField]
        private SerializableDictionaryEnumKey<HeroItem, ResourcePicker> resourceReward;

        [SerializeField]
        private Image icon;

        [Serializable]
        private class ResourcePicker{
            public Sprite sprite;
        }
        
        public void ChangeImage(Sprite sprite) {
            icon.sprite = sprite;
        }

        public Sprite GetSprite(HeroItem item) {
            return resourceReward.ContainsKey(item) ? resourceReward[item].sprite : null;
        }
    }
}