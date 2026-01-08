using System;
using System.Globalization;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Manager;

using Senspark;

using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogConfirmStake : Dialog {
        [SerializeField]
        private Button chestBtn;
        
        [SerializeField]
        private Button depositBtn;
        
        [SerializeField]
        private Text bcoinInChestLbl;
        
        [SerializeField]
        private InputField bcoinStakeInput;
        
        [SerializeField]
        private Button confirmBtn;
        
        [SerializeField]
        private StakingInput[] stakingInputs;
        
        private Action _refreshCallback;
        private ISoundManager _soundManager;
        private IStorageManager _storeManager;
        private IServerManager _serverManager;
        private IStakeManager _stakeManager;
        private IStakeResult _stakeResult;
        private ILanguageManager _languageManager;
        private IChestRewardManager _chestRewardManager;
        
        private BlockRewardType _selectedType;
        private float _bcoinStaking;
        
        public static UniTask<DialogConfirmStake> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogConfirmStake>();
        }
        
        private bool CanStake(float value) {
            return _stakeManager.CanStake(_stakeResult.MyStake, value);
        }
        
        private void Start() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _stakeManager = ServiceLocator.Instance.Resolve<IStakeManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            SetInfo(true);
            foreach (var stakingInput in stakingInputs) {
                stakingInput.Initialize(CanStake, Stake);
            }
        }
        
        public void Init(IStakeResult stakeResult, Action refreshCallback) {
            _stakeResult = stakeResult;
            _refreshCallback = refreshCallback;
        }
        
        public void OnChestBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            SetInfo(true);
        }
        
        public void OnDepositBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            SetInfo(false);
        }
        
        public void OnStakeAllBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var coin = _selectedType == BlockRewardType.BCoin
                ? _chestRewardManager.GetChestReward(BlockRewardType.BCoin)
                : _chestRewardManager.GetChestReward(BlockRewardType.BCoinDeposited);
            bcoinStakeInput.text = App.Utils.FormatBcoinValue(coin);
        }
        
        public async void Stake(bool max, BlockRewardType rewardType, float value) {
            _soundManager.PlaySound(Audio.Tap);
            var confirmDialog = await DialogConfirm.Create();
            var from = rewardType == BlockRewardType.BCoin
                ? _languageManager.GetValue(LocalizeKey.ui_stake_reward)
                : _languageManager.GetValue(LocalizeKey.ui_deposit);
            var bcoin = App.Utils.FormatBcoinValue(value);
            // bcoinStakeInput.text = bcoin;
            var str = $"Do you want to stake {bcoin} BCoin from {from}?";
            confirmDialog.SetInfo(
                str,
                null,
                null,
                () => BeginStake(max, rewardType, value),
                null
            );
            confirmDialog.Show(DialogCanvas);
        }
        
        private void BeginStake(bool max, BlockRewardType rewardType, float value) {
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    // var max = GetMaxStakeCoin();
                    // var stakeAll = Mathf.Abs(_bcoinStaking - max) <= 0.01f;
                    var result = await _serverManager.General.Stake(value, rewardType, max);
                    DialogOK.ShowInfo(DialogCanvas, "Successfully");
                    _refreshCallback?.Invoke();
                    Hide();
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
        
        public void OnBcoinInputValueChanged(string _) {
            var text = bcoinStakeInput.text.Replace(',', '.');
            float.TryParse(text, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out _bcoinStaking);
            confirmBtn.interactable = _bcoinStaking <= GetMaxStakeCoin() &&
                                      _stakeManager.CanStake(_stakeResult.MyStake, _bcoinStaking);
        }
        
        private void SetInfo(bool isUsingChestCoin) {
            chestBtn.interactable = !isUsingChestCoin;
            depositBtn.interactable = isUsingChestCoin;
            _selectedType = isUsingChestCoin ? BlockRewardType.BCoin : BlockRewardType.BCoinDeposited;
            var str = isUsingChestCoin
                ? $"{_languageManager.GetValue(LocalizeKey.ui_stake_reward)} {App.Utils.FormatBcoinValue(_chestRewardManager.GetChestReward(BlockRewardType.BCoin))}"
                : $"{_languageManager.GetValue(LocalizeKey.ui_deposit)} {App.Utils.FormatBcoinValue(_chestRewardManager.GetChestReward(BlockRewardType.BCoinDeposited))}";
            bcoinInChestLbl.text = str;
            bcoinStakeInput.text = string.Empty;
            confirmBtn.interactable = false;
        }
        
        private float GetMaxStakeCoin() {
            var max = _selectedType == BlockRewardType.BCoin
                ? _chestRewardManager.GetChestReward(BlockRewardType.BCoin)
                : _chestRewardManager.GetChestReward(BlockRewardType.BCoinDeposited);
            return (float)max;
        }
        
        public void HideAll() {
            OnWillHide(() => {
                foreach (Transform child in DialogCanvas.transform) {
                    if (child.gameObject.GetInstanceID() == gameObject.GetInstanceID()) {
                        continue;
                    }
                    Destroy(child.gameObject);
                }
            });
            Hide();
        }
    }
}