using System;
using App;
using Game.Dialog;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace Scenes.MainMenuScene.Scripts
{
    public class BLDialogPvpDisclaimer : Dialog
    {
        [SerializeField] private Button acceptBtn;
        [SerializeField] private Button declineBtn;
        [SerializeField] private Toggle dontShowAgainToggle;

        private Action _onAccept;
        private Action _onDecline;

        public static async UniTask<BLDialogPvpDisclaimer> Create()
        {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogPvpDisclaimer>();
        }

        protected override void Awake()
        {
            base.Awake();
            acceptBtn.onClick.AddListener(OnAcceptClick);
            declineBtn.onClick.AddListener(OnDeclineClick);
        }

        public void Initialize(Action onAccept, Action onDecline = null, Action onSkip = null)
        {
            _onAccept = onAccept;
            _onDecline = onDecline;
            
            bool skip = PlayerPrefs.GetInt("PvpWagerDisclaimerSkip", 0) == 1;
            if (skip)
            {
                onSkip?.Invoke();
                // We still show the dialog if specifically called, but the caller can decide to skip based on this.
                // Or we can just call onAccept and hide immediately if skip is true.
            }
        }

        private void OnAcceptClick()
        {
            if (dontShowAgainToggle != null && dontShowAgainToggle.isOn)
            {
                PlayerPrefs.SetInt("PvpWagerDisclaimerSkip", 1);
                PlayerPrefs.Save();
            }
            _onAccept?.Invoke();
            Hide();
        }

        private void OnDeclineClick()
        {
            _onDecline?.Invoke();
            Hide();
        }

        protected override void OnYesClick() => OnAcceptClick();
        protected override void OnNoClick() => OnDeclineClick();
    }
}
