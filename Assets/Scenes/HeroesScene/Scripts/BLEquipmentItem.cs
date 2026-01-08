using System;
using App;
using BomberLand.Inventory;
using Data;
using DG.Tweening;
using Engine.Entities;
using Senspark;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BLEquipmentItem : MonoBehaviour {
        [SerializeField]
        private SkinChestType type;
        public SkinChestType Type => type;
        
        [SerializeField]
        private Image iconType;
        
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Image cover;

        [SerializeField]
        private GameObject tip;

        [SerializeField]
        private Transform stats;

        [SerializeField]
        private Text equipName;
        
        [SerializeField]
        public OverlayTexture effectPremium;

        [SerializeField]
        private BLInventoryStat statPrefab;
        
        private ISoundManager _soundManager;
        private Action<BLEquipmentItem> _onClickCallback;

        
        private Sequence _fadeSequence = null;
        private CanvasGroup _cgTip = null;
        private bool _isDisable;
        private bool _isClicked;
        
        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            if (tip == null) {
                return;
            }
            _cgTip = tip.GetComponent<CanvasGroup>();
            if (_cgTip == null) {
                _cgTip = tip.AddComponent<CanvasGroup>();
            }
            _cgTip.alpha = 0;
        }

        private void Start() {
            if (tip != null) {
                tip.SetActive(false);
            }
        }
        
        protected void OnDestroy() {
            if (_fadeSequence != null) {
                _fadeSequence.Kill();
            }
        }

        public void SetOnClick(Action<BLEquipmentItem> callback) {
            _onClickCallback = callback;
        }

        public void SetImageCover(Sprite s) {
            _isDisable = s == null;
            if (!s) {
                iconType.gameObject.SetActive(true);
                icon.gameObject.SetActive(false);
                cover.gameObject.SetActive(false);
                
            } else {
                iconType.gameObject.SetActive(false);
                icon.gameObject.SetActive(true);
                icon.sprite = s;
                cover.gameObject.SetActive(true);
            }
        }

        public void OnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _onClickCallback?.Invoke(this);
        }

        public void OnChoose() {
            if (_isClicked)
                return;
            _isClicked = true;
            OnClicked();
        }

        public void AfterClose() {
            _isClicked = false;
        }

        public void SetTipInfo(ISkinManager.Skin skin) {
            if (tip == null) {
                return;
            }
            equipName.text = skin.SkinName;
            if (effectPremium) {
                var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
                var itemKind = productItemManager.GetItem(skin.SkinId).ItemKind;
                // effectPremium.enabled = itemKind == ProductItemKind.Premium;
                effectPremium.enabled = false;
                equipName.color = itemKind == ProductItemKind.Premium
                    ? effectPremium.m_OverlayColor : Color.white;
            }
            if (skin.Stats.Length > 0) {
                stats.gameObject.SetActive(true);
                foreach (var stat in skin.Stats) {
                    var statItem = Instantiate(statPrefab, stats, false);
                    statItem.SetInfo(stat.StatId, stat.Value);
                }
            } else {
                stats.gameObject.SetActive(false);
            }
        }

        public void HideTip() {
            _cgTip.alpha = 0;
            tip.SetActive(false);
        }

        public void ShowTip() {
            if (_isDisable) {
                return;
            }
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
    }
}