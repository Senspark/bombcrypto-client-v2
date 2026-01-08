using Engine.Components;

using DG.Tweening;

using Game.Dialog.BomberLand.BLGacha;

using UnityEngine;

namespace Engine.Entities {
    public class Item : EntityLocation {
        [SerializeField]
        private SpriteRenderer sprite;

        [SerializeField]
        private BlinkEffect blinkEffect;

        [SerializeField]
        private BLGachaRes resource;

        public bool IsActive { get; set; }

        private ItemType _itemType;

        public ItemType ItemType {
            get => _itemType;
            set {
                _itemType = value;
                SetSprite(value);
            }
        }

        public Vector2Int Location { get; private set; }

        private void Awake() {
            var updater = new Updater()
                .OnBegin(Init);
            AddEntityComponent<Updater>(updater);
        }

        private async void SetSprite(ItemType itemType) {
            sprite.sprite = await resource.GetSpriteByItemType(itemType);
        }

        public void SetLocation(Vector2Int location) {
            Location = new Vector2Int(location.x, location.y);
        }

        private void Init() {
            SetImmortal();
            SetActive(true);
        }

        private void SetImmortal() {
            Immortal = true;
            DOTween.Sequence()
                .AppendInterval(2)
                .AppendCallback(() => { Immortal = false; });
        }

        public void SetActive(bool value) {
            if (blinkEffect == null) {
                return;
            }

            IsActive = value;
            if (!IsActive) {
                blinkEffect.StartBlink(null, -1);
            } else {
                blinkEffect.StopBlink();
            }
        }
    }
}