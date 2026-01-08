using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Manager;
using Game.UI.Information;

using Senspark;

using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogStaking : Dialog {
        [SerializeField]
        private Text bcoinInChestLbl;

        [SerializeField]
        private Text bcoinInStakeLbl;

        [SerializeField]
        private Text profitLbl;

        [SerializeField]
        private Text apdLbl;

        [SerializeField]
        private Text totalStakeLbl;

        [SerializeField]
        private Text stakeDateLbl;

        private ISoundManager _soundManager;
        private IStorageManager _storeManager;
        private IServerManager _serverManager;
        private IStakeResult _stakeResult;
        private ILanguageManager _languageManager;
        private IChestRewardManager _chestRewardManager;

        public static UniTask<DialogStaking> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogStaking>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        }

        public override void Show(Canvas canvas) {
            base.Show(canvas);
            Refresh();
        }

        public void OnStakeBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            DialogConfirmStake.Create().ContinueWith(dialog => {
                dialog.Init(_stakeResult, Refresh);
                dialog.Show(DialogCanvas);
            });
        }
        
        public void OnUnStakeBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            DialogConfirmUnStake.Create().ContinueWith(dialog => {
                if (_stakeResult != null) {
                    dialog.Init(_stakeResult, Refresh);
                }
                dialog.Show(DialogCanvas);
            });
        }

        public async void OnHelpBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var dialog = await DialogInformation.Create();
            dialog.OpenTab(BasicInformationTabType.Stake);
            dialog.Show(DialogCanvas);
        }

        private void Refresh() {
            const string nullStr = "--";
            bcoinInChestLbl.text = App.Utils.FormatBcoinValue(_chestRewardManager.GetChestReward(BlockRewardType.BCoin));
            bcoinInStakeLbl.text = nullStr;
            profitLbl.text = nullStr;
            apdLbl.text = nullStr;
            var totalStakePrefix = _languageManager.GetValue(LocalizeKey.ui_total_stake);
            var stakeDatePrefix = _languageManager.GetValue(LocalizeKey.ui_stake_date);
            totalStakeLbl.text = $"{totalStakePrefix} {nullStr}";
            stakeDateLbl.text = string.Empty;

            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    _stakeResult = await _serverManager.General.GetStakeInfo();
                    bcoinInStakeLbl.text = App.Utils.FormatBcoinValue(_stakeResult.MyStake);
                    profitLbl.text = App.Utils.FormatBcoinValue(_stakeResult.Profit);
                    apdLbl.text = $"{_stakeResult.DPY * 100:#,0.####}%";
                    totalStakeLbl.text = $"{totalStakePrefix}: {App.Utils.FormatBcoinValue(_stakeResult.TotalStake)}";
                    if (_stakeResult.StakeDate != DateTime.MinValue) {
                        stakeDateLbl.text =
                            $"{stakeDatePrefix}: <color=#8BF294>{_stakeResult.StakeDate.ToShortDateString()}</color>";
                    }
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e.Message);    
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
                    }
                }
                waiting.End();
            });
        }
    }
}