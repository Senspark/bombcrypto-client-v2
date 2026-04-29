using Engine.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BLPvpMode.UI {
    public class BLHeroReadyCom : MonoBehaviour
    {
        [SerializeField]
        public Avatar avatar;

        [SerializeField]
        public BLBoosterUI boosterDisplay;

        [SerializeField]
        public Text addressText;
        
        [SerializeField]
        public Text ranks;

        [SerializeField]
        public Image rankIcons;
        
        [SerializeField]
        public Text readyText;
        
        [SerializeField]
        public ImageAnimation avatarTR;

        [Header("Communication")]
        [SerializeField]
        public GameObject emojiBubble;
        [SerializeField]
        public Image emojiImage;

        private float _emojiHideTime;

        public void ShowEmoji(Sprite emoji) {
            if (emojiBubble == null || emojiImage == null) return;
            emojiBubble.SetActive(true);
            emojiImage.sprite = emoji;
            _emojiHideTime = Time.time + 3f;
        }

        private void Update() {
            if (emojiBubble != null && emojiBubble.activeSelf && Time.time > _emojiHideTime) {
                emojiBubble.SetActive(false);
            }
        }
    }
}
