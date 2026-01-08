using System;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Component {
    public class ButtonBomb : MonoBehaviour {
        [SerializeField]
        private UnityEngine.UI.Button button;
            
        [SerializeField]
        private Text quantity;

        public void SetVisible(bool value) {
            gameObject.SetActive(value);
        }
         
        public bool Interactable {
            get => button.interactable;
            set => button.interactable = value;
        }

        public void SetQuantity(int value) {
            if (value > 99) {
                value = 99;
            }
            quantity.text = $"{value}";
        }
    }
}