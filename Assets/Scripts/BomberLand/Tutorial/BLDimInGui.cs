using DG.Tweening;

using UnityEngine;

namespace BomberLand.Tutorial {
    public class BLDimInGui : MonoBehaviour {
        public GameObject content;

        public void SetSize(int size) {
            var scale = size / 100;
            var scaleTo = new Vector3(scale, scale, 1);
            content.transform.localScale = scaleTo * 2;
            content.transform.DOScale(scaleTo, 0.5f);
        }

        public void SetSize(float w, float h) {
            // content.transform.localScale = new Vector3(w / 100, h / 100, 1);
            var scaleTo = new Vector3(w / 100, h / 100, 1);
            content.transform.localScale = scaleTo * 2;
            content.transform.DOScale(scaleTo, 0.5f);
        }
    }
}