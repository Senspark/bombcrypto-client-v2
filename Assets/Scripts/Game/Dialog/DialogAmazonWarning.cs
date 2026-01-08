using System;

using App;

using Scenes.FarmingScene.Scripts;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogAmazonWarning : Dialog {
        [SerializeField]
        private Image toggleOnImg;

        private const string KEY = "DialogAmazonWarning";
        private ISoundManager _soundManager;
        private bool _dontShowAgain = true;
        private Action _onHide;

        public static bool CanShow() {
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            if (featureManager.WarningHeroS) {
                return PlayerPrefs.GetInt(KEY, 0) == 0;
            }
            return false;
        }
        
        public static DialogAmazonWarning Create() {
            var prefab = Resources.Load<DialogAmazonWarning>("Prefabs/Dialog/DialogAmazonWarning");
            return Instantiate(prefab);
        }

        public void Init(Action onHide) {
            _onHide = onHide;
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            toggleOnImg.enabled = _dontShowAgain;
            
            OnWillHide(() => {
                PlayerPrefs.SetInt(KEY, _dontShowAgain ? 1 : 0);
                PlayerPrefs.Save();
                _onHide?.Invoke();
            });
        }

        public void ToggleDontShowAgain() {
            _soundManager.PlaySound(Audio.Tap);
            _dontShowAgain = !_dontShowAgain;
            toggleOnImg.enabled = _dontShowAgain;
        }

        public async void OnOpenShopBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var dialog = await DialogShop.Create();
            dialog.Show(DialogCanvas);
            _onHide = null;
            Hide();
        }

        public void OnOpenFusionBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var dialog = DialogFusion.Create();
            dialog.Show(DialogCanvas);
            _onHide = null;
            Hide();
        }
    }
}