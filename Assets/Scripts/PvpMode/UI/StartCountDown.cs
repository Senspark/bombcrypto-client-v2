using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.UI {
    public class StartCountDown : MonoBehaviour {
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private Text counter;

        private System.Action _onFinish;
        
        private readonly string[] _countStrings = new string[] {
            "3", "2", "1", "GO!"
        };

        private int _count = 0;

        private void Awake() {
            _count = 0;
            var color = counter.color;
            color.a = 0;
            counter.color = color;
        }

        public void StartCount(System.Action callback) {
            _onFinish = callback;
            DoMoveDown();
        }

        private void Finish() {
            _onFinish?.Invoke();
            var fadeout = canvasGroup.DOFade(0, 1f);
            DOTween.Sequence()
                .Append(fadeout)
                .AppendCallback(() => {
                    gameObject.SetActive(false);
                });
        }
        
        private void DoZoomIn() {
            counter.text = _countStrings[3];
            var delay = 0.2f;
            var zoomOut = counter.transform.DOScale(10, 0);
            var fadeIn = counter.DOFade(1, delay);
            var zoomIn = counter.transform.DOScale(1, delay);
            var fadeOut = counter.DOFade(0, delay);
            DOTween.Sequence()
                .Append(zoomOut)
                .Append(fadeIn)
                .Join(zoomIn)
                .AppendInterval(1)
                .Append(fadeOut)
                .AppendCallback(Finish);
        }
        
        private void DoMoveDown() {
            if (_count > 2) {
                DoZoomIn();
                return;
            }
            counter.text = _countStrings[_count];
            var trans = counter.transform;
            var position = trans.localPosition;
            position.y += 40;
            trans.localPosition = position;

            var delay = 0.2f;
            var fadeIn = counter.DOFade(1, delay);
            var moveDown = counter.transform.DOLocalMoveY(-40, delay).SetRelative(true);
            var fadeOut = counter.DOFade(0, delay);
            DOTween.Sequence()
                .Append(fadeIn)
                .Join(moveDown)
                .AppendInterval(1)
                .Append(fadeOut)
                .AppendCallback(() => {
                    _count += 1;
                    DoMoveDown();
                });
        }
    }
}
