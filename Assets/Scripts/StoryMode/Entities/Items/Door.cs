using Engine.Components;

using UnityEngine;

using DG.Tweening;

namespace Engine.Entities {
    public class Door : Item {
        [SerializeField]
        private GameObject effectLight;

        private DoorAnimation _doorAnimation;

        private bool Activated { get; set; }

        private void Awake() {
            var updater = new Updater()
                .OnBegin(Init);
            AddEntityComponent<Updater>(updater);
        }

        private void Init() {
            effectLight.SetActive(false);
            _doorAnimation = GetComponent<DoorAnimation>();
            SetDoorActive(false);
            SetImmortal();
        }

        private void SetImmortal() {
            Immortal = true;
            DOTween.Sequence()
                .AppendInterval(2)
                .AppendCallback(() => { Immortal = false; });
        }

        public Vector2Int GetLocation() {
            return EntityManager.MapManager.GetTileLocation(transform.localPosition);
        }

        public void SetDoorActive(bool value) {
            if (Activated == value) {
                return;
            }

            Activated = value;
            if (Activated) {
                _doorAnimation.AnimateBlink();
            } else {
                _doorAnimation.StopAnimate();
            }
        }

        public void PlayerEnter() {
            if (!Activated) {
                return;
            }
            effectLight.SetActive(true);
            EntityManager.PlayerManager.RequestEnterDoor(0, this);
        }

        public void DenyPlayerEnter() {
            effectLight.SetActive(false);
        }
    }
}