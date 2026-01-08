using System;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogFusionSkin : Dialog {
        [SerializeField]
        private GameObject[] rarityHighlights;
        
        [SerializeField]
        private Button leftArrow;
    
        [SerializeField]
        private Button rightArrow;
        
        [SerializeField]
        private Text rarityText;
        
        [SerializeField]
        private FusionSkinItem skinItemPrefab;
        
        [SerializeField]
        private Transform content;
        
        [SerializeField]
        private Text comingSoonTxt;
        
        [SerializeField]
        private SkinTypeResource skinTypeResource;
        
        [SerializeField]
        private CanvasGroup dialogInformation;
        
        private HeroRarity _curRarity;
        private List<FusionSkinItem> _listSkinItem = new List<FusionSkinItem>();
        private Action<PlayerData[], HeroRarity> _onSelectSkin;
        private ISoundManager _soundManager;
        
        public static UniTask<DialogFusionSkin> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogFusionSkin>();
        }
        
        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }
        
        public override void Show(Canvas canvas) {
            base.Show(canvas);
            UpdateUI();
        }

        public void SetDefaultRarity(HeroRarity rarity) {
            _curRarity = rarity;
        }
        
        private void UpdateUI() {
            foreach (var highlight in rarityHighlights) {
                highlight.SetActive(false);
            }
            leftArrow.interactable = _curRarity > HeroRarity.Rare;
            rightArrow.interactable = _curRarity < HeroRarity.SuperMystic;
            rarityHighlights[(int)_curRarity].SetActive(true);
            rarityText.text = $"{HeroRarityDisplay.GetRarityData((int)_curRarity).Name} Hero Skin";
            rarityText.color = HeroRarityDisplay.GetRarityData((int)_curRarity).Color;

            var listSkin = skinTypeResource.GetSkinTypeByRarity(_curRarity);
            if (listSkin.Count == 0) {
                content.gameObject.SetActive(false);
                comingSoonTxt.gameObject.SetActive(true);
                return;
            }
            
            content.gameObject.SetActive(true);
            comingSoonTxt.gameObject.SetActive(false);
            foreach (var item in _listSkinItem) {
                item.gameObject.SetActive(false);
            }
            var count = _listSkinItem.Count;
            for (var i = 0; i < listSkin.Count; i++) {
                if (i < count) {
                    _listSkinItem[i].gameObject.SetActive(true);
                    _listSkinItem[i].Init(listSkin[i], _curRarity);
                    _listSkinItem[i].SetSelectSkin(SelectSkin);
                } else {
                    var skinItem = Instantiate(skinItemPrefab, content).GetComponent<FusionSkinItem>();
                    skinItem.Init(listSkin[i], _curRarity);
                    skinItem.SetSelectSkin(SelectSkin);
                    _listSkinItem.Add(skinItem);
                }
            }
        }

        private void SelectSkin(PlayerData[] fusionMaterials) {
            _onSelectSkin?.Invoke(fusionMaterials, _curRarity);
            Hide();
        }

        public void SetSelectSkin(Action<PlayerData[], HeroRarity> onSelectSkin) {
            _onSelectSkin = onSelectSkin;
        }
        
        public void OnBtnLeftArrow() {
            _soundManager.PlaySound(Audio.Tap);
            _curRarity--;
            UpdateUI();
        }

        public void OnBtnRightArrow() {
            _soundManager.PlaySound(Audio.Tap);
            _curRarity++;
            UpdateUI();
        }
    
        public void OnSelectRarityTabItem(int rarity) {
            _soundManager.PlaySound(Audio.Tap);
            _curRarity = (HeroRarity)rarity;
            UpdateUI();
        }

        public void OnButtonClose() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        public void EnableDialogInformation(bool state) {
            if (state) {
                dialogInformation.gameObject.SetActive(true);
                dialogInformation.alpha = 0f;
                DOTween.To(
                    () => dialogInformation.alpha,
                    x => dialogInformation.alpha = x,
                    1f, 0.5f);
            } else {
                DOTween.To(
                    () => dialogInformation.alpha,
                    x => dialogInformation.alpha = x,
                    0f, 0.5f)
                    .OnComplete(() => {
                        dialogInformation.alpha = 0f;
                        dialogInformation.gameObject.SetActive(false);
                    });
            }
        }
    }
}
