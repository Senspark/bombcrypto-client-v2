using System;
using System.Collections.Generic;
using System.Linq;

using Scenes.StoryModeScene.Scripts;

using UnityEngine;

namespace Game.Dialog.BomberLand.BLLuckyWheel {
    [CreateAssetMenu(fileName = "BLLuckyWheelRes", menuName = "BomberLand/LuckyWheelRes")]
    public class BLLuckyWheelResource : ScriptableObject
    {
        [Serializable]
        public class SkinPicker {
            public string code;
            public Sprite sprite;
        }
        [SerializeField]
        private SerializableDictionaryEnumKey<TypeBLLuckyWheel, SkinPicker> resourceSkin;

        // private Dictionary<string, SkinPicker> dicSkinPicker = null;

        public SkinPicker GetPicker(TypeBLLuckyWheel type) {
            if (!resourceSkin.ContainsKey(type)) {
                return null;
            }
            return resourceSkin[type];
        }
        public SkinPicker GetPickerByCode(string code) {
            foreach (var it in resourceSkin) {
                if (it.Value.code == code) {
                    return it.Value;
                }
            }
            return null;
        }
        public Sprite GetSprite(TypeBLLuckyWheel type) {
            if (!resourceSkin.ContainsKey(type)) {
                return null;
            }
            return resourceSkin[type].sprite;
        }
    }
}
