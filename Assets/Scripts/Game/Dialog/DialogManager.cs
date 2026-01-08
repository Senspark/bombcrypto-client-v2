using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Game.Dialog {
    [Service(nameof(IDialogManager))]
    public interface IDialogManager : IService {
        void AddDialog(IDialogControllerSupport dialog);
        void RemoveDialog(IDialogControllerSupport dialog);
        bool IsAnyDialogOpened(IDialogControllerSupport me = null);
    }

    public class DefaultDialogManager : IDialogManager {
        private readonly IDialogManager _dialogManager;

        public DefaultDialogManager(ILogManager logManager) {
            if (AppConfig.IsWebGL()) {
                _dialogManager = new DialogManager(logManager);
            } else {
                _dialogManager = new NullDialogManager();
            }
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public void AddDialog(IDialogControllerSupport dialog) {
            _dialogManager.AddDialog(dialog);
        }

        public void RemoveDialog(IDialogControllerSupport dialog) {
            _dialogManager.RemoveDialog(dialog);
        }

        public bool IsAnyDialogOpened(IDialogControllerSupport me = null) {
            return _dialogManager.IsAnyDialogOpened(me);
        }
    }

    public class DialogManager : IDialogManager {
        private readonly Stack<IDialogControllerSupport> _dialogs = new();
        private readonly List<IDialogControllerSupport> _dialogsAlreadyRemove = new();
        private static DialogManagerObject _objectRunner;
        private readonly ILogManager _logManager;

        public DialogManager(ILogManager logManager) {
            _logManager = logManager;
            var dialogManagerObject = new GameObject("DialogManagerObject");
            if (_objectRunner != null) {
                Object.Destroy(_objectRunner);
            }
            _objectRunner = dialogManagerObject.AddComponent<DialogManagerObject>();
            Object.DontDestroyOnLoad(dialogManagerObject);
        }

        public void AddDialog(IDialogControllerSupport dialog) {
            if (_dialogs.Contains(dialog)) {
                _logManager.Log($"dialog is already added {dialog}");
                return;
            }
            _dialogs.Push(dialog);
            _objectRunner?.SetNewDialog(dialog);
        }

        public void RemoveDialog(IDialogControllerSupport dialog) {
            if (_dialogs.Count > 0 && _dialogs.Peek() == dialog) {
                if(_objectRunner != null)
                    _objectRunner.StartCoroutine(RemoveDialogAtEndOfFrame());
            }
            // Dialog cần remove không phải là dialog đc mở gần nhất, lưu lại để lát remove
            else if (_dialogs.Contains(dialog) && !_dialogsAlreadyRemove.Contains(dialog)) {
                _dialogsAlreadyRemove.Add(dialog);
            } else {
                _logManager.Log($"dialog not existed {dialog}");
            }
        }

        /// <summary>
        /// Dialog đã remove tại thời điểm này nhưng đợi cuối frame mới cập nhật cho chính xác
        /// </summary>
        /// <returns></returns>
        private IEnumerator RemoveDialogAtEndOfFrame() {
            yield return new WaitForEndOfFrame();

            _dialogs.Pop();
            if (_dialogs.Count > 0) {
                IDialogControllerSupport currentDialog = _dialogs.Peek();
                //Tìm dialog tiếp theo còn tồn tại
                while (currentDialog == null || (_dialogsAlreadyRemove.Contains(currentDialog) && _dialogs.Count > 0)) {
                    _dialogs.Pop();
                    if(_dialogsAlreadyRemove.Contains(currentDialog))
                        _dialogsAlreadyRemove.Remove(currentDialog);
                    if (_dialogs.Count == 0) {
                        break;
                    }
                    currentDialog = _dialogs.Peek();
                }
                _objectRunner?.SetNewDialog(currentDialog);
            } else {
                _objectRunner?.SetNewDialog(null);
            }
        }

        public bool IsAnyDialogOpened(IDialogControllerSupport me = null) {
            if (me != null) {
                return _dialogs.Count > 1 || _dialogs.Count == 1 && _dialogs.Peek() != me;
            }
            return _dialogs.Count > 0;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        private class DialogManagerObject : MonoBehaviour {
            private IDialogControllerSupport _currentDialog;

            public void SetNewDialog(IDialogControllerSupport dialog) {
                _currentDialog = dialog;
            }

            private void Update() {
                _currentDialog?.OnUpdate();
            }
        }
    }

    public class NullDialogManager : IDialogManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public void AddDialog(IDialogControllerSupport dialog) {
        }

        public void RemoveDialog(IDialogControllerSupport dialog) {
        }

        public bool IsAnyDialogOpened(IDialogControllerSupport me = null) {
            return false;
        }
    }
}