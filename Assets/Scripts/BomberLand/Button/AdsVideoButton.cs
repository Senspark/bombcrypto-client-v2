using DG.Tweening;

using Senspark;
using Senspark.Ads;

using Services.IapAds;

using UnityEngine;
using UnityEngine.UI;

namespace StickWar.Manager {
    public class AdsVideoButton : MonoBehaviour {
        [SerializeField]
        private Button button;
        
        [SerializeField]
        private GameObject waiting;
        
        [SerializeField]
        private RectTransform waitingRun;

        public bool Enable { set; private get; } = true;

        private IUnityAdsManager _adsManager;
        private RewardedAdButton _rewardedAdButton;

        public bool Interactable {
            get => button.interactable;
            set {
                Enable = value;
                button.interactable = value;
                if (value) {
                    _rewardedAdButton.Refresh();
                } else { 
                    waiting.SetActive(false);
                }
            }
        }
        
        private void Awake() {
            _adsManager = ServiceLocator.Instance.Resolve<IUnityAdsManager>();
            _rewardedAdButton = gameObject.AddComponent<RewardedAdButton>();   
            _rewardedAdButton.Init(_adsManager, OnAdLoaded);
        }

        private void Start() {
            DOTween.defaultTimeScaleIndependent = true;
            waitingRun.DORotate(new Vector3(0, 0, 360), 0.5f).SetRelative(true).SetEase(Ease.Linear).SetLoops(-1);
        }

        protected void OnDestroy() {
            DOTween.Kill(waitingRun);
        }

        private void OnAdLoaded(bool value) {
            UpdateUi(value);
        }

        private void UpdateUi(bool value) {
            if (!Enable) {
                return;
            }
            button.enabled = value;
            waiting.SetActive(!value);
        }
    }
}