using System;
using App;
using Cysharp.Threading.Tasks;
using Scenes.FarmingScene.Scripts;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum LockedType {
    HeroLocked,
    HouseLocked
}

namespace Game.Dialog {
    public class DialogLockedTon : Dialog {
        [SerializeField]
        private Text title;
        
        [SerializeField]
        private TextMeshProUGUI description;
        
        private ISoundManager _soundManager;

        private Action _onCloseOtherDialog;
        private LockedType _currentLockedType;
        
        public static async UniTask<DialogLockedTon> Create() {
           return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogLockedTon>();
        }
        
        private void Start() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public void SetLockedType(LockedType lockedType, Action action) {
            _currentLockedType = lockedType;
            switch (_currentLockedType) {
                case LockedType.HeroLocked:
                    title.text = "hero locked";
                    description.SetText("Heroes from Master Season I are being locked \n\n Visit Shop to buy new Heroes");
                    break;
                case LockedType.HouseLocked:
                    title.text = "house locked";
                    description.SetText("Houses from Master Season I are being locked \n\n Visit Shop to buy new Houses");
                    break;
            }
            _onCloseOtherDialog = action;
        }
        
        public async void OnBtnVisitShop() {
            _soundManager.PlaySound(Audio.Tap);
            _onCloseOtherDialog?.Invoke();
            var dialog = await DialogShop.Create();
            dialog.Show(DialogCanvas);
            switch (_currentLockedType) {
                case LockedType.HeroLocked:
                    dialog.OnBuyHeroSClicked();
                    break;
                case LockedType.HouseLocked:
                    dialog.OnBuyHouseClicked();
                    break;
            }
            Hide();
        }
        
        public void OnCloseBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}