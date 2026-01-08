using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Tutorial {
    public class BLDimHighlight : MonoBehaviour {
        [SerializeField]
        private RectTransform root;

        [SerializeField]
        private RectTransform blockTop;

        [SerializeField]
        private RectTransform blockBot;

        [SerializeField]
        private RectTransform blockLeft;

        [SerializeField]
        private RectTransform blockRight;

        [SerializeField]
        private RectTransform rtDim;

        public void SetHoverRect(RectTransform rt) {
            // var rootRect = this.GetComponent<RectTransform>();
            rtDim.anchorMin = rt.anchorMin;
            rtDim.anchorMax = rt.anchorMax;
            rtDim.pivot = rt.pivot;
            rtDim.position = rt.position;
            rtDim.sizeDelta = rt.sizeDelta;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rtDim);
            SetHoverRectBlock();
        }

        [Button]
        public void SetHoverRectBlock() {
            var rootRect = root.rect;
            rtDim.ForceUpdateRectTransforms();
            var hoverRect = GetRectInParentSpace(rtDim);
            blockTop.sizeDelta = new Vector2(blockTop.sizeDelta.x, Mathf.Max(0, rootRect.height - hoverRect.yMax));
            blockBot.sizeDelta = new Vector2(blockBot.sizeDelta.x, Mathf.Max(0, hoverRect.yMin));
            blockLeft.sizeDelta = new Vector2(Mathf.Max(0, hoverRect.xMin), blockLeft.sizeDelta.y);
            blockRight.sizeDelta = new Vector2(Mathf.Max(0, rootRect.width - hoverRect.xMax), blockRight.sizeDelta.y);
        }

        private Rect GetRectInParentSpace(RectTransform rt) {
            var rect = rt.rect;
            var vector2 = rt.offsetMin + Vector2.Scale(rt.pivot, rect.size);
            if ((bool) (Object) transform.parent) {
                var component = transform.parent.GetComponent<RectTransform>();
                if ((bool) (Object) component) {
                    vector2 += Vector2.Scale(rt.anchorMin, component.rect.size);
                }
            }
            rect.x += vector2.x;
            rect.y += vector2.y;
            return rect;
        }
    }
}