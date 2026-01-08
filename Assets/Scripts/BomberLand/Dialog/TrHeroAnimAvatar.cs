using BomberLand.Component;

using UnityEngine;

namespace Game.Dialog {
    public class TrHeroAnimAvatar : MonoBehaviour {
        [SerializeField]
        private PlayerInMenu avatar;

        [SerializeField]
        private PlayerInMenu highLight;

        public void UpdateHero(UIHeroData hero) {
            avatar.ChangeImage(hero.HeroType, hero.HeroColor);
            if (highLight != null) {
                highLight.ChangeImage(hero.HeroType, hero.HeroColor);
                highLight.SetAnimation();
            }
        }
        
        public void ShowHighLight(bool value) {
            if (value) {
                avatar.SetAnimation();
            } else {
                avatar.SetIdle();
            }

            if (highLight == null) {
                return;
            }
            highLight.gameObject.SetActive(value);
            highLight.SetAnimation();
        }
    }
}