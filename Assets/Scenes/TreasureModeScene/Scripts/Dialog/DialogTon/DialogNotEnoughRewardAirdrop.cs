using System;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogNotEnoughRewardAirdrop : Dialog {
        [SerializeField]
        private Text description;
        
        [SerializeField]
        private AirdropRewardTypeResource airdropRewardRes;

        private ISoundManager _soundManager;
        
        private BlockRewardType _rewardType;

        public static async UniTask<DialogNotEnoughRewardAirdrop> Create(BlockRewardType rewardType) {
            var dialog = await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>()
                .Instantiate<DialogNotEnoughRewardAirdrop>();
            dialog.SetInfo(rewardType);
            return dialog;
        }
        
        private void Start() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        private void SetInfo(BlockRewardType rewardType) {
            _rewardType = rewardType;
            var text = String.Format(description.text, $"{airdropRewardRes.GetAirdropText(_rewardType)} Deposit");
            description.text = text;
        }

        public async void OnBtnDeposit() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            var dialog = await DialogDepositAirdrop.Create(_rewardType);
            dialog.Show(DialogCanvas);
            Hide();
        }
        
        public void OnCloseBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}