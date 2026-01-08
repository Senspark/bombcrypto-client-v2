using System;
using App;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Senspark;
using Share.Scripts.PrefabsManager;
using TMPro;
using UnityEngine;

namespace Share.Scripts.Dialog {
    public class DialogOK : Game.Dialog.Dialog {
        [SerializeField]
        private TMP_Text titleTxt;

        [SerializeField]
        private TMP_Text descriptionTxt;

        public static UniTask<DialogOK> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogOK>();
        }
        
        public static void ShowInfo(Canvas canvas, string message, Action onDidHide = null) {
            ShowInfo(canvas, "Notification", message, new Optional {OnDidHide = onDidHide});
        }

        public static void ShowInfo(Canvas canvas, LocalizeKey title, LocalizeKey desc, Action onDidHide = null) {
            var languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            ShowInfo(canvas, languageManager.GetValue(title), languageManager.GetValue(desc),
                new Optional { OnDidHide = onDidHide });
        }

        public static void ShowInfo(Canvas canvas, string title, string message, Optional optional = null) {
            _ = Create().ContinueWith(dialog => {
                if (optional?.OnWillHide != null) {
                    dialog.OnDidHide(optional.OnWillHide);
                }
                if (optional?.OnDidHide != null) {
                    dialog.OnDidHide(optional.OnDidHide);
                }
                dialog.SetInfo(title, message);
                dialog.Show(canvas);
            });
        }

        public static async UniTask ShowInfoAsync(Canvas canvas, string message, Optional optional = null) {
            var d = await Create();
            message = message.Substring(0, Mathf.Min(100, message.Length));
            d.SetInfo(optional?.Title ?? "Notification", message);
            if (optional?.OnWillHide != null) {
                d.OnDidHide(optional.OnWillHide);
            }
            if (optional?.OnDidHide != null) {
                d.OnDidHide(optional.OnDidHide);
            }
            d.Show(canvas);
            if (optional?.WaitUntilHidden == true) {
                await d.WaitForHide();
            }
        }
        
        public static async UniTask ShowErrorAsync(Canvas canvas, string message, Optional optional = null) {
            var d = await Create();
            message = message.Substring(0, Mathf.Min(100, message.Length));
            d.SetInfo(optional?.Title ?? "Error", message);
            if (optional?.OnWillHide != null) {
                d.OnDidHide(optional.OnWillHide);
            }
            if (optional?.OnDidHide != null) {
                d.OnDidHide(optional.OnDidHide);
            }
            d.Show(canvas);
            if (optional?.WaitUntilHidden == true) {
                await d.WaitForHide();
            }
        }

        public static void ShowError(
            Canvas canvas,
            string message,
            Action onDidHide = null,
            bool useActionsOnDestroy = false
        ) {
            _ = Create().ContinueWith(dialog => {
                message = message.Substring(0, Mathf.Min(100, message.Length));
                dialog.SetInfo("Error", message);
                dialog.Show(canvas);
                if (onDidHide != null) {
                    dialog.OnDidHide(onDidHide);
                }
                if (useActionsOnDestroy) {
                    dialog.UseActionsOnDestroy = true;
                }
            });
        }

        public static void ShowErrorAndKickToConnectScene(Canvas canvas, string message) {
            ShowError(canvas, message, App.Utils.KickToConnectScene, true);
        }

        public void SetInfo(string title, string desc) {
            titleTxt.text = title;
            descriptionTxt.text = desc;
        }

        public DialogOK SetInfo(LocalizeKey title, LocalizeKey desc) {
            var languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            SetInfo(languageManager.GetValue(title), languageManager.GetValue(desc));
            return this;
        }

        public void OnOkBtnClicked() {
            var soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            soundManager?.PlaySound(Audio.Tap);
            Hide();
        }

        public class Optional {
            [CanBeNull]
            public Action OnDidHide;

            [CanBeNull]
            public Action OnWillHide;
            
            [CanBeNull]
            public string Title = null;

            public bool WaitUntilHidden = false;
        }
    }
}