using System;
using App;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class HouseItem : MonoBehaviour {
        [SerializeField]
        private Image background;

        [SerializeField]
        private Text houseType;

        [SerializeField]
        private Text size;

        [SerializeField]
        private Text charge;

        [SerializeField]
        private Text slot;
        
        [SerializeField, CanBeNull]
        private Text price;

        [SerializeField]
        private Image soldOutImg;
    
        [SerializeField]
        private Color activeColor = new(0.32f, 0.32f, 0.32f, 1f);
    
        [SerializeField]
        private Color unActiveColor  = new(1f, 1f, 1f, 1f);

        public HouseData Data { get; private set; }

        private Action<int> _onItemClicked;
        private int _myIndex;
        private ISoundManager _soundManager;

        public void SetInfo(int index, HouseData data, Action<int> onItemClicked, ISoundManager soundManager) {
            _myIndex = index;
            Data = data;
            _onItemClicked = onItemClicked;
            _soundManager = soundManager;

            houseType.text = DefaultHouseStoreManager.GetHouseName(data.HouseType);
            size.text = DefaultHouseStoreManager.GetSizeString(data.Size);
            charge.text = $"{data.Charge:0.##}/m";
            slot.text = "" + data.Slot;
            if (price != null) {
                price.text = $"{data.Price:0.##}";
            }

            var isSoldOut = data.Supply == 0 && !AppConfig.IsAirDrop();
            if (price != null) {
                price.gameObject.SetActive(!isSoldOut);
            }
            soldOutImg.gameObject.SetActive(isSoldOut);
        }

        public void SetActive(bool value) {
            background.color = value ? activeColor : unActiveColor;
        }

        public void OnItemClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _onItemClicked?.Invoke(_myIndex);
        }
    }
}