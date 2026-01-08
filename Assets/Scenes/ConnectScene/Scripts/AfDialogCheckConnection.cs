using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Scenes.ConnectScene.Scripts {
    public class AfDialogCheckConnection : Dialog {
        [SerializeField]
        private Button btnReconnect;

        private ISoundManager _soundManager;
        private CheckInternetConnectionHelper _checkInternetConnection;
        
        public static UniTask<AfDialogCheckConnection> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<AfDialogCheckConnection>();
        }

        protected override void Awake() {
            base.Awake();

            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            IgnoreOutsideClick = true;
            _checkInternetConnection = new CheckInternetConnectionHelper();
        }

        public void OnBtnReconnectClicked() {
            _soundManager.PlaySound(Audio.Tap);
            btnReconnect.interactable = false;
            UniTask.Void(async () => {
                var result = await _checkInternetConnection.CheckConnection();
                if (result) {
                    Hide();
                } else {
                    btnReconnect.interactable = true;
                }
            });
        }
    }
}