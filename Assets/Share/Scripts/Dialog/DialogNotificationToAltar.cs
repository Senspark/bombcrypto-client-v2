using System;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.PrefabsManager;
using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Share.Scripts.Dialog {
    public class DialogNotificationToAltar : Game.Dialog.Dialog {
        private bool _isClicked;

        
        private static UniTask<DialogNotificationToAltar> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogNotificationToAltar>();
        }
        
        public static void ShowOn(Canvas canvas, Action callback = null, Action onHide = null) {
            Create().ContinueWith(dialog => {
                dialog.Show(canvas);
                dialog.OnDidHide(onHide);
            });
        }

        public void OnButtonNoClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }
        
        protected override void OnYesClick() {
            if(_isClicked)
                return;
            _isClicked = false;
            OnButtonVisitAltarClicked();
        }

        public void OnButtonVisitAltarClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            const string sceneName = "AltarScene";
            SceneLoader.LoadSceneAsync(sceneName).Forget();
        }
    }
}