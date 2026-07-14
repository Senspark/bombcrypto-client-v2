using Services.Rewards;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class InGameRewardToken : MonoBehaviour {
        [SerializeField]
        private Image img;

        public void Init(TokenData data) {
            if (data == null) {
                Debug.LogError("data is null");
                img.enabled = false;
                return;
            }
            if (!data.icon) {
                Debug.LogError($"Icon not set for {data.tokenName.ToString()} {data.networkSymbol.ToString()}");
                img.enabled = false;
                return;
            }
            Init(data.icon);
        }
        
        public void Init(Sprite icon) {
            if (!icon) {
                Debug.LogError("icon is null");
                img.enabled = false;
                return;
            }
            img.sprite = icon;
            img.sprite.texture.filterMode = FilterMode.Point;
        }
    }
}