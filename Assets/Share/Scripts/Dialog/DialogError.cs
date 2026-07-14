using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Share.Scripts.Dialog {
    public class DialogError : Game.Dialog.Dialog {
        [SerializeField]
        private Button buttonOk;

        [SerializeField]
        private TextMeshProUGUI buttonOkText;

        [SerializeField]
        private Button buttonDetail;

        private bool _kickToConnectScene;
        private System.Action _onDidHide;
        protected string _message;

        private static async UniTask<DialogError> Create() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogError>();

        }

        public static void ShowError(Canvas canvas, Exception e) {
            Debug.LogException(e);
            ShowErrorMsgOnly(canvas, e.Message);
        }

        public static UniTask<DialogError> ShowErrorDialog(Canvas canvas, Exception e) {
            Debug.LogException(e);
            return ShowErrorMsgOnlyDialog(canvas, e.Message);
        }

        public static UniTask<DialogError> ShowError(Canvas canvas, Exception e, System.Action onDidHide) {
            Debug.LogException(e);
            return ShowErrorMsgOnly(canvas, e.Message, onDidHide);
        }

        public static void ShowErrorAndKickToConnectScene(Canvas canvas, Exception e) {
            Debug.LogException(e);
            ShowErrorMsgOnlyAndKickToConnectScene(canvas, e.Message);
        }

        public static void ShowErrorMsgOnly(Canvas canvas, string message) {
            Debug.LogError(message);
            Create().ContinueWith(dialog => {
                dialog._message = message;
                dialog.Show(canvas);
            });
        }

        public static async UniTask<DialogError> ShowErrorMsgOnlyDialog(Canvas canvas, string message) {
            Debug.LogError(message);
            var dialog = await Create();
            dialog._message = message;
            dialog.Show(canvas);
            return dialog;
        }

        public static async UniTask<DialogError> ShowErrorMsgOnly(Canvas canvas, string message, System.Action onDidHide) {
            var dialog = await ShowErrorMsgOnlyDialog(canvas, message);
            dialog.IgnoreOutsideClick = true;
            dialog._onDidHide = onDidHide;
            return dialog;
        }

        public static async void ShowErrorMsgOnlyAndKickToConnectScene(Canvas canvas, string message) {
            var dialog = await ShowErrorMsgOnlyDialog(canvas, message);
            dialog.IgnoreOutsideClick = true;
            dialog._kickToConnectScene = true;
            dialog._onDidHide = () => App.Utils.KickToConnectScene();
            dialog.SetButtonOkText("RELOAD");
            dialog.UseActionsOnDestroy = true;
        }

        protected override void Awake() {
            base.Awake();
            buttonOk.onClick.AddListener(OnOkButtonClicked);
            buttonDetail.onClick.AddListener(OnDetailButtonClicked);
        }

        public void SetMessage(string message) {
            _message = message;
        }

        private void SetButtonOkText(string text) {
            buttonOkText.text = text;
        }

        private void OnOkButtonClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            if (_onDidHide != null) {
                OnDidHide(_onDidHide);
            }
            Hide();
        }

        private void OnDetailButtonClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            if (_kickToConnectScene) {
                DialogErrorDetails.ShowErrorAndKickToConnectScene(DialogCanvas, _message);
            } else {
                DialogErrorDetails.ShowError(DialogCanvas, _message, _onDidHide).Forget();
            }
            _onDidHide = null;
            Hide();
        }
    }
}
