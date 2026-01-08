using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.PvpModeScene.Scripts {
    public class BLDialogPvPReward : Dialog {
        [SerializeField]
        private Text button;
        
        [SerializeField]
        private Text prize;

        [SerializeField]
        private Text rank;

        [SerializeField]
        private Text title;
        
        [SerializeField]
        private Button buttonClaim;

        private int _prize;
        private IServerManager _serverManager;

        public event Action Claimed;

        public static UniTask<BLDialogPvPReward> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogPvPReward>();
        }
        
        protected override void Awake() {
            base.Awake();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        }

        public void Initialize(int p, int r) {
            _prize = p;
            button.text = p > 0 ? "CLAIM" : "OK";
            prize.text = $"+{p}";
            rank.text = $"#{r}";
            title.text = p > 0 ? "CONGRATULATIONS" : "SEASON SUMMARY";
        }

        protected override void OnYesClick() {
            if (buttonClaim.IsInteractable()) {
                OnButtonClaimClicked();
            }
        }

        public void OnButtonClaimClicked() {
            buttonClaim.interactable = false;
            UniTask.Void(async () => {
                try {
                    await _serverManager.Pvp.ClaimPvpReward();
                    await _serverManager.General.GetChestReward();
                    Claimed?.Invoke();
                } catch (Exception e) {
                    Debug.LogWarning(e.Message);
                    DialogOK.ShowInfo(DialogCanvas, "You already claimed your reward");
                }
                Hide();
            });
        }
    }
}