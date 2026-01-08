using Game.Dialog.BomberLand.BLGacha;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Tutorial {
    public class TutorialEquipmentItem : MonoBehaviour {
        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI expiredTimeText;

        [SerializeField]
        private BLGachaRes resource;

        public async void SetInfo(TutorialEquipment equipment) {
            titleText.text = equipment.Title;
            expiredTimeText.text = equipment.Footer;
            icon.sprite = await resource.GetSpriteByItemId(equipment.ItemId);
        }
    }
}