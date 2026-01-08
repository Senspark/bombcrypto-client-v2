using System;
using System.Collections.Generic;

using Services;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.Dialog {
    public class Equipment : MonoBehaviour {
        [FormerlySerializedAs("_button")]
        [SerializeField]
        private Button button;
        
        [FormerlySerializedAs("_equippedSignal")]
        [SerializeField]
        private GameObject equippedSignal;

        [FormerlySerializedAs("_image")]
        [SerializeField]
        private Image image;

        private IEquipmentManager.Equipment _equipment;
        
        private readonly Dictionary<int, string> _prefixes = new() {
            [1] = "SkinChest",
            [2] = "SkinChest",
            [3] = "SkinChest"
        };

        public void Initialize(IEquipmentManager.Equipment equipment, bool equipped) {
            var prefix = _prefixes.TryGetValue(equipment.ItemType, out var value) ? value : "SkinChest";
            image.sprite = Resources.Load<Sprite>($"{prefix}/{equipment.ItemId}");
            _equipment = equipment;
            UpdateEquipped(equipped);
        }

        public void AddClickedHandler(Action<IEquipmentManager.Equipment> action) {
            button.onClick.AddListener(() => action(_equipment));
        }

        public void UpdateEquipped(bool equipped) {
            equippedSignal.SetActive(equipped);
        }
    }
}