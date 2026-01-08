using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.UI.Custom {
    public class FixAspectRatio : MonoBehaviour, ILayoutGroup {
        [SerializeField]
        public bool isUpdatePosition = true;

        public void SetLayoutHorizontal() {
            AutoLayout();
        }

        public void SetLayoutVertical() {
            AutoLayout();
        }

        private void AutoLayout() {
            var size = this.GetComponent<RectTransform>().rect.size;
            if (size.x <= 0 || size.y <= 0) {
                return;
            }
            var tf = transform;
            var rtParent = tf.parent.GetComponent<RectTransform>();
            var parentSize = rtParent.rect.size;
            if (parentSize.x <= 0 || parentSize.y <= 0) {
                return;
            }
            var scale = Mathf.Min(parentSize.x / size.x, parentSize.y / size.y);
            tf.localScale = scale < 1 ? new Vector3(scale, scale, 1) : Vector3.one;
            if (isUpdatePosition) {
                var sizeDelta = rtParent.sizeDelta;
                tf.localPosition = new Vector3(sizeDelta.x * 0.5f, sizeDelta.y * 0.5f, 0);
            }
        }
    }
}