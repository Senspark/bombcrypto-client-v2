using System;
using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Senspark;

using JetBrains.Annotations;

using PvpMode.Component;
using PvpMode.Manager;

using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.UI {
    public class BoosterButton : MonoBehaviour {
        public delegate Task<int> ChooseBoosterDelegate(BoosterType type, bool chosen);

        [SerializeField]
        private int buttonIndex;

        [SerializeField]
        private GameObject tip;

        [SerializeField]
        private Text tipDescription;
        
        [SerializeField]
        private BoosterType boosterType;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Image highLight;

        [SerializeField]
        private Text quantityText;

        [SerializeField]
        private UpdateValueCounter counter;

        [SerializeField]
        private bool isDisableChoose;
        
        [SerializeField]
        private bool isDisable;
        public bool IsDisable => isDisable;
        
        public int Quantity { get; private set; }
        
        public ChooseBoosterDelegate ChooseBooster { get; set; }
        
        private Action<int, bool> _onUpdateChoose;
        private Action<BoosterType, Action> _onNotificationToMarket;
        
        private bool _isChoose;
        private IServerManager _serverManager;
        private bool _isCanClick;
        [CanBeNull]
        private Sequence _fadeSequence = null;
        private CanvasGroup _cgTip = null;
        private bool _isClicked;

        private void Awake() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            // FIXME: not used.
            // ChooseBooster = async (type, chosen) => {
            //     (await _serverManager.Pvp.ChoosePvPBooster((int) type, chosen)).Code;
            _cgTip = tip.GetComponent<CanvasGroup>();
            if (_cgTip == null) {
                _cgTip = tip.AddComponent<CanvasGroup>();
            }
            _cgTip.alpha = 0;
        }

        private void Start() {
            if(AppConfig.IsTournament())
                gameObject.SetActive(false);
            if (tip != null) {
                tip.SetActive(false);
            }
        }
        
        protected void OnDestroy() {
            if (_fadeSequence != null) {
                _fadeSequence.Kill();
            }
        }

        public BoosterType GetBoosterType() {
            return boosterType;
        }

        public void SetTipDescription(string description) {
            tipDescription.text = description;
        }

        public void SetCallback(Action<BoosterType, Action> notiCallback, Action<int, bool> updateCallback) {
            _onNotificationToMarket = notiCallback;
            _onUpdateChoose = updateCallback;
        }
        
        public void SetQuantity(int value) {
            Quantity = value;
            if (counter == null) {
                quantityText.text = $"{value}";
                _isCanClick = value > 0;
                return;
            }
            counter.SetValue(value);
        }

        public void SetQuantityWithCounter(int value) {
            if (counter == null) {
                SetQuantity(value);
                return;
            }
            counter.SetValue(value, true);
        }

        public void SetAddQuantity(int value) {
            quantityText.text = $"+ {value}";
        }

        public void ChangeBoosterType(BoosterType type, Sprite sprite) {
            boosterType = type;
            icon.sprite = sprite;
        }

        public void OnButtonClicked() {
            if (!_isCanClick) {
                _onNotificationToMarket?.Invoke(boosterType, () => {
                    _isClicked = false;
                });
                return;
            }
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            UniTask.Void(async () => {
                if (ChooseBooster == null) {
                    return;
                }
                var result = await ChooseBooster(boosterType, !_isChoose);
                if (result == 0) {
                    _isChoose = !_isChoose;
                    highLight.gameObject.SetActive(_isChoose);
                    _onUpdateChoose?.Invoke(buttonIndex, _isChoose);
                }
                _isClicked = false;
            });
        }

        public void OnForceToFalse() {
            if (_isChoose) {
                UniTask.Void(async () => {
                    var result = await ChooseBooster(boosterType, false);
                    if (result == 0) {
                        _isChoose = false;
                        highLight.gameObject.SetActive(_isChoose);
                        _onUpdateChoose?.Invoke(buttonIndex, _isChoose);
                    }
                });
            }
        }

        public void SetChoose(bool value) {
            if (isDisableChoose) {
                return;
            }
            _isChoose = value;
            highLight.gameObject.SetActive(_isChoose);
        }

        public void ShowTip() {
            tip.SetActive(true);
            if (_fadeSequence != null) {
                _fadeSequence.Kill();
            }
            _fadeSequence = DOTween.Sequence();
            _fadeSequence.Append(_cgTip.DOFade(1.0f, 0.3f));
            _fadeSequence.AppendInterval(3);
            _fadeSequence.Append(_cgTip.DOFade(0.0f, 0.3f));
            _fadeSequence.AppendCallback(() => {
                tip.SetActive(false);
                _fadeSequence = null;
            });
        }

        public void HideTip() {
            _cgTip.alpha = 0;
            tip.SetActive(false);
        }

        public void OnChoose() {
            if(isDisableChoose || isDisable)
                return;
            if(_isClicked)
                return;
            _isClicked = true;
            
            //Key và shield mặc định chọn
            if(boosterType == BoosterType.Key || boosterType == BoosterType.Shield) 
                return;
            OnButtonClicked();
        }
        
        public void OnNavigate(bool isNavigate) {
            if (isNavigate) {
                ShowTip();
            } else {
                HideTip();
            }
        }

        public void OnTouchDown() {
        }
        
        public void OnTouchUp() {
        }
    }
}