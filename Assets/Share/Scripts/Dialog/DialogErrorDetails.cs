using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Share.Scripts.Dialog {
    public class DialogErrorDetails : Game.Dialog.Dialog {
        [SerializeField]
        private Button buttonOk;

        [SerializeField]
        private TextMeshProUGUI buttonOkText;
        
        [SerializeField]
        private Text descriptionTxt;

        protected System.Action _onDidHide;
        
        protected override void Awake() {
            base.Awake();
            buttonOk.onClick.AddListener(OnOkButtonClicked);
        }

        public static UniTask<DialogErrorDetails> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogErrorDetails>();

        }

        public static async UniTask<DialogErrorDetails> ShowError(Canvas canvas, string message, string buttonText = "OK") {
            var dialog = await Create();
            message = message.Substring(0, Mathf.Min(100, message.Length));
            dialog.SetInfo(message, buttonText);
            dialog.Show(canvas);
            return dialog;
        }
        
        public static async UniTask<DialogErrorDetails> ShowError(Canvas canvas, string message, System.Action onDidHide) {
            var dialog = await ShowError(canvas, message);
            dialog.IgnoreOutsideClick = true;
            dialog._onDidHide = onDidHide;
            return dialog;
        }
        
        public static async void ShowErrorAndKickToConnectScene(Canvas canvas, string message) {
            var dialog = await ShowError(canvas, message, "RELOAD");
            dialog.IgnoreOutsideClick = true;
            dialog._onDidHide = () => App.Utils.KickToConnectScene(); 
            dialog.UseActionsOnDestroy = true;
        }
        
        protected void SetInfo(string desc, string buttonText) {
            descriptionTxt.text = desc;
            buttonOkText.text = buttonText;
        }

        private void OnOkButtonClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            if (_onDidHide != null) {
                OnDidHide(_onDidHide);
            }
            Hide();
        }
    }
}