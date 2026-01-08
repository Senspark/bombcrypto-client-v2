using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogAutoMine : Dialog {
        [SerializeField]
        private Image toggleOnImg;
        
        [SerializeField]
        private bool dontShowAgain = true;

        private ISoundManager _soundManager;
        private const string Key = "DialogAutoMine";

        public static bool CanShow() {
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            if (featureManager.WarningAutoMine) {
                return PlayerPrefs.GetInt(Key, 0) == 0;
            }
            return false;
        }

        public static UniTask<DialogAutoMine> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogAutoMine>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            toggleOnImg.enabled = dontShowAgain;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            PlayerPrefs.SetInt(Key, dontShowAgain ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void ToggleDontShowAgain() {
            _soundManager.PlaySound(Audio.Tap);
            dontShowAgain = !dontShowAgain;
            toggleOnImg.enabled = dontShowAgain;
        }
        
        public void OnCloseBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}