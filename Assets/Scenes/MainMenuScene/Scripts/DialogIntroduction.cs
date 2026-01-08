using App;

using Cysharp.Threading.Tasks;

using Scenes.MainMenuScene.Scripts;

using Senspark;

using Share.Scripts.PrefabsManager;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogIntroduction : Dialog {
        private const string LINK_URL = "https://bombcrypto.io/";
        private const string VIDEO_URL = "https://www.youtube.com/watch?v=pq6oNe_tDzw";
        
        protected override void Awake() {
            base.Awake();
            IgnoreOutsideClick = true;
        }

        private static UniTask<DialogIntroduction> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogIntroduction>();
        }
        
        public static void ShowInfo(Canvas canvas) {
            Create().ContinueWith(dialog => {
                dialog.Show(canvas);
            });
        }

        public void OnCloseBtnClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }
        
        public void OnWatchVideoBtnClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Application.OpenURL(VIDEO_URL);
            Hide();
        }
        
        public void OnLinkBtnClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Application.OpenURL(LINK_URL);
            Hide();
        }
    }
}