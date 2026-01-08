using UnityEngine;

using System.Collections;

using UnityEngine.UI;

namespace Utils {
    public static class GameObjectExtension {
        public static void SetLayer(this GameObject parent, int layer, bool includeChildren = true) {
            parent.layer = layer;
            if (includeChildren) {
                foreach (var trans in parent.transform.GetComponentsInChildren<Transform>(true)) {
                    trans.gameObject.layer = layer;
                }
            }
        }

        public static void RebuildLayout(this MonoBehaviour monoBehaviour) {
            monoBehaviour.StartCoroutine(RebuildLayout(monoBehaviour.transform as RectTransform));
        }

        private static IEnumerator RebuildLayout(RectTransform rectTransform) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}