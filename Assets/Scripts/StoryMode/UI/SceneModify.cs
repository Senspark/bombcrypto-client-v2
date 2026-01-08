using UnityEngine;

namespace StoryMode.UI {
    public class SceneModify : MonoBehaviour {
        [SerializeField]
        private RectTransform leftBanner;

        [SerializeField]
        private Transform[] leftContent;

        public void SetSafeArea() {
            var safeArea = (int) Screen.safeArea.size.x;
            var fullArea = Screen.width;
            var ratio = 960.0f / Screen.width;
            var dx = fullArea - safeArea;
            MoveToRight(dx * ratio);
        }
        
        private void MoveToRight(float dx) {
            foreach (var content in leftContent) {
                MoveContentToRight(content, dx);
            }

            var size = leftBanner.sizeDelta;
            size.x += (dx / 2);
            leftBanner.sizeDelta = size;
        }

        private void MoveContentToRight(Transform content, float dx) {
            var position = content.position;
            position.x += dx;
            content.position = position;
        }
    }
}