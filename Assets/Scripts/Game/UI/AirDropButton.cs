using App;

using DG.Tweening;

using Game.Dialog;

using Senspark;

using UnityEngine;

namespace Game.UI {
    public class AirDropButton : MonoBehaviour {
        [SerializeField]
        private Canvas dialogCanvas;
    
        private ISoundManager _soundManager;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            var featureManager =ServiceLocator.Instance.Resolve<IFeatureManager>();
            gameObject.SetActive(featureManager.EnableClaim);
            if (featureManager.EnableClaim) {
                transform.DOScale(1.2f, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);    
            }
        }

        private void OnDestroy() {
            transform.DOKill();
        }

        public void OnBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var dialog = DialogAirDrop.Create();
            dialog.Show(dialogCanvas);
        }
    }
}