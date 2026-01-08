using System;
using System.Collections.Generic;
using System.Linq;
using App;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.UI;
using Game.UI.Information;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Share.Scripts.Dialog {
    public class DialogInformation : Game.Dialog.Dialog {
        [SerializeField]
        private Text contentText;

        [SerializeField]
        private DialogInformationTab tabPrefab;

        [SerializeField]
        private Transform tabContainer;
        
        [SerializeField]
        private Sprite tabNormalSpr;
        
        [SerializeField]
        private Sprite tabBigSpr;
        
        [SerializeField]
        private Vector2 tabNormalSize;
        
        [SerializeField]
        private Vector2 tabBigSize;

        [SerializeField]
        private ScrollRect scrollRect;
        
        [SerializeField]
        private Slider slider;

        [SerializeField]
        private Image bottomArrow;

        private ISoundManager _soundManager;
        private IInformationManager _informationManager;
        private ILanguageManager _languageManager;
        
        private Tween _arrowTween;
        private List<DialogInformationTab> _tabs;
        private InformationData[] _data;
        private Sequence _sliderSequence;

        public static UniTask<DialogInformation> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogInformation>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _informationManager = ServiceLocator.Instance.Resolve<IInformationManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            
            _arrowTween = bottomArrow.DOFade(0.2f, 1f).SetLoops(-1, LoopType.Yoyo);
            
            _data = _informationManager.GetTokenData();
            InstantiateItems();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _arrowTween?.Kill();
            _sliderSequence?.Kill();
        }

        public DialogInformation OpenTab(BasicInformationTabType tab) {
            var tabName = tab switch {
                //DevHoang: Add new airdrop
                BasicInformationTabType.Rename => "RENAME",
                BasicInformationTabType.ResetRoi => "RSROI",
                BasicInformationTabType.Stake => "STAKE",
                BasicInformationTabType.AutoMine => "AUTOMINE",
                BasicInformationTabType.AutoMineTon => "AUTOMINE_TON",
                BasicInformationTabType.AutoMineSol => "AUTOMINE_SOL",
                BasicInformationTabType.AutoMineRon => "AUTOMINE_RON",
                BasicInformationTabType.AutoMineBas => "AUTOMINE_BAS",
                BasicInformationTabType.AutoMineVic => "AUTOMINE_VIC",
                _ => string.Empty
            };
            return OpenTab(tabName);
        }

        public DialogInformation OpenTab(string tabName) {
            var data = _data.FirstOrDefault(e => e.code.Contains(tabName));
            var tab = data != null ? _tabs.FirstOrDefault(e => e.Data == data) : _tabs[0];
            OnTabButtonClicked(tab);
            return this;
        }

        public void OnTabButtonClicked(DialogInformationTab tab) {
            _soundManager.PlaySound(Audio.Tap);
            slider.normalizedValue = 1;
            DisplayInfo(tab.Data);
            foreach (var t in _tabs) {
                ResetTabButton(t);
            }
            ScaleUpTabButton(tab);
            
            ShowSlider();
        }

        public void OnSliderValueChanged(float _) {
            if (Math.Abs(scrollRect.verticalNormalizedPosition - slider.normalizedValue) > 0.01f) {
                scrollRect.verticalNormalizedPosition = slider.normalizedValue;
                SetArrowVisible();
            }
        }

        public void OnScrollerScroll(float _) {
            if (Math.Abs(slider.normalizedValue - scrollRect.verticalNormalizedPosition) > 0.01f) {
                slider.normalizedValue = scrollRect.verticalNormalizedPosition;
                SetArrowVisible();
            }
        }
        
        private void ShowSlider() {
            slider.gameObject.SetActive(false);
            bottomArrow.gameObject.SetActive(false);

            _sliderSequence?.Kill();
            _sliderSequence = DOTween.Sequence();
            _sliderSequence
                .AppendCallback(() => {
                    var showSlider = contentText.rectTransform.sizeDelta.y > scrollRect.viewport.rect.height;
                    slider.gameObject.SetActive(showSlider);
                    bottomArrow.gameObject.SetActive(showSlider);
                })
                .SetDelay(0.1f);
        }

        private void DisplayInfo(InformationData data) {
            contentText.text = data.content;
        }

        private void InstantiateItems() {
            App.Utils.ClearAllChildren(tabContainer);
            _tabs = new List<DialogInformationTab>();
            foreach (var d in _data) {
                var item = Instantiate(tabPrefab, tabContainer);
                item.Init(d, OnTabButtonClicked);
                ResetTabButton(item);
                _tabs.Add(item);
            }
        }

        private void ResetTabButton(DialogInformationTab tab) {
            tab.SetSize(tabNormalSize, tabNormalSpr);
        }

        private void ScaleUpTabButton(DialogInformationTab tab) {
            tab.SetSize(tabBigSize, tabBigSpr);
        }

        private void SetArrowVisible() {
            var t = slider.normalizedValue;
            bottomArrow.enabled = t > 0.2f;
        }
    }
}