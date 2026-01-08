using BomberLand.Component;

using Engine.Entities;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class TrHeroAvatar : MonoBehaviour {
        [SerializeField]
        private Avatar avatar;

        [SerializeField]
        private Text heroName;
        
        [SerializeField]
        private Avatar highLight;

        [SerializeField]
        private BLHeroStats stats;

        public void UpdateHero(UIHeroData hero) {
            avatar.ChangeImage(hero.HeroType, hero.HeroColor);

            if (heroName != null) {
                heroName.text = hero.HeroName;
            }

            if (highLight != null) {
                highLight.ChangeImage(hero.HeroType, hero.HeroColor);
            }

            if (stats != null) {
                stats.UpdateStats(hero);
            }
        }

        public void ShowHighLight(bool value) {
            if (highLight == null) {
                return;
            }
            highLight.gameObject.SetActive(value);
        }
    }
}