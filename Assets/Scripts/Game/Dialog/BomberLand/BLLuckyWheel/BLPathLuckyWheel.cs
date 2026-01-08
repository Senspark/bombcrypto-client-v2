using System;

using Scenes.StoryModeScene.Scripts;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLLuckyWheel {
    public class BLPathLuckyWheel : MonoBehaviour
    {
        [SerializeField]
        private TypeBLLuckyWheel currentTabSelect;
        
        [SerializeField]
        private Image icon;
        
        [SerializeField]
        private Text quantity;
        
        [SerializeField]
        private Text textNoThing;
        
        private string _code;
        
        public TypeBLLuckyWheel CurrentTabSelect => currentTabSelect;

        public void Awake() {
            icon.gameObject.SetActive(false);
            textNoThing.gameObject.SetActive(false);
            quantity.gameObject.SetActive(false);
        }

        public void ForceSetUnknown() {
            icon.gameObject.SetActive(false);
            textNoThing.gameObject.SetActive(false);
            quantity.gameObject.SetActive(true);
            quantity.text = "";
        }

        public void SetIcon(Sprite sprite) {
            icon.gameObject.SetActive(true);
            quantity.gameObject.SetActive(true);
            textNoThing.gameObject.SetActive(false);
            icon.sprite = sprite;
        }

        public void SetNothing() {
            icon.gameObject.SetActive(false);
            quantity.gameObject.SetActive(false);
            textNoThing.gameObject.SetActive(true);
        }

        public void SetQuantity(int num) {
            quantity.text = $"x{num}";
        }
    }
}
