using App;
using Senspark;
using Game.Dialog;
using UnityEngine;

namespace Game.UI {
    public class ReferralButton : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private LevelScene levelScene;

        private ISoundManager _soundManager;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            gameObject.SetActive(AppConfig.IsTon());
        }

        public async void OnBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (levelScene != null) {
                levelScene.EnableDialogBackground(true);
            }
            var dialogReferral = await DialogReferral.Create();
            dialogReferral.InitData(levelScene.chestIcon, levelScene.parent, canvasDialog);
            dialogReferral.Show(canvasDialog);
            dialogReferral.OnDidHide(() => {
                levelScene.ResetButtonEvents();
                levelScene.EnableDialogBackground(false);
            });
        }
    }
}