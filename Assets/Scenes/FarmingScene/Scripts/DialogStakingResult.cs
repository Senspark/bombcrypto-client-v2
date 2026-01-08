using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;

namespace Scenes.FarmingScene.Scripts {
    public class HeroDataSuccess {
        public PlayerData PlayerData;
        public string HeroId;
        public string Level;
        public string CurrentShield;
        public string TotalShield;
    }
    public class DialogStakingResult : Dialog
    {
        [SerializeField]
        private GameObject successPanel, failPanel;
    
        [SerializeField]
        private TMP_Text heroId, level, shield;
    
        [SerializeField]
        private TMP_Text rareText;
    
        [SerializeField]
        private Avatar icon;
    
        [SerializeField]
        private DisplayRarity rarity;
    
        private Action _callbackHide;
    
        public static UniTask<DialogStakingResult> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogStakingResult>();
        }
    
        public void Show(bool isSuccess, HeroDataSuccess heroData, Canvas canvas, Action callbackHide = null) {
            _callbackHide = callbackHide;
        
            if (heroData != null) {
                if (icon) icon.ChangeImage(heroData.PlayerData);
                if (heroId) heroId.text = heroData.HeroId;
                if (level) level.text = "Lv " + heroData.Level;
                if (shield) shield.text = heroData.CurrentShield + "/" + heroData.TotalShield;
                if (rarity) rarity.Show(heroData.PlayerData.rare);
                if (rareText) rareText.text = rareText.text;
            }
        
            successPanel.SetActive(isSuccess);
            failPanel.SetActive(!isSuccess);
            if (isSuccess) {
                var onBoardingManager = ServiceLocator.Instance.Resolve<IOnBoardingManager>();
                onBoardingManager.DispatchEvent(e => e.updateOnBoarding?.Invoke(TutorialStep.DoneStakeHero));
            }
            base.Show(canvas);
        }
    
        public new void Hide() {
            _callbackHide?.Invoke();
            base.Hide();
        }
    }
}