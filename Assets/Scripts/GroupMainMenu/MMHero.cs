using Animation;

using Engine.Components;
using Engine.Entities;
using Engine.Utils;

using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;

using UnityEngine;

namespace GroupMainMenu {
    public class MMHero : MonoBehaviour {
        [SerializeField]
        private ImageAnimation icon;

        [SerializeField]
        private ImageAnimation wing;

        [SerializeField]
        private ImageAnimation bomb;

        [SerializeField]
        private GameObject[] shadow;

        [SerializeField]
        private BLGachaRes resource;

        [SerializeField]
        private AnimationResource animationResource;

        
        public void SetInfo(int heroId, int wingId, int bombId) {
            PlayAnimationHero(heroId);
            PlayAnimation(bomb, bombId, DefaultEntity.DefaultBomb);
            if (wingId >= 0) {
                PlayAnimation(wing, wingId);
                wing.gameObject.SetActive(true);
            } else {
                wing.gameObject.SetActive(false);
            }

            foreach (var iter in shadow) {
                iter.SetActive(true);
            }
        }

        public void UpdateHero(int heroId) {
            PlayAnimationHero(heroId);
        }

        public void UpdateWing(int wingId) {
            if (wingId >= 0) {
                PlayAnimation(wing, wingId);
                wing.gameObject.SetActive(true);
            } else {
                wing.gameObject.SetActive(false);
            }
        }

        public void UpdateBomb(int bombId) {
            PlayAnimation(bomb, bombId, DefaultEntity.DefaultBomb);
            bomb.gameObject.SetActive(true);
        }        
        
        private void PlayAnimationHero(int heroId) {
            var playerType = UIHeroData.ConvertFromHeroId(heroId);
            var sprites = animationResource.GetSpriteIdle(playerType, PlayerColor.HeroTr, FaceDirection.Down);
            icon.StartLoop(sprites);
        }

        private async void PlayAnimation(ImageAnimation imageAnimation, int itemId, DefaultEntity defaultEntity = DefaultEntity.Unknown) {
            if (itemId <= 0) {
                imageAnimation.StartLoop(animationResource.GetDefaultSprite(defaultEntity));
                return;
            }
            var resourcePicker = await resource.GetAnimationByItemId(itemId);
            var type = resourcePicker.Type;
            if (type > 0 && resourcePicker.AnimationIdle.Length > 0) {
                imageAnimation.StartLoop(resourcePicker.AnimationIdle);
            } else {
                imageAnimation.StartLoop(animationResource.GetDefaultSprite(defaultEntity));
            }
        }
    }
}