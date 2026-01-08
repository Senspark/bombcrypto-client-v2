using System;
using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Dialog {
    public class DialogForge : Dialog {
        [SerializeField]
        private TextMeshProUGUI titleTxt;

        [SerializeField]
        private TextMeshProUGUI descriptionTxt;
        
        [SerializeField]
        private Text buttonTxt;

        private const string DEFAULT_BUTTON_TEXT = "OK";

        private new static UniTask<DialogForge> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogForge>();
        }
        
        protected override void Awake() {
            base.Awake();
            IgnoreOutsideClick = true;
        }
        
        public static void ShowInfo(Canvas canvas, string title, string message, string buttonText = DEFAULT_BUTTON_TEXT, Action onDidHide = null) {
            Create().ContinueWith(dialog => {
                if (onDidHide != null) dialog.OnDidHide(onDidHide);
                dialog.SetInfo(title, message, buttonText);
                dialog.Show(canvas);
            });
        }
        
        public static void ShowInfo(Canvas canvas, string message) {
            Create().ContinueWith(dialog => {
                dialog.SetInfo("Notification", message, DEFAULT_BUTTON_TEXT);
                dialog.Show(canvas);
            });
        }
        
        public static void ShowError(Canvas canvas, string message) {
            Create().ContinueWith(dialog => {
                message = message.Substring(0, Mathf.Min(100, message.Length));
                dialog.SetInfo("Error", message, DEFAULT_BUTTON_TEXT);
                dialog.Show(canvas);
            });
        }

        private void SetInfo(string title, string desc, string buttonText) {
            titleTxt.text = title;
            descriptionTxt.text = desc;
            buttonTxt.text = buttonText;
        }
        
        public DialogForge SetInfo(LocalizeKey title, LocalizeKey desc, LocalizeKey text) {
            var languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            SetInfo(languageManager.GetValue(title), languageManager.GetValue(desc), languageManager.GetValue(text));
            return this;
        }

        public void OnOKBtnClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }
    }
}