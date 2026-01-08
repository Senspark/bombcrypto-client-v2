using System;
using App;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Manager;
using Senspark;
using Services.WebGL;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogReferral : Dialog {
        [SerializeField]
        private TextMeshProUGUI linkText;
        
        [SerializeField]
        private TextMeshProUGUI friendInvitedText;
        
        [SerializeField]
        private TextMeshProUGUI starCoreEarnedText;
        
        [SerializeField]
        private TextMeshProUGUI minimumClaimText;
        
        [SerializeField]
        private RectTransform begin;
        
        [SerializeField]
        private GameObject starCorePrefab;
        
        [SerializeField]
        private Button claimAllButton;
        
        [SerializeField]
        private Button copyButton;
        
        [SerializeField]
        private TextMeshProUGUI copyText;
        
        [SerializeField]
        private GameObject error;

        private ISoundManager _soundManager;
        private ILogManager _logManager;
        private ObserverHandle _handle;
        private Transform _target;
        private Transform _parent;
        private Canvas _canvas;
        private string _referralLink;
        private double _starCoreEarned;
        private int _minClaimReferral;
        private int _timePayOutReferral;
        
        private const string LINK_TEST = "";
        private const string LINK_PROD = "https://game.bombcrypto.io/ton";

        public static UniTask<DialogReferral> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogReferral>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
        }

        private void ResetUI() {
            error.SetActive(false);
            SetCopyBtn(false);
            linkText.SetText(string.Empty);
            copyButton.interactable = false;
            var defaultValue = 0;
            friendInvitedText.SetText($"{defaultValue}");
            _starCoreEarned = defaultValue;
            _minClaimReferral = defaultValue;
            _timePayOutReferral = defaultValue;
            minimumClaimText.SetText($"Minimum claim {_minClaimReferral}");
            UpdateLocalStarCore();
            claimAllButton.interactable = false;
        }

        public void InitData(Transform target, Transform parent, Canvas canvas) {
            ResetUI();
            _target = target;
            _parent = parent;
            _canvas = canvas;
            var waiting = new WaitingUiManager(_canvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
                    var referralData = await serverManager.UserTonManager.GetReferralData();
                    UpdateUI(referralData);
                } catch (Exception e) {
                    DialogOK.ShowError(_canvas, e.Message);
                } finally {
                    waiting.End();
                }
            });
        }

        private void UpdateUI(IReferralData data) {
            copyButton.interactable = true;
            var link = AppConfig.IsProduction ? LINK_PROD : LINK_TEST;
            _referralLink = $"{link}?code=c-{data.referralCode}";
            linkText.SetText(_referralLink);
            friendInvitedText.SetText($"{data.childQuantity}");
            _starCoreEarned = data.rewards;
            _minClaimReferral = data.minClaimReferral;
            _timePayOutReferral = data.timePayOutReferral;
            minimumClaimText.SetText($"Minimum claim {_minClaimReferral}");
            UpdateLocalStarCore();
        }

        private void UpdateLocalStarCore() {
            var truncate = Truncate(_starCoreEarned, 10);
            starCoreEarnedText.SetText(truncate.ToString("0.##########"));
            claimAllButton.interactable = _starCoreEarned >= _minClaimReferral;
        }
        
        private double Truncate(double value, int decimalPlaces) {
            double multiplier = Math.Pow(10, decimalPlaces);
            return Math.Floor(value * multiplier) / multiplier;
        }

        protected override void OnDestroy() {
            if (_handle != null) {
                _handle.Dispose();
            }
            base.OnDestroy();
        }

        public void OnCloseBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        public async void OnInfoBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var dialogReferralInformation = await DialogReferralTask.Create();
            dialogReferralInformation.UpdateUI(_minClaimReferral, _timePayOutReferral);
            dialogReferralInformation.Show(_canvas);
        }

        public void OnClaimAllBtnClicked() {
            _soundManager.PlaySound(Audio.CollectBCoin);
            var waiting = new WaitingUiManager(_canvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
                    await serverManager.UserTonManager.ClaimReferralReward();
                    _starCoreEarned = 0;
                    UpdateLocalStarCore();
                    DoStarCoreAnimation();
                } catch (Exception e) {
                    DialogOK.ShowError(_canvas, e.Message);
                } finally {
                    waiting.End();
                }
            });
        }

        public void OnCopyBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var webGLBridgeUtils = ServiceLocator.Instance.Resolve<IWebGLBridgeUtils>();
            webGLBridgeUtils.CopyToClipboard(_referralLink);
            SetCopyBtn(true);
        }

        public void SetCopyBtn(bool state) {
            copyText.SetText(state ? $"Copied" : $"Copy");
        }
        
        private void DoStarCoreAnimation() {
            var cameraMain = Camera.main;
            if (cameraMain != null) {
                for (int i = 0; i < 20; i++) {
                    // Instantiate the starCorePrefab at a random position near the 'begin' position
                    var position = cameraMain.WorldToScreenPoint(begin.position);
                    var randomX = UnityEngine.Random.value < 0.5f
                        ? UnityEngine.Random.Range(-20f, -10f)
                        : UnityEngine.Random.Range(10f, 20f);
                    var randomY = UnityEngine.Random.value < 0.5f
                        ? UnityEngine.Random.Range(-20f, -10f)
                        : UnityEngine.Random.Range(10f, 20f);
                    Vector3 randomPos = position +
                                        new Vector3(randomX, randomY, 0);
                    GameObject starCore = Instantiate(starCorePrefab, _parent);
                    starCore.transform.position = position;

                    // Move the instantiated starCore to the '_target' position using DoTween
                    starCore.transform.DOMove(randomPos, 0.5f).SetEase(Ease.InOutQuad).SetUpdate(true).OnComplete(
                        () => starCore.transform.DOMove(_target.position, 2f).SetEase(Ease.InOutQuad).SetUpdate(true)
                            .OnComplete(() => { Destroy(starCore); })
                    );
                }
            }
        }

        protected override void OnYesClick() {
            // Do nothing
        }
    }
}