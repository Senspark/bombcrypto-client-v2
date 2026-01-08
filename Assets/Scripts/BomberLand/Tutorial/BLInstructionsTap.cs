using System;
using System.Threading.Tasks;

using Analytics;

using App;

using DG.Tweening;

using Senspark;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Tutorial {
    public class BLInstructionsTap : MonoBehaviour {
        [SerializeField]
        private GameObject welcomeToBomberLand;

        [SerializeField]
        private TMP_Text instructionText;

        [SerializeField]
        private BLInstructionsBox instructionsBox;

        [SerializeField]
        private GameObject pointerHand;

        [SerializeField]
        private GameObject pressHere;

        [SerializeField]
        private Image dimBackground;

        [SerializeField]
        private GameObject boxSystem;

        [SerializeField]
        private Text textSystem;

        [SerializeField]
        private GameObject bgPvp;

        [SerializeField]
        private Transform shortDialog;

        public BLInstructionsBox InstructionsBox => instructionsBox;

        public GameObject PointerHand => pointerHand;
        public GameObject PressHere => pressHere;
        public GameObject BoxSystem => boxSystem;
        public GameObject BgPvp => bgPvp;
        public Transform ShortDialog => shortDialog;

        // private TaskCompletionSource<bool> _waitInstructionsBoxHide;

        private Action _onTap;

        private CanvasGroup _canvasGroup;

        private ISoundManager _soundManager;
        private IAnalytics _analytics;

        private bool _isWelcomeWaiting;

        private Image _imageBlockTouch;

        protected void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
            _imageBlockTouch = GetComponent<Image>();
            _imageBlockTouch.enabled = false;
            _canvasGroup.alpha = 0.0f;
            pointerHand.SetActive(false);
            pressHere.SetActive(false);
            instructionsBox.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);
            boxSystem.SetActive(false);
            bgPvp.SetActive(false);
            _isWelcomeWaiting = true;
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
        }

        private void Update() {
            if (!_isWelcomeWaiting) {
                return;
            }
            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Tab) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Escape)
               ) {
                OnWelcomeTap();
            }
        }

        public void HideWelcome() {
            welcomeToBomberLand.gameObject.SetActive(false);
        }

        public void SetInstruction(string instruction, Vector2? position = null) {
            instructionText.text = instruction;
            if (position != null) {
                instructionText.transform.localPosition = position.Value;
            }
            instructionText.gameObject.SetActive(true);
        }

        public void HideInstruction() {
            instructionText.gameObject.SetActive(false);
        }

        public void SetPosPointerHand(RectTransform rt) {
            pointerHand.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            pointerHand.transform.position = rt.position;
        }

        public void HidePointerHand() {
            pointerHand.SetActive(false);
        }

        public void SetPosPressHere(RectTransform rt) {
            pressHere.SetActive(true);
            pressHere.transform.position = rt.position;
        }

        public void SetPosAndResizePressHere(RectTransform rt, float offsetY = 6) {
            pressHere.SetActive(true);
            var pos = rt.position;
            pressHere.transform.position = new Vector3(pos.x, pos.y, pos.z);
            pressHere.transform.DOLocalMoveY(pressHere.transform.localPosition.y - offsetY, 0.1f);
            var size = rt.GetComponent<RectTransform>().sizeDelta;
            pressHere.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x + 20, size.y + 20);
        }

        public void SetPosPointerHand(RectTransform rt, Vector3 posAdd) {
            pointerHand.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            pointerHand.transform.position = rt.position + posAdd;
        }

        public void SetPosPointerHand(Vector3 pos) {
            pointerHand.SetActive(true);
            pointerHand.transform.position = pos;
        }

        public void SetPosPointerHand(Vector3 pos, Vector3 posAdd) {
            pointerHand.SetActive(true);
            pointerHand.transform.position = pos + posAdd;
        }

        public void SetRotatePointerHand(float rotate) {
            if (rotate is > 0 and < 180) {
                pointerHand.transform.localRotation = Quaternion.Euler(180, 0, rotate);
            } else {
                pointerHand.transform.localRotation = Quaternion.Euler(0, 0, rotate);
            }
        }

        public void SetOpacityDimBackground(float opacity) {
            dimBackground.color =
                new Color(dimBackground.color.r, dimBackground.color.g, dimBackground.color.b, opacity);
        }

        public Task SetOpacityDimBackgroundAsync(float opacity) {
            var task = new TaskCompletionSource<bool>();
            dimBackground.DOFade(opacity, 0.3f).OnComplete(() => { task.SetResult(true); });
            return task.Task;
        }

        public void ShowDimBackground() {
            dimBackground.enabled = true;
        }

        public void HideDimBackground() {
            dimBackground.enabled = false;
        }

        public Task<bool> Show() {
            var task = new TaskCompletionSource<bool>();
            _canvasGroup.DOFade(1.0f, 0.3f).OnComplete(() => { task.SetResult(true); });
            return task.Task;
        }

        public Task WaitNextTap(bool isFadeInstructionsBox = true) {
            var task = new TaskCompletionSource<bool>();
            _onTap = () => {
                _onTap = null;
                _soundManager.PlaySound(Audio.Tap);
                if (isFadeInstructionsBox) {
                    instructionsBox.Hide((() => { task.SetResult(true); }));
                } else {
                    task.SetResult(true);
                }
            };
            return task.Task;
        }

        private void OnWelcomeTap() {
            _isWelcomeWaiting = false;
            _soundManager.PlaySound(Audio.Tap);
        }

        public void OnBtTap() {
            instructionsBox.TryFastAni();
            if (_isWelcomeWaiting) {
                OnWelcomeTap();
                return;
            }
            _onTap?.Invoke();
        }

        public async Task NpcGuide(string content, string subTrack, bool ignoreTouch = false) {
            if (!ignoreTouch) {
                _imageBlockTouch.enabled = true;
            }
            instructionsBox.gameObject.SetActive(true);
            instructionsBox.TapToSkip.SetActive(false);
            await InstructionsBox.SetContentAni(content);
            _analytics.TrackSceneAndSub(SceneType.FTUEConversion, subTrack);
            if (ignoreTouch) {
                // Do nothing
                return;
            }
#if SKIP_WHEN_HAS_TAP
            if (InstructionsBox.IsSkip) {
                await WebGLTaskDelay.Instance.Delay(100);
            } else {
                await WaitNextTap(false);
            }
#else
            instructionsBox.TapToSkip.SetActive(true);
            await WaitNextTap(false);
#endif
            _imageBlockTouch.enabled = false;
            instructionsBox.TapToSkip.SetActive(false);
        }

        public void SetSystemText(string content, Vector2? localPosition = null) {
            textSystem.text = content;
            boxSystem.gameObject.SetActive(true);
            var rt = boxSystem.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(textSystem.preferredWidth + 30, rt.sizeDelta.y);
            if (localPosition != null) {
                boxSystem.transform.localPosition = localPosition.Value;
            }
        }

        public void SetSystemBoxPos(Vector3 pos) {
            boxSystem.transform.position = pos;
        }

        public void HideSystemBox() {
            boxSystem.gameObject.SetActive(false);
        }

        public void ShowBgPvp() {
            bgPvp.SetActive(true);
        }

        public void HideBgPvp() {
            bgPvp.SetActive(false);
        }

        public void ClearShortDialog() {
            var childCount = shortDialog.childCount;
            for (var idx = childCount - 1; idx > 0; idx--) {
                Destroy(shortDialog.GetChild(idx).gameObject);
            }
        }
    }
}