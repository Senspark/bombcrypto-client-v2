using System;
using System.Collections.Generic;
using App;
using Senspark;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class FusionItemDisplayPolygon : MonoBehaviour {
        [SerializeField]
        private List<CanvasGroup> rarityGroups;

        //Light Avatar UI
        [SerializeField]
        private List<Sprite> rarityLightAvatarImgs;

        [SerializeField]
        private Image lightRarityAvatarImg;
        
        [SerializeField]
        private Text targetRarityTxt;

        private enum Rarity {
            Common,
            Rare,
            SuperRare,
            Epic,
            Legend,
            SuperLegend,
            Mega,
            SuperMega,
            Mystic,
            SuperMystic
        }

        private ISoundManager _soundManager;

        private int _targetRarityUpgrade;
        private Action<int> _displayTargetUpgradeRarityHero;
        private Action<int> _callBackTargetUpgradeRarity;

        private void Start() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public void Init(int targetRarityUpgrade, Action<int> callBackTargetUpgradeRarity) {
            _targetRarityUpgrade = targetRarityUpgrade;
            _callBackTargetUpgradeRarity = callBackTargetUpgradeRarity;

            const int expected = (int)Rarity.Mystic + 1;
            if (rarityGroups.Count < expected || rarityLightAvatarImgs.Count < expected) {
                Debug.LogWarning(
                    $"[FusionItemDisplayPolygon] DialogFusionPolygon.prefab has {rarityGroups.Count} rarityGroups / " +
                    $"{rarityLightAvatarImgs.Count} rarityLightAvatarImgs; expected {expected} (Common..Mystic, " +
                    "SuperMystic is result-only and not selectable).");
            }

            RefreshUIAfterChooseRarity();
        }

        public void OnArrowLeftBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _targetRarityUpgrade -= 1;
            if (_targetRarityUpgrade < 0) {
                _targetRarityUpgrade = rarityGroups.Count - 1;
            }
            RefreshUIAfterChooseRarity();
            _callBackTargetUpgradeRarity.Invoke(_targetRarityUpgrade);
        }

        public void OnArrowRightBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _targetRarityUpgrade += 1;
            if (_targetRarityUpgrade >= rarityGroups.Count) {
                _targetRarityUpgrade = 0;
            }
            RefreshUIAfterChooseRarity();
            _callBackTargetUpgradeRarity.Invoke(_targetRarityUpgrade);
        }

        public void OnSelectBtnClicked(int rarity) {
            _soundManager.PlaySound(Audio.Tap);
            _targetRarityUpgrade = rarity;
            RefreshUIAfterChooseRarity();
            _callBackTargetUpgradeRarity.Invoke(_targetRarityUpgrade);
        }

        private void RefreshUIAfterChooseRarity() {
            for (var i = 0; i < rarityGroups.Count; i++) {
                rarityGroups[i].alpha = i == _targetRarityUpgrade ? 1f : 0.5f;
            }
            if (_targetRarityUpgrade >= 0 && _targetRarityUpgrade < rarityLightAvatarImgs.Count) {
                lightRarityAvatarImg.sprite = rarityLightAvatarImgs[_targetRarityUpgrade];
            }
            var currentData = HeroRarityDisplay.GetRarityData(_targetRarityUpgrade);
            var nextData = HeroRarityDisplay.GetRarityData(_targetRarityUpgrade + 1);
            var currentHex = ColorUtility.ToHtmlStringRGB(currentData.Color);
            var nextHex = ColorUtility.ToHtmlStringRGB(nextData.Color);
            targetRarityTxt.text =
                $"<color=#{currentHex}>{currentData.Name}</color> > <color=#{nextHex}>{nextData.Name}</color>";
        }
    }
}