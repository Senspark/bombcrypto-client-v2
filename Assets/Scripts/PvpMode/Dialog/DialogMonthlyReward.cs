using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Manager;

using PvpMode.Manager;
using PvpMode.Services;

using Senspark;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.Dialogs {
    public class DialogMonthlyReward : Dialog {
        [SerializeField]
        private GameObject itemPrefab;

        [SerializeField]
        private Transform itemContain;

        [SerializeField]
        private RankRewardItem currentUser;

        [SerializeField]
        private Text rank;

        [SerializeField]
        private Text heroText;

        [SerializeField]
        private Text coinText;

        [SerializeField]
        private Text shieldText;

        [SerializeField]
        private Button buttonClaim;

        [SerializeField]
        private Text claimText;

        [SerializeField]
        private Text matchText;

        [SerializeField]
        private Text playMatchText;
        
        private System.Action<IPvpCurrentRewardResult> _onClaimRewardClose;
        private IServerManager _serverManager;
        private ILanguageManager _languageManager;
        private IStorageManager _storeManager;
        private IAccountManager _accountManager;

        public static DialogMonthlyReward Create() {
            var prefab = Resources.Load<DialogMonthlyReward>("Prefabs/PvpMode/Dialog/DialogMonthlyReward");
            return Instantiate(prefab);
        }

        protected override void Awake() {
            base.Awake();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _accountManager = ServiceLocator.Instance.Resolve<IAccountManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
        }

        private string GetUserName() {
            var userName = _storeManager.NickName;
            if (string.IsNullOrWhiteSpace(userName) || userName == _accountManager.Account) {
                userName = App.Utils.FormatWalletId(_accountManager.Account);
            }
            return userName;
        }        
        
        private void SetCurrentReward(IPvpCurrentRewardResult item) {
            currentUser.SetCurrentInfo(item.Rank, GetUserName(), item.BCoin, item.HeroBox, item.Shield);
            
            if (item.TotalMatch < item.PvpMatchReward) {
                claimText.gameObject.SetActive(false);
                matchText.gameObject.SetActive(true);
                matchText.text = $"{item.TotalMatch}/{item.PvpMatchReward}";
                playMatchText.gameObject.SetActive(true);
                playMatchText.text = string.Format(string.Format(_languageManager.GetValue(LocalizeKey.ui_play_matches),
                    item.PvpMatchReward));
            } else {
                claimText.gameObject.SetActive(true);
                matchText.gameObject.SetActive(false);
                playMatchText.gameObject.SetActive(false);
            }
            buttonClaim.interactable = item.IsReward && !item.IsClaim && (item.TotalMatch >= item.PvpMatchReward);
            
            _onClaimRewardClose?.Invoke(item);

            if (item.Rank <= 0) {
                rank.text = "--";
                return;
            }
            rank.text = "" + item.Rank;

            if (item.IsClaim) {
                coinText.text = "--";
                heroText.text = "--";
                shieldText.text = "--";
                return;
            }
            coinText.text = "" + (int) item.BCoin;
            heroText.text = "" + item.HeroBox;
            shieldText.text = "" + item.Shield;
        }

        private void SetGiftListInfo(IPvpRewardResult[] list) {
            foreach (var reward in list) {
                var obj = Instantiate(itemPrefab, itemContain, false);
                var item = obj.GetComponent<RankRewardItem>();
                item.SetInfo( reward.MinRank, reward.MaxRank,  reward.BCoin, reward.HeroBox, reward.Shield);
            }
        }

        public void OnClaimButtonClicked() {
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var result = await _serverManager.Pvp.ClaimPvpReward();
                    var current = result.CurrentReward;
                    SetCurrentReward(current);

                    // Update Deposited coins
                    await _serverManager.General.GetChestReward();

                    // Update User Booster
                    var boosterManager = ServiceLocator.Instance.Resolve<IBoosterManager>();
                    var boosterResult = await _serverManager.Pvp.GetUserBooster();
                    boosterManager.SetUserBoosters(boosterResult.Boosters);

                    waiting.End();
                    ShowRewardReceive(current);
                } catch (Exception e) {
                    waiting.End();
                    Debug.LogWarning(e.Message);
                    DialogOK.ShowInfo(DialogCanvas, "You already claimed your reward");
                }
            });
        }

        private void ShowRewardReceive(IPvpCurrentRewardResult reward) {
            var dialog = DialogRewardReceive.Create();
            dialog.SetInfo(reward);
            dialog.Show(DialogCanvas);
        }

        public void OnCloseButtonClicked() {
            Hide();
        }
    }
}