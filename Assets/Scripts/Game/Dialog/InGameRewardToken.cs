using App;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class InGameRewardToken : MonoBehaviour {
        [SerializeField]
        private Image img;

        public void Init(Sprite icon) {
            img.sprite = icon;
            img.sprite.texture.filterMode = FilterMode.Point;
        }
    }
}