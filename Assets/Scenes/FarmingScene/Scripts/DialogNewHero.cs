using System;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Senspark;
using Share.Scripts.PrefabsManager;
using TMPro;
using UnityEngine;

namespace Scenes.FarmingScene.Scripts {
    public class DialogNewHero : Dialog {
        [SerializeField]
        private HeroDetailsDisplay newHero;

        [SerializeField]
        private TMP_Text amountLeft;

        [SerializeField]
        private GameObject btnSkip, btnContinue;
        
        private Action _onSkip;

        public static UniTask<DialogNewHero> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogNewHero>();
        }

        public void SetInfo(PlayerData player, Action onSkip = null, int amount = 1) {
            btnSkip.SetActive(amount >= 1);
            btnContinue.SetActive(amount < 1);
            amountLeft.text =  amount.ToString();
            
            _onSkip = onSkip;
            
            newHero.Init(false);
            newHero.SetInfo(player, DialogCanvas);
            var avatar = GetComponent<Avatar>(); 
            if (avatar) {
                avatar.ChangeImage(player);
            }
        }

        public void OnOkBtnClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }

        protected override void OnYesClick() {
            OnOkBtnClicked();
        }

        protected override void OnNoClick() {
            OnBtnSkipClicked();
        }

        public void OnBtnSkipClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            _onSkip?.Invoke();
            _onSkip = null;
            Hide();
        }
    }
}