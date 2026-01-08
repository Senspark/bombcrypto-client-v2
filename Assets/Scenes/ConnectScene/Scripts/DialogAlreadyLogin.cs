using System;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using TMPro;

namespace Game.Dialog {
    public class DialogAlreadyLogin : Dialog {
        [SerializeField]
        private TextMeshProUGUI descriptionTxt;

        private Action _continueCallBack;

        public void Start() {
            var userName = ServiceLocator.Instance.Resolve<IUserAccountManager>().GetRememberedAccount()?.userName;
            userName = UserAccount.TryRemoveSuffixInUserName(userName);
            descriptionTxt.text =
                $"{userName} is already logged-in on another device\n\nDo you want to continue on this device?";
        }

        public static UniTask<DialogAlreadyLogin> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogAlreadyLogin>();
        }

        public void SetContinueCallback(Action callback) {
            _continueCallBack = callback;
        }

        public void OnContinueBtnClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            _continueCallBack?.Invoke();
            Hide();
        }

        public void OnCancelBtnClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            App.Utils.Logout();
            Hide();
        }
    }
}