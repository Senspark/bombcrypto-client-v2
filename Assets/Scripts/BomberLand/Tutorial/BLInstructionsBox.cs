using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Tutorial {
    public class BLInstructionsBox : MonoBehaviour {
        public enum PosBox {
            Center = 1,
            Bot = 2,
            Left = 3,
            BotRight = 4,
        }

        [SerializeField]
        private Text textInstructions;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private GameObject npcLeft;

        [SerializeField]
        private GameObject npcRight;

        [SerializeField]
        private GameObject tapToSkip;

        private const int WAIT_ANI_TEXT_MAX = 70;
        private const int WAIT_ANI_TEXT_MIN = 10;
        private int _waitAniText = 0;
        private bool _isSkip;
        public bool IsSkip => _isSkip;
        public GameObject TapToSkip => tapToSkip;

        private void Awake() {
            tapToSkip.SetActive(false);
        }

        public void SetPos(PosBox type) {
            var tr = transform;
            var rectParent = tr.parent.GetComponent<RectTransform>().rect;
            tr.localPosition = type switch {
                PosBox.Center => new Vector3(0, 0, 0),
                PosBox.Bot => new Vector3(0, rectParent.yMin, 0),
                PosBox.Left => new Vector3(-100, 0, 0),
                PosBox.BotRight => new Vector3(0, rectParent.yMin, 0),
                _ => tr.localPosition
            };
            npcRight.SetActive(type == PosBox.BotRight);
            npcLeft.SetActive(!npcRight.activeSelf);
        }

        public void SetContent(string content) {
            textInstructions.text = content;
        }

        public void TryFastAni() {
            _isSkip = true;
            if (_waitAniText <= WAIT_ANI_TEXT_MIN) {
                return;
            }
            _waitAniText = Mathf.Max(_waitAniText - 30, WAIT_ANI_TEXT_MIN);
        }

        public Task<bool> SetContentAni(string content) {
            canvasGroup.alpha = 1;
            var task = new TaskCompletionSource<bool>();
            textInstructions.text = "";
            _isSkip = false;
            UniTask.Void(async () => {
                var idx = 0;
                _waitAniText = WAIT_ANI_TEXT_MAX;
                while (idx < content.Length) {
                    textInstructions.text += content[idx++];
                    await WebGLTaskDelay.Instance.Delay(_waitAniText);
                    _waitAniText = Mathf.Max(_waitAniText - 1, WAIT_ANI_TEXT_MIN);
                }
                task.SetResult(true);
            });
            return task.Task;
        }

        public void Hide(TweenCallback onHide) {
            canvasGroup.DOFade(0.0f, 0.2f).OnComplete(onHide);
        }

        public Task<bool> HideAsync() {
            var task = new TaskCompletionSource<bool>();
            canvasGroup.DOFade(0.0f, 0.3f).OnComplete(() => { task.SetResult(true); });
            return task.Task;
        }

        public Task<bool> SetContentAsync(string content) {
            textInstructions.text = content;
            var task = new TaskCompletionSource<bool>();
            canvasGroup.DOFade(1.0f, 0.3f).OnComplete(() => { task.SetResult(true); });
            return task.Task;
        }
    }
}