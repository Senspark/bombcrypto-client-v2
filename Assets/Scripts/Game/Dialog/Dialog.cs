using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using UnityEngine;

using DG.Tweening;

using Senspark;

using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Game.Dialog {
    public interface IDialogControllerSupport {
        void OnUpdate();
    }
    public class Dialog : MonoBehaviour, IDialogControllerSupport {
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private Image dimBackground;

        private readonly List<Action> _willShowActions = new();
        private readonly List<Action> _didShowActions = new();
        private readonly List<Action> _willHideActions = new();
        private readonly List<Action> _didHideActions = new();

        public float BackgroundAlpha {
            set {
                var color = dimBackground.color;
                color.a = value / 255;
                dimBackground.color = color;
            }
        }

        public Canvas DialogCanvas { get; private set; }

        protected bool IgnoreOutsideClick { get; set; }
        protected bool UseActionsOnDestroy { get; set; }

        private Sequence _showSequence;
        private Sequence _hideSequence;
        private bool _isHiding;
        private TaskCompletionSource<bool> _waitForDialogClosedTask;
        
        private IInputManager _inputManager;
        private IDialogManager _dialogManager;

        protected virtual void Awake() {
            SetupClickOutside();
            _waitForDialogClosedTask = new TaskCompletionSource<bool>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            _dialogManager = ServiceLocator.Instance.Resolve<IDialogManager>();
        }

        protected bool IsHiding() {
            return _isHiding;
        }

        protected virtual void OnDestroy() {
            _dialogManager?.RemoveDialog(this);
            _showSequence.Kill();
            _hideSequence.Kill();
            if (_waitForDialogClosedTask != null) {
                _waitForDialogClosedTask.SetResult(true);
            }
            if (UseActionsOnDestroy) {
                _willHideActions.ForEach(e => e?.Invoke());
                _didHideActions.ForEach(e => e?.Invoke());
            }
        }

        public Dialog OnWillShow(Action action) {
            Assert.IsNotNull(action);
            _willShowActions.Add(action);
            return this;
        }

        public Dialog OnDidShow(Action action) {
            Assert.IsNotNull(action);
            _didShowActions.Add(action);
            return this;
        }

        public Dialog OnWillHide(Action action) {
            Assert.IsNotNull(action);
            _willHideActions.Add(action);
            return this;
        }

        public Dialog OnDidHide(Action action) {
            Assert.IsNotNull(action);
            _didHideActions.Add(action);
            return this;
        }

        public virtual void Show(Canvas canvas) {
            canvasGroup.alpha = 0;
            DialogCanvas = canvas;
            transform.SetParent(canvas.transform, false);
            FadeToShow();
            _dialogManager?.AddDialog(this);
        }
        public void ShowImmediately(Canvas canvas) {
            DialogCanvas = canvas;
            transform.SetParent(canvas.transform, false);
            _willShowActions.ForEach(item => item?.Invoke());
            _willShowActions.Clear();
            _dialogManager?.AddDialog(this);

        }

        /// <summary>
        /// Tạo Dialog mới từ Dialog hiện tại
        /// </summary>
        public void Reload() {
            var parent = transform.parent;
            var canvas = DialogCanvas;
            Hide();
            var newDialog = Instantiate(gameObject, parent);
            if (canvas) {
                newDialog.GetComponent<Dialog>().Show(canvas);
            }
        }

        public async Task WaitForHide() {
            await _waitForDialogClosedTask.Task;
        }

        private void FadeToShow(float delay = 0) {
            _willShowActions.ForEach(item => item?.Invoke());
            _willShowActions.Clear();
            var fade = canvasGroup.DOFade(1, 0.5f).SetEase(Ease.InOutSine);
            var sequence = DOTween.Sequence();
            if (delay > 0) {
                sequence.SetDelay(delay);
            }
            sequence.Append(fade);
            _showSequence = sequence.AppendCallback(() => {
                    _didShowActions.ForEach(item => item?.Invoke());
                    _didShowActions.Clear();
                })
                .SetUpdate(UpdateType.Normal, true);
        }
        
        public virtual void HideImmediately() {
            if (_isHiding) {
                return;
            }
            _isHiding = true;
            _willHideActions.ForEach(item => item?.Invoke());
            _willHideActions.Clear();
            Destroy(gameObject);
        }

        public virtual void Hide() {
            if (_isHiding) {
                return;
            }
            
            _willHideActions.ForEach(item => item?.Invoke());
            _willHideActions.Clear();

            var fade = canvasGroup.DOFade(0, 0.5f).SetEase(Ease.InOutSine);
            _hideSequence = DOTween.Sequence()
                .Append(fade)
                .AppendCallback(() => {
                    _didHideActions?.ForEach(item => item?.Invoke());
                    _didHideActions?.Clear();
                })
                .SetUpdate(UpdateType.Normal, true)
                .OnComplete(() => {
                    _isHiding = true;
                    Destroy(gameObject);
                });
        }

        private void SetupClickOutside() {
            if (!dimBackground) {
                return;
            }

            var trigger = dimBackground.GetComponent<EventTrigger>();
            if (!trigger) {
                trigger = dimBackground.gameObject.AddComponent<EventTrigger>();
            }

            var autoClose = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
            autoClose.callback.AddListener(OnClickedOutside);
            trigger.triggers.Add(autoClose);
        }

        private void OnClickedOutside(BaseEventData _) {
            if (!IgnoreOutsideClick) {
                Hide();
            }
        }
        
        protected virtual void OnYesClick() {
            Hide();
        }

        protected virtual void OnNoClick() {
            Hide();
        }

        protected virtual void ExtraCheck() {}
        public void OnUpdate() {
            if(!AppConfig.IsWebGL())
                return;
            if(_inputManager == null)
                return;

            ExtraCheck();
            
            if (_inputManager.ReadButton(_inputManager.InputConfig.Enter)) {
                OnYesClick();
            }
            if (_inputManager.ReadButton(_inputManager.InputConfig.Back)) {
                OnNoClick();
            }
        }
    }
}