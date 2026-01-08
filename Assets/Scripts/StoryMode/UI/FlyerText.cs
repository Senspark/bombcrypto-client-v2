using DG.Tweening;

using TMPro;

using UnityEngine;

namespace StoryMode.UI {
    public class FlyerText : MonoBehaviour {
        [SerializeField]
        private TextMeshPro valueText;

        private Vector3 _position;

        private void Awake() {
            _position = valueText.transform.localPosition;
            valueText.alpha = 0;
            valueText.sortingOrder = 2;
        }

        public void FlyingValueText(int value) {
            valueText.text = $"+{value}";
            valueText.alpha = 1;

            valueText.DOFade(0, 1.0f).SetDelay(0.5f);
            valueText.transform
                .DOLocalMoveY(valueText.transform.localPosition.y + 1f, 2f)
                .OnComplete(
                    () => { valueText.transform.localPosition = _position; });
        }
    }
}