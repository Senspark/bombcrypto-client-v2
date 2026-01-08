using System;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Scenes.ConnectScene.Scripts.Connectors;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;

namespace Scenes.ConnectScene.Scripts {
    public class DialogChooseLoginMethod : Dialog {
        [SerializeField]
        private GameObject editorLayout;
        
        [SerializeField]
        private GameObject webLayout;
        
        [SerializeField]
        private GameObject androidLayout;
        
        [SerializeField]
        private GameObject iosLayout;

        private ISoundManager _soundManager;
        private WebConnectController.ConnectMethodType? _chosen;

        public static UniTask<DialogChooseLoginMethod> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogChooseLoginMethod>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();

            var platform = Application.platform;
            var webGlOnly = platform == RuntimePlatform.WebGLPlayer || Application.isEditor;
            var androidOnly = platform == RuntimePlatform.Android;
            var iosOnly = platform == RuntimePlatform.IPhonePlayer;
            
            webLayout.SetActive(webGlOnly);
            androidLayout.SetActive(androidOnly);
            iosLayout.SetActive(iosOnly);
        }

        public DialogChooseLoginMethod StartFlow(Action<WebConnectController.ConnectMethodType> resolve, Action reject) {
            OnDidHide(() => {
                if (_chosen != null) {
                    resolve?.Invoke(_chosen.Value);
                } else {
                    reject?.Invoke();
                }
            });
            return this;
        }

        public void OnGuestBtnClicked() {
            Choose(WebConnectController.ConnectMethodType.Guest);
        }

        public void OnConnectWalletBtnClicked() {
            Choose(WebConnectController.ConnectMethodType.Wallet);
        }

        public void OnFacebookBtnClicked() {
            Choose(WebConnectController.ConnectMethodType.Facebook);
        }

        public void OnAppleBtnClicked() {
            Choose(WebConnectController.ConnectMethodType.Apple);
        }

        public void OnSensparkBtnClicked() {
            Choose(WebConnectController.ConnectMethodType.Senspark);
        }
        public void OnTelegramBtnClicked() {
            Choose(WebConnectController.ConnectMethodType.Telegram);
        }

        private void Choose(WebConnectController.ConnectMethodType type) {
            _soundManager.PlaySound(Audio.Tap);
            _chosen = type;
            Hide();
        }

        protected override void OnYesClick() {
            // Do nothing
        }

        protected override void OnNoClick() {
            // Do nothing
        }
    }
}