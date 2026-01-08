using UnityEngine;

namespace Engine.UI {
    public class HealthBar : MonoBehaviour {
        [SerializeField]
        private SpriteRenderer background;

        [SerializeField]
        private SpriteRenderer bar;

        private float _progress;

        private bool _showWhenFull;
        private bool _showHealthBar;

        private void Awake() {
            _progress = 1;
            _showHealthBar = true;
            SetVisible(_showWhenFull);
        }

        public void SetColor(Color color) {
            bar.GetComponent<SpriteRenderer>().color = color;
        }

        public void SetShowHealthBar(bool value) {
            _showHealthBar = value;
        }

        public void SetShowWhenFull(bool value) {
            _showWhenFull = value;
            SetVisible(value);
        }

        public float Progress {
            get => _progress;
            set {
                if (!Mathf.Approximately(_progress, value)) {
                    _progress = value;
                    UpdateProgress();

                    if (_showHealthBar) {
                        if (!_showWhenFull) {
                            SetVisible(_progress < 1);
                        }
                    }
                }
            }
        }

        private void UpdateProgress() {
            var barWidth = 0.8f * _progress;
            bar.size = new Vector2(barWidth, bar.size.y);

            var barTransform = bar.transform;
            var position = barTransform.localPosition;
            position.x = (barWidth / 2) - 0.4f;
            barTransform.localPosition = position;
        }

        private void SetVisible(bool visible) {
            background.enabled = bar.enabled = visible;
        }
    }
}