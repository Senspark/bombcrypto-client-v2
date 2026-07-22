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
        private IBHeroManager _bHeroManager;
        private IServerManager _serverManager;
        private ISoundManager _soundManager;
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
            _bHeroManager = ServiceLocator.Instance.Resolve<IBHeroManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        private void UpdateText(DataUnStake data) {
            stakedText.text = data.Staked;
            unStakeText.text = data.UnStake;
            feeText.text = data.Fee + "%";
            totalUnstake.text = data.TotalUnStake;
            stakeRemain.text = data.StakeRemain;
            minValue.text = data.MinValue;
        }

        private async Task<StakeResult> UnStake() {
            var category = _tokenType == RewardType.BCOIN ? StakeHeroCategory.Bcoin : StakeHeroCategory.Sen;
            return await _blockchainManager.WithDrawFromHeroId(_heroId, _amountWantUnStake, category);
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
            if (!result.success) {
                IsProcessing(false);
                return;
            }

#if UNITY_EDITOR
            // Editor không tương tác được với blockchain → yêu cầu server push fake
            // BHERO_STAKE_PUSH với state hiện tại để FarmingSceneStakeObserver hiển thị
            // DialogStakingResult.
            _serverManager.Pve.RequestFakeStakePush(_dataUnStake.PlayerData.heroId);
#else
            // Production: trigger server fetch fresh stake từ ap-blockchain bằng txHash → push BHERO_STAKE_PUSH.
            if (!string.IsNullOrEmpty(result.txHash)) {
                _serverManager.Pve.RefreshHeroStake(_dataUnStake.PlayerData.heroId, result.txHash);
            }
#endif

            IsProcessing(false);

            // Đóng dialog ngay sau khi tx on-chain success — không block UI chờ server.
            _callback.StakeOrUnStakeComplete?.Invoke();
            _callback.UnStakeHide?.Invoke();
            Hide();
        }

        #region Processing

        private bool _isProcessing;
        private DialogWaiting _dialogWaiting;

        private void ChangeToProcessing(bool isProcess) {
            _isProcessing = isProcess;

            if (_isProcessing) {
                _ = DialogWaiting.Create().ContinueWith(d => {
                    // Race: IsProcessing(false) có thể chạy xong trước khi Create resolve
                    // (flow unstake mới không còn 30s wait). Trong trường hợp đó, hide ngay
                    // để không có DialogWaiting orphan mãi trên screen.
                    if (!_isProcessing) {
                        d.Hide();
                        return;
                    }
                    _dialogWaiting = d;
                    _dialogWaiting.Show(DialogCanvas);
                    _dialogWaiting.ShowLoadingAnim();
                });
            } else {
                if (_dialogWaiting != null) {
                    _dialogWaiting.Hide();
                    _dialogWaiting = null;
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
