using System;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.Dialog
{
    public class DialogPvpWagerDisclaimer : Dialog
    {
        [SerializeField]
        private Toggle understandLoseToggle;

        [SerializeField]
        private Toggle acceptFeeToggle;

        [SerializeField]
        private Button acceptBtn;

        private Action<bool> _acceptCallback;
        private bool _accepted = false;
        private ISoundManager _soundManager;

        public static UniTask<DialogPvpWagerDisclaimer> Create()
        {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogPvpWagerDisclaimer>();
        }

        protected override void Awake()
        {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            base.Awake();
        }

        public void SetAcceptCallback(Action<bool> acceptCallback)
        {
            _acceptCallback = acceptCallback;
            OnDidHide(() => _acceptCallback?.Invoke(_accepted));
            UpdateAcceptButtonState();
        }

        public void OnToggleValueChanged(bool value)
        {
            _soundManager.PlaySound(Audio.Tap);
            UpdateAcceptButtonState();
        }

        private void UpdateAcceptButtonState()
        {
            acceptBtn.interactable = understandLoseToggle.isOn && acceptFeeToggle.isOn;
        }

        public void OnBtnAccept()
        {
            _accepted = true;
            _soundManager.PlaySound(Audio.Tap);
            
            // Save acceptance if needed via Service
            ServiceLocator.Instance.Resolve<IPvpWagerTermsManager>().SetWagerDisclaimerAccepted(true);
            
            Hide();
        }
        
        public void OnBtnCancel()
        {
            _accepted = false;
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}
