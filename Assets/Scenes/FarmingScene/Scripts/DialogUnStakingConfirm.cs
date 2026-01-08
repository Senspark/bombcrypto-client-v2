using System.Threading.Tasks;

using App;

using Constant;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogUnStakingConfirm : Dialog
    {
        [SerializeField]
        private TMP_Text
            stakedText,
            unStakeText,
            feeText,
            totalUnstake,
            stakeRemain,
            minValue,
            proccessing;
    

        [SerializeField]
        private Button buttonUnStake;
    
        [SerializeField]
        private GameObject blockPanel;

        [SerializeField]
        private GameObject infoText;

        private IBlockchainManager _blockchainManager;
        private IPlayerStorageManager _playerStorageManager;
        private IServerManager _serverManager;
        private ISoundManager _soundManager;
        private INetworkConfig _networkConfig;
        private DataUnStake _dataUnStake;
        private PlayerData _playerData;
        private IInputManager _inputManager;
    
        private RewardType _tokenType;
        private StakeCallback.Callback _callback;

        private double _amountWantUnStake;
        private int _heroId;
        private bool _isInit;
        private bool _isClicked;

    
        public static UniTask<DialogUnStakingConfirm> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogUnStakingConfirm>();
        }

        protected override void Awake() {
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            base.Awake();
        }

        public void Show(DataUnStake data, PlayerData player, Canvas canvas, StakeCallback.Callback callback = null) {
            Reset();
            _dataUnStake = data;
            _callback = callback;
            _heroId = player.heroId.Id;
            _playerData = player;
            _tokenType = data.TokenType;

            _amountWantUnStake = double.Parse(data.UnStake);
            infoText.SetActive(data.TokenType == RewardType.BCOIN && !player.IsHeroS);
        
            InitIfNeeded();
            UpdateText(data);
        
            base.Show(canvas);
        }
    
        private void InitIfNeeded() {
            if(_isInit)
                return;
            _isInit = true;
        
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _playerStorageManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _networkConfig = ServiceLocator.Instance.Resolve<INetworkConfig>();
        }

        private void UpdateText(DataUnStake data) {
            stakedText.text = data.Staked;
            unStakeText.text = data.UnStake;
            feeText.text = data.Fee + "%";
            totalUnstake.text = data.TotalUnStake;
            stakeRemain.text = data.StakeRemain;
            minValue.text = data.MinValue;
        }

        private async Task<bool> UnStake() {
            var tokenAddress = _tokenType == RewardType.BCOIN
                ? _networkConfig.BlockchainConfig.CoinTokenAddress
                : _networkConfig.BlockchainConfig.SensparkTokenAddress;
            return await _blockchainManager.WithDrawFromHeroId(_heroId, _amountWantUnStake, tokenAddress);
        }

        protected override void OnYesClick() {
            if(_isClicked)
                return;
            _isClicked = true;
            OnBtnConfirm();
        }

        public async void OnBtnConfirm() {
            _soundManager.PlaySound(Audio.Tap);
            IsProcessing(true);
            var result = await UnStake();
            if (result) {
                //Đợi 30s trước khi gọi lên server
                await WebGLTaskDelay.Instance.Delay(30 * 1000);
                await _serverManager.Pve.CheckBomberStake(_dataUnStake.PlayerData.heroId);
            }

            IsProcessing(false);
        
            var dialogResult = await DialogUnStakingResult.Create();
            dialogResult.Show(result, DialogCanvas, _callback.Hide);
            //callback sau khi unstake hoặc stake
            _callback.StakeOrUnStakeComplete?.Invoke();
        
            //Unstake thành công thì kiểm tra xem hero đó có còn đủ stake đê là heroS ko, nếu ko thì ẩn hero đó ra khỏi ds chọn
            var player = _playerStorageManager.GetPlayerDataFromId(_playerData.heroId);
            _callback.UnStakeComplete?.Invoke(player);
        
            Hide();
        }

        #region Processing

        private bool _isProcessing;
        private DialogWaiting _dialogWaiting;

        private void ChangeToProcessing(bool isProcess) {
            _isProcessing = isProcess;
        
            if (_isProcessing) {
                _ = DialogWaiting.Create().ContinueWith(d => {
                    _dialogWaiting = d;
                    _dialogWaiting.Show(DialogCanvas);
                    _dialogWaiting.ShowLoadingAnim();
                });
            } else {
                if (_dialogWaiting != null) {
                    _dialogWaiting.Hide();
                }
            }
        }

        private void IsProcessing(bool value) {
            ChangeToProcessing(value);
            blockPanel.SetActive(value);
        }

        private void Reset() {
            IsProcessing(false);
        }
    
        public void OnBtnHide() {
            if(_isProcessing)
                return;
            _callback.UnStakeHide?.Invoke();
            Hide();
        }
        #endregion
    }
}
