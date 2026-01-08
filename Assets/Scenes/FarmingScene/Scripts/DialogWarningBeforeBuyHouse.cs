using System;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts
{
    public class DialogWarningBeforeBuyHouse : Dialog {
        [SerializeField]
        private Toggle understandToggle;

        [SerializeField]
        private Button acceptBtn;

        private Action<bool> _acceptCallback;
        private bool _accepted = false;
        private ISoundManager _soundManager;

        public static UniTask<DialogWarningBeforeBuyHouse> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogWarningBeforeBuyHouse>();
        }

        protected override void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            base.Awake();
        }

        public static bool CanShow() {
            return ServiceLocator.Instance.Resolve<IHouseStorageManager>().GetHouseCount() > 0;
        }

        public static bool CanShowForRent(int slotLandRent) {
            return ServiceLocator.Instance.Resolve<IHouseStorageManager>().GetHouseSlot() + slotLandRent > 15;
        }

        public void SetAcceptCallback(Action<bool> acceptCallback) {
            _acceptCallback = acceptCallback;
            OnDidHide(() => _acceptCallback?.Invoke(_accepted));
            acceptBtn.interactable = false;
        }

        public void OnToggleValueChanged(bool value) {
            _soundManager.PlaySound(Audio.Tap);
            acceptBtn.interactable = value;
        }

        public void OnBtnContinue() {
            _accepted = true;
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}